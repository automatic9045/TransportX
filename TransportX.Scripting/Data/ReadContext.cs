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
            private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializerCache = [];


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
                if (type != typeof(T)) throw new InvalidOperationException("型引数がフィールドの型と一致しません。");

                return field;
            }

            private void SetToField<T>(FieldInfo field, string source, string displayName, IXmlLineInfo lineInfo)
            {
                Type type = field.FieldType.GenericTypeArguments[0];
                if (type == typeof(string))
                {
                    field.SetValue(Source, CreateValue((string?)source, lineInfo));
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(source, CultureInfo.InvariantCulture, out int value))
                    {
                        field.SetValue(Source, CreateValue(value, lineInfo));
                    }
                    else
                    {
                        ReportInvalidValueError(displayName, source);
                    }
                }
                else if (type == typeof(float))
                {
                    if (float.TryParse(source, CultureInfo.InvariantCulture, out float value))
                    {
                        field.SetValue(Source, CreateValue(value, lineInfo));
                    }
                    else
                    {
                        ReportInvalidValueError(displayName, source);
                    }
                }
                else if (type.IsEnum)
                {
                    if (Enum.TryParse(type, source, out object? value))
                    {
                        field.SetValue(Source, CreateValue((T)value, lineInfo));
                    }
                    else
                    {
                        ReportInvalidValueError(displayName, source);
                    }
                }
                else
                {
                    throw new NotSupportedException($"型 {type} はサポートされません。");
                }
            }

            public void ReadAttribute<T>(string elementName, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                string? source = Reader.GetAttribute(elementName);
                if (source is null)
                {
                    if (isRequired) ReportValueNotDefinedError(displayName);
                    return;
                }

                FieldInfo field = GetField<T>(elementName);
                SetToField<T>(field, source, displayName, (IXmlLineInfo)Reader);
            }

            public void AddElement<T>(string elementName, string? displayName = null, bool isRequired = false)
            {
                displayName ??= elementName;

                FieldInfo field = GetField<T>(elementName);
                Type type = typeof(T);

                Action<IXmlLineInfo> readAction;
                if (typeof(IXmlSerializable).IsAssignableFrom(type))
                {
                    readAction = lineInfo =>
                    {
                        IXmlSerializable instance = (IXmlSerializable)Activator.CreateInstance<T>()!;
                        instance.ReadXml(Reader);
                        field.SetValue(Source, CreateValue(instance, lineInfo));
                    };
                }
                else if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
                {
                    readAction = lineInfo =>
                    {
                        string value = Reader.ReadElementContentAsString();
                        SetToField<T>(field, value, displayName, lineInfo);
                    };
                }
                else
                {
                    readAction = lineInfo =>
                    {
                        XmlSerializer serializer = SerializerCache.GetOrAdd(type, t => new XmlSerializer(t));
                        object? result = serializer.Deserialize(Reader);
                        field.SetValue(Source, CreateValue((T)result!, lineInfo));
                    };
                }

                ReadElementActions[elementName] = new(readAction, displayName, isRequired);
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
                            LineInfoSnapshot lineInfoSnapshot = new((IXmlLineInfo)Reader);
                            action.Invoke(lineInfoSnapshot);
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

            private void ReportError(string message)
            {
                IXmlLineInfo lineInfo = (IXmlLineInfo)Reader;
                Error error = new(ErrorLevel.Error, message, Location)
                {
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
