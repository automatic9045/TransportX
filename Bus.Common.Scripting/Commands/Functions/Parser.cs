using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bus.Common.Scripting.Commands.Functions
{
    public class Parser
    {
        private static readonly Regex StringRegex = new Regex("^\"(.*)\"$");
        private static readonly Regex IntegerRegex = new Regex("^[+-]?[0-9]+$");
        private static readonly Regex RealRegex = new Regex(@"^[+-]?[0-9]*\.[0-9]+$");

        private static readonly Regex LiteralEscapingRegex = new Regex(@"\\.");


        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<int, FunctionSignature>> Signatures;

        public Parser(IEnumerable<FunctionSignature> signatures)
        {
            try
            {
                Signatures = signatures
                .GroupBy(signature => signature.Name)
                .ToDictionary(
                    overloads => overloads.Key,
                    overloads => (IReadOnlyDictionary<int, FunctionSignature>)overloads.ToDictionary(overload => overload.ArgTypes.Count));
            }
            catch (ArgumentException)
            {
                FunctionSignature[] overloads = signatures
                    .GroupBy(signature => signature.Name)
                    .Select(overloads => overloads
                        .GroupBy(overload => overload.ArgTypes.Count)
                        .SelectMany(x => x.Skip(1)))
                    .First()
                    .ToArray();

                throw new ArgumentException(
                    $"引数を {overloads[0].ArgTypes.Count} 個持つ '{overloads[0].Name}' 関数が {overloads.Length} 個定義されています。" +
                    $"同名で引数の長さが同じ関数を複数回定義することはできません。", nameof(signatures));
            }
        }

        public Function Parse(string text)
        {
            string? name = null;
            List<string> argTexts = new List<string>();

            StringBuilder buffer = new StringBuilder();
            bool isArgBegun = false;
            bool isInLiteral = false;
            bool isEscapingInLiteral = false;
            bool isArgCompleted = false;
            foreach (char c in text)
            {
                if (name is null)
                {
                    switch (c)
                    {
                        case ' ':
                        case '\t':
                            break;

                        case '(':
                            name = buffer.ToString();
                            buffer.Clear();
                            break;

                        default:
                            if (!(('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || (0 < buffer.Length && '0' <= c && c <= '9')))
                            {
                                throw new FormatException($"関数名として使用できない文字 '{c}' が含まれています。");
                            }

                            buffer.Append(c);
                            break;
                    }
                }
                else
                {
                    if (isInLiteral)
                    {
                        buffer.Append(c);
                        if (isEscapingInLiteral)
                        {
                            isEscapingInLiteral = false;
                        }
                        else
                        {
                            switch (c)
                            {
                                case '\\':
                                    isEscapingInLiteral = true;
                                    break;

                                case '"':
                                    isInLiteral = false;
                                    isArgCompleted = true;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case ' ':
                            case '\t':
                                if (isArgBegun) isArgCompleted = true;
                                break;

                            case ',':
                            case ')':
                                if (!isArgBegun) throw new FormatException($"{argTexts.Count + 1} 個目の引数が指定されていません。");

                                argTexts.Add(buffer.ToString());
                                buffer.Clear();
                                isArgBegun = false;
                                isArgCompleted = false;
                                break;

                            case '"':
                                if (isArgBegun || isArgCompleted) throw new FormatException($"{argTexts.Count + 1} 個目の引数の形式が不正です。");

                                buffer.Append(c);
                                isArgBegun = true;
                                isInLiteral = true;
                                break;

                            default:
                                if (isArgCompleted) throw new FormatException($"{argTexts.Count + 1} 個目の引数の形式が不正です。");

                                isArgBegun = true;
                                buffer.Append(c);
                                break;
                        }
                    }
                }
            }

            name ??= buffer.ToString();
            if (isArgBegun || isInLiteral || isArgCompleted) throw new FormatException("関数が終了していません。");

            List<object> originalArgs = argTexts.ConvertAll<object>(argText =>
            {
                if (RegexMatch(StringRegex, out Match match))
                {
                    return LiteralEscapingRegex.Replace(match.Groups[1].Value, match => match.Value switch
                    {
                        "\\\\" => "\\",
                        "\\\"" => "\"",
                        _ => throw new FormatException($"文字列 '{argText}' 内に不正なエスケープ '{match.Value}' が存在します。"),
                    });
                }
                else if (RegexMatch(IntegerRegex, out match))
                {
                    return int.Parse(match.Value);
                }
                else if (RegexMatch(RealRegex, out match))
                {
                    return float.Parse(match.Value);
                }
                else
                {
                    throw new FormatException($"引数 '{argText}' は不正な形式です。");
                }


                bool RegexMatch(Regex regex, out Match match)
                {
                    match = regex.Match(argText);
                    return match.Success;
                }
            });

            if (!Signatures.TryGetValue(name, out IReadOnlyDictionary<int, FunctionSignature>? overloads))
                throw new FormatException($"'{name}' 関数は定義されていません。");

            if (!overloads.TryGetValue(originalArgs.Count, out FunctionSignature? signature))
                throw new FormatException($"引数の長さが間違っています。{originalArgs.Count} 個の引数が渡されましたが、" +
                    $"正しくは {string.Join(" または ", overloads.Values.Select(overload => overload.ArgTypes.Count))} 個です。");

            object[] args = new object[originalArgs.Count];
            for (int i = 0; i < originalArgs.Count; i++)
            {
                object originalArg = originalArgs[i];
                Type argType = signature.ArgTypes[i];
                try
                {
                    args[i] = Convert.ChangeType(originalArg, argType);
                }
                catch (Exception ex)
                {
                    throw new FormatException(
                        $"{i + 1} 番目の引数の型が間違っています。{originalArg.GetType()} 型の値 '{originalArg}' を {argType} 型に変換できません。", ex);
                }
            }

            return new Function(signature, name, args);
        }
    }
}
