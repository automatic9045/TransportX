using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data
{
    public partial class XmlSerializable
    {
        public class ReadContext
        {
            private readonly XmlSerializable Source;
            private readonly Dictionary<string, ReadElementAction> ReadElementActions = [];

            public XmlReader Reader { get; }
            public bool IsEmptyElement { get; }
            public string Location { get; }

            public ReadContext(XmlSerializable source, XmlReader reader, bool isEmptyElement, string location)
            {
                Source = source;
                Reader = reader;
                IsEmptyElement = isEmptyElement;
                Location = location;
            }

            private FieldInfo GetField<T>(string elementName)
            {
                FieldInfo field = Source.Fields[elementName];

                Type type = field.FieldType.GenericTypeArguments[0];
                if (type != typeof(T)) throw new InvalidOperationException($"フィールド '{elementName}' に型 '{typeof(T).Name}' を代入できません。");

                return field;
            }

            private static bool TryParseStandardValue<T>(string source, out T result)
            {
                Type type = typeof(T);
                if (type == typeof(string))
                {
                    result = (T)(object)source;
                    return true;
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(source, CultureInfo.InvariantCulture, out int intValue))
                    {
                        result = (T)(object)intValue;
                        return true;
                    }
                }
                else if (type == typeof(float))
                {
                    if (float.TryParse(source, CultureInfo.InvariantCulture, out float floatValue))
                    {
                        result = (T)(object)floatValue;
                        return true;
                    }
                }
                else if (type == typeof(double))
                {
                    if (double.TryParse(source, CultureInfo.InvariantCulture, out double floatValue))
                    {
                        result = (T)(object)floatValue;
                        return true;
                    }
                }
                else if (type.IsEnum)
                {
                    if (Enum.TryParse(type, source, out object? enumValue) && enumValue is not null)
                    {
                        result = (T)enumValue;
                        return true;
                    }
                }

                result = default!;
                return false;
            }

            public void ReadAttribute<TSource, TTarget>(string elementName, Converter<TSource, TTarget> converter, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                string? sourceString = Reader.GetAttribute(elementName);
                if (sourceString is null)
                {
                    if (isRequired) ReportValueNotDefinedError(displayName);
                    return;
                }

                FieldInfo field = GetField<TTarget>(elementName);

                if (TryParseStandardValue(sourceString, out TSource parsedSource))
                {
                    try
                    {
                        TTarget targetValue = converter(parsedSource);
                        field.SetValue(Source, CreateValue(targetValue, (IXmlLineInfo)Reader));
                    }
                    catch
                    {
                        ReportInvalidValueError(displayName, sourceString);
                    }
                }
                else
                {
                    ReportInvalidValueError(displayName, sourceString);
                }
            }

            public void ReadAttribute<T>(string elementName, string? displayName = null, bool isRequired = false)
            {
                ReadAttribute<T, T>(elementName, x => x, displayName, isRequired);
            }

            public void AddElement<TSource, TTarget>(string elementName, Converter<TSource, TTarget> converter, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                FieldInfo field = GetField<TTarget>(elementName);
                ReadElementActions[elementName] = new ReadElementAction(Read, displayName, isRequired);


                void Read(IXmlLineInfo lineInfo)
                {
                    string sourceString = Reader.ReadElementContentAsString();

                    if (TryParseStandardValue<TSource>(sourceString, out TSource parsedSource))
                    {
                        try
                        {
                            TTarget targetValue = converter(parsedSource);
                            field.SetValue(Source, CreateValue(targetValue, lineInfo));
                        }
                        catch
                        {
                            ReportInvalidValueError(displayName, sourceString);
                        }
                    }
                    else
                    {
                        ReportInvalidValueError(displayName, sourceString);
                    }
                }

            }

            public void AddElement<T>(string elementName, string? displayName = null, bool isRequired = false)
            {
                AddElement<T, T>(elementName, x => x, displayName, isRequired);
            }

            public void AddSerializedElement<T>(string elementName, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                FieldInfo? field = Source.GetType().GetField(elementName) ?? throw new InvalidOperationException($"フィールド '{elementName}' が見つかりません。");
                if (!field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    throw new InvalidOperationException($"フィールド '{elementName}' に型 '{typeof(T).Name}' を代入できません。");
                }

                ReadElementActions[elementName] = new ReadElementAction(
                    _ => ReadSerializedElementCore<T>(elementName, displayName, obj => field.SetValue(Source, obj)),
                    displayName, isRequired);
            }

            public void AddSerializedListElement<TItem>(string elementName, string listFieldName, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                FieldInfo? field = Source.GetType().GetField(listFieldName) ?? throw new InvalidOperationException($"フィールド '{listFieldName}' が見つかりません。");

                if (!typeof(IList<TItem>).IsAssignableFrom(field.FieldType))
                {
                    throw new InvalidOperationException($"フィールド '{listFieldName}' に '{typeof(TItem).Name}' のリストを代入できません。");
                }

                IList<TItem>? list = (IList<TItem>?)field.GetValue(Source);
                if (list is null)
                {
                    list = [];
                    field.SetValue(Source, list);
                }

                ReadElementActions[elementName] = new ReadElementAction(
                    _ => ReadSerializedElementCore<TItem>(elementName, displayName, obj => list.Add(obj)),
                    displayName, isRequired);
            }

            private void ReadSerializedElementCore<T>(string elementName, string displayName, Action<T> onSuccess)
            {
                try
                {
                    XmlSerializer serializer = SerializerCache.GetOrAdd((typeof(T), elementName),
                        key => new XmlSerializer(key.Type, new XmlRootAttribute(key.ElementName)));

                    bool wasEmpty = Reader.IsEmptyElement;
                    using (XmlReader subReader = Reader.ReadSubtree())
                    {
                        subReader.MoveToContent();

                        if (serializer.Deserialize(subReader) is T deserializedObject)
                        {
                            onSuccess(deserializedObject);
                        }
                    }

                    if (wasEmpty)
                    {
                        Reader.Read();
                    }
                    else if (Reader.NodeType == XmlNodeType.EndElement)
                    {
                        Reader.ReadEndElement();
                    }
                }
                catch (Exception ex)
                {
                    ReportError($"{displayName}の値は無効です。", ex);
                }
            }

            internal void ParseElements()
            {
                Reader.ReadStartElement();

                if (IsEmptyElement)
                {
                    CheckRequiredElements();
                    return;
                }

                Reader.MoveToContent();

                while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
                {
                    if (Reader.NodeType == XmlNodeType.Element)
                    {
                        string localName = Reader.LocalName;

                        if (ReadElementActions.TryGetValue(localName, out ReadElementAction? action))
                        {
                            IXmlLineInfo lineInfo = (IXmlLineInfo)Reader;
                            action.Invoke(lineInfo);
                        }
                        else
                        {
                            Reader.Skip();
                        }
                    }
                    else
                    {
                        Reader.Read();
                    }

                    Reader.MoveToContent();
                }

                CheckRequiredElements();


                void CheckRequiredElements()
                {
                    foreach (ReadElementAction action in ReadElementActions.Values)
                    {
                        if (!action.IsRequiredElementSet)
                        {
                            ReportValueNotDefinedError(action.DisplayName);
                        }
                    }
                }
            }

            private XmlValue<T> CreateValue<T>(T value, IXmlLineInfo lineInfo)
            {
                return new XmlValue<T>(value, Location, lineInfo.LineNumber, lineInfo.LinePosition);
            }

            private void ReportError(string message, Exception? exception = null)
            {
                IXmlLineInfo lineInfo = (IXmlLineInfo)Reader;
                Error error = new(ErrorLevel.Error, message, Location)
                {
                    Exception = exception,
                    LineNumber = lineInfo.LineNumber,
                    LinePosition = lineInfo.LinePosition,
                };
                Source.ErrorsKey.Add(error);
            }

            private void ReportValueNotDefinedError(string displayName)
            {
                ReportError($"{displayName}が定義されていません。");
            }

            private void ReportInvalidValueError(string displayName, string value)
            {
                ReportError($"{displayName} '{value}' は無効です。");
            }


            private readonly struct LineInfoSnapshot : IXmlLineInfo
            {
                public int LineNumber { get; }
                public int LinePosition { get; }

                public LineInfoSnapshot(IXmlLineInfo source)
                {
                    LineNumber = source.LineNumber;
                    LinePosition = source.LinePosition;
                }

                public readonly bool HasLineInfo() => true;
            }

            private class ReadElementAction
            {
                private readonly Action<IXmlLineInfo> Action;

                public string DisplayName { get; }
                public bool IsRequiredElementSet{ get; private set; }

                public ReadElementAction(Action<IXmlLineInfo> action, string displayName, bool isRequired)
                {
                    Action = action;
                    DisplayName = displayName;
                    IsRequiredElementSet = !isRequired;
                }

                public void Invoke(IXmlLineInfo lineInfo)
                {
                    Action.Invoke(lineInfo);
                    IsRequiredElementSet = true;
                }
            }
        }
    }
}
