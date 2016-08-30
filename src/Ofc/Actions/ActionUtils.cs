namespace Ofc.Actions
{
    using System;
    using System.IO;
    using Core;

    internal static class ActionUtils
    {
        /** Extentions for the different file types */
        internal const string MetaFileExtention = ".meta";
        internal const string DataFileExtention = ".dat";
        internal const string UncompressedFileExtention = ".lzma";
        internal const string TempFileExtention = ".tmp";


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


        internal static void AddCompressDirectoryAction<TAlgorithm>(this OfcActionManager manager, IAlgorithm<TAlgorithm> algorithm, IConverter<TAlgorithm> converter, IConfiguaration config, string baseInputDirectory, string baseOutputDirectory, bool recursive)
        {
            baseInputDirectory = Path.GetFullPath(baseInputDirectory);
            baseOutputDirectory = Path.GetFullPath(baseOutputDirectory);
            foreach (var file in Directory.EnumerateFiles(baseInputDirectory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                var relativePath = file.StartsWith(baseInputDirectory) ? file.Substring(baseInputDirectory.Length) : file;
                if (relativePath.StartsWith("/") || relativePath.StartsWith("\\")) relativePath = relativePath.Substring(1);
                var outputPath = Path.Combine(baseOutputDirectory, relativePath);
                manager.Enqueue(new CompressAction<TAlgorithm>(algorithm, converter, config, baseInputDirectory, file, outputPath + DataFileExtention, outputPath + MetaFileExtention, outputPath + UncompressedFileExtention));
            }
        }

        internal static void AddCompressFileAction<TAlgorithm>(this OfcActionManager manager, IAlgorithm<TAlgorithm> algorithm, IConverter<TAlgorithm> converter, IConfiguaration config, string source, string destination)
        {
            manager.Enqueue(new CompressAction<TAlgorithm>(algorithm, converter, config, null, source, destination + DataFileExtention, destination + MetaFileExtention, destination + UncompressedFileExtention));
        }

        internal static void AddDecompressDirectoryAction<TAlgorithm>(this OfcActionManager manager, IAlgorithm<TAlgorithm> algorithm, IConverter<TAlgorithm> converter, string baseInputDirectory, string baseOutputDirectory, bool recursive)
        {
            baseInputDirectory = Path.GetFullPath(baseInputDirectory);
            baseOutputDirectory = Path.GetFullPath(baseOutputDirectory);
            foreach (var file in Directory.EnumerateFiles(baseInputDirectory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                var relativePath = file.StartsWith(baseInputDirectory) ? file.Substring(baseInputDirectory.Length) : file;
                if (relativePath.StartsWith("/") || relativePath.StartsWith("\\")) relativePath = relativePath.Substring(1);

                var extention = Path.GetExtension(file);
                if (extention != MetaFileExtention && extention != UncompressedFileExtention)
                {
                    if (extention == DataFileExtention) continue;
                    Console.WriteLine($" WARNING: unhandled file '{relativePath}'");
                    continue;
                }

                var compressed = extention == MetaFileExtention;
                var outputPath = Path.Combine(baseOutputDirectory, Path.ChangeExtension(relativePath, null));
                if (compressed)
                {
                    var dataPath = Path.ChangeExtension(file, DataFileExtention);
                    manager.Enqueue(new DecompressAction<TAlgorithm>(algorithm, converter, baseInputDirectory, file, File.Exists(dataPath) ? dataPath : null, false, outputPath));
                }
                else manager.Enqueue(new DecompressAction<TAlgorithm>(algorithm, converter, baseInputDirectory, file, null, true, outputPath));
            }
        }

        internal static void AddDecompressFileAction<TAlgorithm>(this OfcActionManager manager, IAlgorithm<TAlgorithm> algorithm, IConverter<TAlgorithm> converter, string metaSource, string dataSource, string destination)
        {
            manager.Enqueue(new DecompressAction<TAlgorithm>(algorithm, converter, null, metaSource, dataSource, Path.GetExtension(metaSource) == UncompressedFileExtention, destination));
        }
    }
}