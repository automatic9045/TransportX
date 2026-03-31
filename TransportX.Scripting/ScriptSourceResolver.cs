using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace TransportX.Scripting
{
    public partial class ScriptSourceResolver : SourceReferenceResolver
    {
        [GeneratedRegex(@"^[ \t]*#load[ \t]+""(?:[^""]*[/\\])?__Editor\.csx""[ \t]*",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex LoadForEditorRegex();


        private readonly SourceReferenceResolver DefaultResolver;

        public ScriptSourceResolver(SourceReferenceResolver defaultResolver)
        {
            DefaultResolver = defaultResolver;
        }

        public override bool Equals(object? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => DefaultResolver.GetHashCode();
        public override string? NormalizePath(string path, string? baseFilePath) => DefaultResolver.NormalizePath(path, baseFilePath);

        public override string? ResolveReference(string reference, string? baseFilePath)
        {
            return DefaultResolver.ResolveReference(reference, baseFilePath);
        }

        public override Stream OpenRead(string resolvedPath)
        {
            if (Path.GetFileName(resolvedPath).Equals("__Editor.csx", StringComparison.InvariantCultureIgnoreCase)) return new MemoryStream();

            Stream originalStream = DefaultResolver.OpenRead(resolvedPath);

            using StreamReader reader = new(originalStream);
            string rawContent = reader.ReadToEnd();

            string content = LoadForEditorRegex().Replace(rawContent, string.Empty);
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}
