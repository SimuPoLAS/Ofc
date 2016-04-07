namespace Ofc.Util
{
    using System;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.IO;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;
    using OfcAlgorithm.Blocky.Integration;
    using OfcAlgorithm.Integration.Dummy;
    using OfcAlgorithm.Rounding;
    using OfcCore.Configurations;

    /// <summary>
    ///     Provides a set of method which speed up general parsing tasks.
    /// </summary>
    internal static class ParseHelper
    {
        /// <summary>
        ///     Parses the specified file with the specified hook.
        /// </summary>
        /// <param name="target">Target file.</param>
        /// <param name="hook">Target hook used when parsing.</param>
        /// <remarks>
        ///     If no hook is specified <see cref="EmptyHook{T}.Instance" /> will be use.
        /// </remarks>
        internal static void ParseFile(string target, [CanBeNull] IParserHook<string> hook)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (!File.Exists(target)) throw new FileNotFoundException("Could not find the specified file.", target);
            using (var stream = new FileInputStream(target))
            {
                var lexer = new OfcLexer(stream, true);
                var parser = new OfcParser(lexer, hook ?? EmptyHook<string>.Instance);
                parser.Parse();
            }
        }

        /// <summary>
        ///     Parses the specified file with the specified hook and returns if the parsing was successful.
        /// </summary>
        /// <param name="target">Target file.</param>
        /// <param name="hook">Target hook used when parsing.</param>
        /// <remarks>
        ///     If no hook is specified <see cref="EmptyHook{T}.Instance" /> will be use.
        /// </remarks>
        /// <returns>Returns <c>true</c> when the parsing was successful otherwise <c>false</c>.</returns>
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

        /// <summary>
        ///     Parses the specified directory with the specified hook.
        /// </summary>
        /// <param name="target">Target source file.</param>
        /// <param name="hook">Target hook used by when parsing.</param>
        /// <param name="recursive">
        ///     If <c>true</c> all file in the directory will be compressed otherwise only top-level files will
        ///     be compressed.
        /// </param>
        internal static void ParseDirectory(string target, [CanBeNull] IParserHook<string> hook, bool recursive)
        {
            foreach (var file in Directory.EnumerateFiles(target, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                ParseFile(file, hook);
        }

        /// <summary>
        ///     Parses the specified directory with the specified hook and returns if the amount of files that were successful.
        /// </summary>
        /// <param name="target">Target source file.</param>
        /// <param name="hook">Target hook used by when parsing.</param>
        /// <param name="recursive">
        ///     If <c>true</c> all file in the directory will be compressed otherwise only top-level files will
        ///     be compressed.
        /// </param>
        /// <returns>The amount of files that were successfully parsed.</returns>
        internal static int TryParseDirectory(string target, [CanBeNull] IParserHook<string> hook, bool recursive)
        {
            var a = 0;
            foreach (var file in Directory.EnumerateFiles(target, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                if (TryParseFile(file, hook))
                    a++;
            return a;
        }

        #region TESTING ALGORITHM METHODS

        internal static void CompressFolderBlocky(string srcPath, string targetPath)
        {
            using (new FileStream(targetPath, FileMode.Create))
            {
            }
            foreach (var file in Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories))
            {
                CompressFileBlocky(file, targetPath, FileMode.Append);
            }
        }

        internal static void CompressFolderBlockyWithRounding(string srcPath, string targetPath, double min, double max, double epsilon)
        {
            BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);
            using (new FileStream(targetPath, FileMode.Create))
            {
            }
            foreach (var file in Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories))
            {
                CompressFileBlockyWithRounding(file, targetPath, min, max, epsilon, FileMode.Append);
            }
        }

        internal static void CompressFileBlocky(string srcPath, string targetPath, FileMode mode = FileMode.Create)
        {
            using (var str = new FileStream(targetPath, mode))
            {
                BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);
                try
                {
                    var lexi = new OfcLexer(new FileInputStream(srcPath));
                    var purser = new OfcParser(lexi, new AlgorithmHook(new BlockyAlgorithm(), str, EmptyConfiguration.Instance));
                    purser.Parse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(srcPath + ": " + ex);
                }
            }
        }

        internal static void CompressFileBlockyWithRounding(string srcPath, string targetPath, double min, double max, double epsilon, FileMode mode = FileMode.Create)
        {
            var config = new SimpleConfiguration {["RoundingMin"] = min, ["RoundingMax"] = max, ["RoundingEpsilon"] = epsilon};

            using (var str = new FileStream(targetPath, mode))
            {
                try
                {
                    var lexi = new OfcLexer(new FileInputStream(srcPath));
                    var purser = new OfcParser(lexi, new AlgorithmHook(new RounderAlgorithm(new BlockyAlgorithm()), str, config));
                    purser.Parse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(srcPath + ": " + ex);
                }
            }
        }

        internal static void DecompressFileBlocky(string srcPath, string targetPath)
        {
            using (var str = new FileStream(srcPath, FileMode.Open))
            {
                var item = 0;
                while (str.Position < str.Length - 1)
                {
                    new BlockyAlgorithm().Decompress(null, EmptyConfiguration.Instance, str, new DummyReporter(Path.GetDirectoryName(targetPath) + "\\" + item++ + "." + Path.GetFileName(targetPath)));
                }
            }
        }

#endregion
    }
}