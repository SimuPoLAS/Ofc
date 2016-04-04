namespace Ofc.Util
{
    using System;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.IO;
    using Ofc.Parsing;

    internal static class ParseHelper
    {
        internal static void ParseFile(string target, [CanBeNull] IParserHook<string> hook)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (!File.Exists(target)) throw new FileNotFoundException("Could not find the specified file.", target);
            var stream = new FileInputStream(target);
            var lexer = new OfcLexer(stream, true);
            var parser = new OfcParser(lexer, hook);
            parser.Parse();
        }

        internal static bool TryParseFile(string target, [CanBeNull] IParserHook<string> hook)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            try
            {
                ParseFile(target, hook);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void ParseDirectory(string target, [CanBeNull] IParserHook<string> hook, bool recursive)
        {
            foreach (var file in Directory.EnumerateFiles(target, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                ParseFile(file, hook);
        }

        internal static int TryParseDirectory(string target, [CanBeNull] IParserHook<string> hook, bool recursive)
        {
            var a = 0;
            foreach (var file in Directory.EnumerateFiles(target, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                if (TryParseFile(file, hook))
                    a++;
            return a;
        }
    }
}