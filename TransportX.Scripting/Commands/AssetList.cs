using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using TransportX.Collections;
using TransportX.Diagnostics;

namespace TransportX.Scripting.Commands
{
    internal class AssetList : IDisposable
    {
        private readonly int[] ValidColumnLengths;
        private readonly IErrorCollector ErrorCollector;
        private readonly StreamReader Reader = StreamReader.Null;

        public string ListPath { get; } = string.Empty;
        public string ListDirectory { get; } = string.Empty;
        public int LineNumber { get; private set; } = 0;
        public bool IsValid { get; } = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public AssetList(string path, string defaultBaseDirectory, string name, int[] validColumnLengths, IErrorCollector errorCollector)
        {
            ValidColumnLengths = validColumnLengths;
            ErrorCollector = errorCollector;

            string? baseDir = BaseDirectory.Find(4) ?? defaultBaseDirectory;
            ListPath = Path.GetFullPath(Path.Combine(baseDir, path));
            if (!File.Exists(ListPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"{name} '{ListPath}' が見つかりませんでした。");
                ErrorCollector.Report(error);
                return;
            }

            try
            {
                Reader = new StreamReader(ListPath);
                ListDirectory = Path.GetDirectoryName(ListPath)!;
                IsValid = true;
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"{name} '{ListPath}' を読み込めませんでした。");
                ErrorCollector.Report(error);
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
        }

        public bool ReadLine(out string[] line)
        {
            while (!Reader.EndOfStream)
            {
                LineNumber++;
                string? rawText = Reader.ReadLine();
                if (rawText is null) break;

                string[] rawLine = rawText.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (rawLine.Length == 0) continue;
                if (rawLine[0].StartsWith('#')) continue;

                int columnCount = 0;
                for (int i = 0; i < rawLine.Length; i++)
                {
                    int commentIndex = rawLine[i].IndexOf('#');
                    if (0 <= commentIndex)
                    {
                        string token = rawLine[i][..commentIndex].TrimEnd();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            rawLine[i] = token;
                            columnCount = i + 1;
                        }
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(rawLine[i]))
                    {
                        columnCount = i + 1;
                    }
                }

                if (columnCount == 0) continue;

                if (!ValidColumnLengths.Contains(columnCount))
                {
                    Error error = new(ErrorLevel.Error, $"レコード '{rawLine[0]}' の引数の長さが正しくありません。", ListPath)
                    {
                        LineNumber = LineNumber,
                    };
                    ErrorCollector.Report(error);
                    continue;
                }

                if (columnCount == rawLine.Length)
                {
                    line = rawLine;
                }
                else
                {
                    line = rawLine.AsSpan(0, columnCount).ToArray();
                }
                return true;
            }

            line = [];
            return false;
        }
    }
}
