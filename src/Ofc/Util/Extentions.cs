namespace Ofc.Util
{
    using System;
    using System.IO;
    using Ofc.Actions;

    internal static class Extentions
    {
        internal static string GetSymbolForResult(this OfcActionResult result)
        {
            switch (result)
            {
                case OfcActionResult.Done:
                    return " ";
                case OfcActionResult.Unkown:
                    return "?";
                case OfcActionResult.Fatal:
                    return "F";
                case OfcActionResult.Lzma:
                    return ".";
                default:
                    return "E";
            }
        }

        internal static void AddDirectory(this OfcActionManager manager, string baseInputDirectory, string baseOutputDirectory, bool recursive)
        {
            baseInputDirectory = Path.GetFullPath(baseInputDirectory);
            baseOutputDirectory = Path.GetFullPath(baseOutputDirectory);
            foreach (var file in Directory.EnumerateFiles(baseInputDirectory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                var relativePath = file.Substring(baseInputDirectory.Length);
                if (relativePath.StartsWith("/") || relativePath.StartsWith("\\")) relativePath = relativePath.Substring(1);
                var outputPath = Path.Combine(baseOutputDirectory, relativePath);
                manager.Enqueue(new CompressAction(baseInputDirectory, file, outputPath + ".bin", outputPath + ".dat", outputPath + ".datu"));
            }
        }

        internal static void AddFile(this OfcActionManager manager, string source, string destination)
        {
            manager.Enqueue(new CompressAction(string.Empty, source, destination + ".bin", destination + ".dat", destination + ".datu"));
        }

        /// <summary>
        /// Converts a bool array in a binary presentation, where true is a 1 and false is a 0
        /// </summary>
        /// <param name="bits"></param>
        /// <returns>A tuple, where item1 is the encoded int, and byte is the bit count</returns>
        internal static Tuple<int, byte> GetBinaryPresentation(bool[] bits)
        {
            var num = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < bits.Length; i++)
                num = (num << 1) | (bits[i] ? 1 : 0);
            return new Tuple<int, byte>(num, (byte)bits.Length);
        }
    }
}