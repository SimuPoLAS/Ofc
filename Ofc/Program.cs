
#define DBGIN

namespace Ofc
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using LZMA.Core.Helper;
    using Ofc.CommandLine;
    using Ofc.IO;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;
    using OfcAlgorithm.Blocky.Integration;
    using OfcAlgorithm.Integration;

    /// <summary>
    ///     Contains the main entrypoint for the application and the all the CLI functionality.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///     Place where all the logs will be stored.
        /// </summary>
        private const string Logs = "logs/";


        /// <summary>
        ///     Main entrypoint for the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            /**
            using (var source = File.OpenText(@"W:\Documents\GitHub\Ofc\Ofc\bin\dambreak3d.out\dambreak3d\0\alpha1.bin.dat"))
            {
                using (var writer = File.CreateText("out"))
                {
                    using (var stream = File.OpenRead(@"W:\Documents\GitHub\Ofc\Ofc\bin\dambreak3d.out\dambreak3d\0\alpha1.bin"))
                    {
                        var algorithm = new BlockyAlgorithm();
                        var converter = new CompressionDataConverter();
                        using (var reader = new MarerReader<OfcNumber>(source, writer, algorithm, converter, stream))
                        {
                            reader.Do();
                        }
                    }
                }
            }
            return;
            */

            /*
            BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);
            using (var output = File.CreateText("outp"))
            {
                using (var input = File.Open("alpha1.bin", FileMode.Open))
                {
                    var a = new BinaryInputReader<OfcNumber>(input, output, new BlockyAlgorithm(), new CompressionDataConverter());
                    a.Read();
                }
            }

            return;
            //*/

            /* COMPRESSING A DRIECTORY

            var inp = "W:/desktop/dambreak3d/";
            var outp = "W:/desktop/dambreak3d out/";

            var converter = new CompressionDataConverter();
            BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);

            foreach (var file in Directory.EnumerateFiles(inp, "*", SearchOption.AllDirectories))
            {
                var relative = file.Substring(inp.Length);
                var full = Path.Combine(outp, relative + ".bin");

                var success = false;
                try
                {
                    var motherDirectory = Path.GetDirectoryName(full);
                    if (motherDirectory != null) Directory.CreateDirectory(motherDirectory);

                    using (var stream = File.Open(full, FileMode.Create))
                    {
                        using (var bhook = new BinaryOutputHook<OfcNumber>(stream, new BlockyAlgorithm(), converter))
                        {
                            success = ParseHelper.TryParseFile(file, bhook);
                        }
                    }
                }
                catch (Exception)
                {
                    success = false;
                }

                if (!success)
                {
                    File.Delete(full);
                    Console.WriteLine($"Could not compress {relative}.");
                    File.Copy(file, full);
                }
            }

            return;
            */

            BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);

            // initiate the parser
            IArgumentParser<CommandLineLayers> argumentParser = new ArgumentParser<CommandLineLayers>();
            argumentParser.Description = "A command line tool for compressing Open Foam files.";
            argumentParser.Name = "ofc.exe";

            // add parser definitions
            argumentParser.NewLayer(CommandLineLayers.Help).AddOption(e => e.SetShortName('h').SetLongName("help").Description("Displays this help message."));
            argumentParser.NewLayer(CommandLineLayers.Version).AddOption(e => e.SetLongName("version").Description("Displays the current version of the tool."));

            argumentParser.NewLayer(CommandLineLayers.CompressFile).AddCommand(e => e.Name("compress")).AddCommand(e => e.Name("file").Description("Compresses the given file.")).AddArgument(e => e.SetName("input")).AddArgument(e => e.SetName("output").Optional()).AddOption(e => e.SetShortName('f').Description("Force mode."));
            argumentParser.NewLayer(CommandLineLayers.CompressDirectory).AddCommand(e => e.Name("compress")).AddCommand(e => e.Name("directory").Description("Compresses the given directory.")).AddArgument(e => e.SetName("input")).AddArgument(e => e.SetName("output").Optional()).AddOption(e => e.SetShortName('f').Optional().Visibility(ArgumentVisiblility.Usage)).AddOption(e => e.SetShortName('r').Description("Enables recursion on directories.").Optional());

            argumentParser.NewLayer(CommandLineLayers.DecompressFile).AddCommand(e => e.Name("decompress")).AddCommand(e => e.Name("file").Description("Decompresses the specified file.")).AddArgument(e => e.SetName("input")).AddArgument(e => e.SetName("output").Optional()).AddOption(e => e.SetShortName('f').Visibility(ArgumentVisiblility.Usage));
            argumentParser.NewLayer(CommandLineLayers.DecompressDirectory).AddCommand(e => e.Name("decompress")).AddCommand(e => e.Name("directory").Description("Decompresses the specified directory.")).AddArgument(e => e.SetName("input")).AddArgument(e => e.SetName("output").Optional()).AddOption(e => e.SetShortName('f').Visibility(ArgumentVisiblility.Usage)).AddOption(e => e.SetShortName('r').Visibility(ArgumentVisiblility.Usage));

            // parse the arguments
            /*
            
            /*/
#if DBGIN
            while (true)
            {
                Console.Write(" > ");
                var result = argumentParser.Parse(Console.ReadLine() ?? "");
#else
            var result = argumentParser.Parse(args);
#endif

                var ok = false;
                // check if the parser succeeded 
                if (result.Success)
                {
                    ok = true;
                    switch (result.LayerId)
                    {
                        case CommandLineLayers.Help:
                            Console.Write(argumentParser.GenerateHelp());
                            break;
                        case CommandLineLayers.Version:
                            Console.WriteLine($"{argumentParser.Name} [v1.0.000]");
                            break;

                        case CommandLineLayers.CompressFile:
                            CompressFile(result[0], result[1], result['f']);
                            break;
                        case CommandLineLayers.CompressDirectory:
                            CompressDirectory(result[0], result[1], result['f'], result['r']);
                            break;

                        case CommandLineLayers.DecompressFile:
                            DecompressFile(result[0], result[1], result['f']);
                            break;
                        case CommandLineLayers.DecompressDirectory:
                            DecompressDirectory(result[0], result[1], result['f'], result['r']);
                            break;
                    }
                }

                // Write an error message
                if (!ok)
                {
                    Console.WriteLine("Invalid arguments.\n");
                    Console.Write(argumentParser.GenerateHelp());
                }

#if DBGIN
                Console.ReadLine();
#if DNX451
                Console.Clear();
#endif
            }
#endif
        }

        private static bool CompressFile(string input, [CanBeNull] string output, bool force)
        {
            try
            {
                // check if the file exists
                if (!File.Exists(input))
                {
                    Console.WriteLine("File could not be found.");
                    return false;
                }

                // set the output if needed
                if (output == null) output = Path.GetFileName(input) + ".bin";

                // check if threre is an output file
                if (!force && !File.Exists(output))
                {
                    Console.WriteLine("The output file does already exist. Use fore mode (-f) to override it.");
                    return false;
                }

                // start compression
                try
                {
                    // open the output filestream
                    using (var stream = File.Open(output + ".tmp", FileMode.Create))
                    {
                        // parameters for the compression
                        var algorithm = new BlockyAlgorithm();
                        var converter = new CompressionDataConverter();

                        // do the compression
                        try
                        {
                            // create a hook which will handle the internal constructs
                            var hook = new MarerHook<OfcNumber>(algorithm, converter, stream);
                            using (var file = new FileInputStream(input))
                            {
                                var lexer = new OfcLexer(file);
                                var parser = new OfcParser(lexer, hook);
                                hook.PositionProvider = parser;
                                parser.Parse();
                            }

                            // create the data file
                            using (var reader = File.OpenText(input))
                            {
                                using (var ostream = File.CreateText(output + ".dat.tmp"))
                                {
                                    using (var writer = new MarerWriter(reader, ostream, hook.CompressedDataSections))
                                        writer.Do();
                                }
                            }
                            using (var a = File.OpenWrite(Path.ChangeExtension(output, ".dat")))
                            using (var b = File.OpenRead(output + ".dat.tmp"))
                            {
                                Helper.CompressLzma(b, a);
                            }
                            File.Delete(output + ".dat.tmp");
                        }
                            // catch an error from the lexer
                        catch (LexerException ex)
                        {
                            Console.WriteLine("Error while reading the file. [lexing failed]");
                            Console.WriteLine();
                            Console.WriteLine(ex);
                            return false;
                        }
                            // catch an error from the parser
                        catch (ParserException ex)
                        {
                            Console.WriteLine("Error while reading the file. [parsing failed]");
                            Console.WriteLine();
                            Console.WriteLine(ex);
                            return false;
                        }
                            // catch any other error while the parsing happens
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while reading the file. [unknown]");
                            Console.WriteLine();
                            Console.WriteLine(ex);
                            return false;
                        }
                    }
                    using (var a = File.OpenRead(output + ".tmp"))
                    using (var b = File.OpenWrite(output))
                        Helper.CompressLzma(a, b);
                    File.Delete(output + ".tmp");
                }
                    // catch no access error
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("Could not access the output file.");
                    Console.WriteLine();
                    Console.WriteLine(ex);
                    return false;
                }
                    // catch any other arror
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to create and write the output file.");
                    Console.WriteLine();
                    Console.WriteLine(ex);
                    return false;
                }

                // Print a done message.
                Console.WriteLine("File was compressed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error.");
                Console.WriteLine();
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private static bool CompressDirectory(string input, string output, bool force, bool recursive)
        {
            try
            {
                // check if there is an input file
                if (!Directory.Exists(input))
                {
                    Console.WriteLine("Could not find the specified file.");
                    return false;
                }

                // check if threre is an output file
                if (!force && !Directory.Exists(output))
                {
                    Console.WriteLine("The output directory does not exist. Use fore mode (-f) to create it.");
                    return false;
                }

                if (output == null)
                    output = "/";

                // start compression
                try
                {
                    Directory.CreateDirectory(output);

                    // base (root) folder
                    var rUri = new Uri(input);

                    Console.WriteLine($"# {input}");

                    // Start paralell work
                    foreach (var e in Directory.EnumerateFiles(input, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        var mUri = new Uri(e);
                        var relative = rUri.MakeRelativeUri(mUri).ToString();

                        Console.WriteLine($"\n## {relative} [{e.Length}B]");

                        try
                        {
                            var outp = Path.Combine(output, relative);
                            Directory.CreateDirectory(Path.GetDirectoryName(outp));
                            var success = CompressFile(e, outp + ".bin", force);
                            if (!success) // todo 7z lzma
                            {
                                using (var a = File.OpenRead(e))
                                using (var b = File.OpenWrite(outp + ".datu"))
                                    Helper.CompressLzma(a, b);
                                File.Delete(outp + ".bin.tmp");
                                File.Delete(outp + ".bin");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error:");
                            Console.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying compress the specified directory.");
                    Console.WriteLine();
                    Console.WriteLine(ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error.");
                Console.WriteLine();
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }


        private static bool DecompressFile(string input, [CanBeNull] string output, bool force)
        {
            try
            {
                var info = new FileInfo(input);

                // check if the input exists
                if (!info.Exists)
                {
                    Console.WriteLine("Could not find the specified file.");
                    return false;
                }

                // set output
                if (output == null) output = Path.Combine(info.DirectoryName, info.Name.Substring(0, info.Name.Length - info.Extension.Length));

                if (info.Extension == ".dat")
                {
                    var t = Path.ChangeExtension(info.FullName, ".bin");
                    if (!File.Exists(t))
                    {
                        Console.WriteLine("Bin container does not exist for the specified file.");
                        return false;
                    }


                    // lzma .dat
                    using (var istream = info.OpenRead())
                    using (var ostream = File.OpenWrite("temp.dat"))
                        Helper.DecompressFileLzma(istream, ostream);

                    // lzma .bin
                    using (var istream = File.OpenRead(Path.ChangeExtension(info.FullName, ".bin")))
                    using (var ostream = File.OpenWrite("temp.bin"))
                        Helper.DecompressFileLzma(istream, ostream);

                    using (var istream = File.OpenText("temp.dat"))
                    using (var bstream = File.OpenRead("temp.bin"))
                    using (var stream = File.OpenWrite(output))
                    using (var ostream = new StreamWriter(stream))
                    {
                        var algorithm = new BlockyAlgorithm();
                        var converter = new CompressionDataConverter();
                        using (var marer = new MarerReader<OfcNumber>(istream, ostream, algorithm, converter, bstream))
                            marer.Do();
                    }
                    File.Delete("temp.dat");
                    File.Delete("temp.bin");
                    Console.WriteLine($"Successfully decompressed the specified file to: '{output}'");
                }
                else if (info.Extension == ".datu")
                {
                    using (var istream = info.OpenRead())
                    using (var ostream = File.OpenWrite(output))
                        Helper.DecompressFileLzma(istream, ostream);
                    Console.WriteLine($"Successfully decompressed to '{output}'");
                }
                else
                {
                    Console.WriteLine("The target file must have a.dat or.datu extention.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error.");
                Console.WriteLine();
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private static bool DecompressDirectory(string input, [CanBeNull] string output, bool force, bool recursive)
        {
            try
            {
                if (output == null) throw new ArgumentNullException(nameof(output));

                if (!Directory.Exists(input))
                {
                    Console.WriteLine("Can not find the specified directory.");
                    return false;
                }

                input = Path.GetFullPath(input);
                var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                int z = 0, r = 0;
                foreach (var file in Directory.EnumerateFiles(input, "*.dat", option).Union(Directory.EnumerateFiles(input, "*.datu", option)))
                {
                    z++;
                    try
                    {
                        var info = new FileInfo(file);
                        var outp = Path.Combine(output, file.Substring(input.Length));
                        outp = Path.ChangeExtension(outp, "");
                        Directory.CreateDirectory(Path.GetDirectoryName(outp));
                        if (DecompressFile(file, outp, force))
                            r++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                Console.WriteLine($"Directory successfully decompress [{r}/{z} files]");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error.");
                Console.WriteLine();
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        /*
        /// <summary>
        ///     Compresses the specified file or directory and saves the output under the specified path.
        /// </summary>
        /// <param name="input">Input file or directory.</param>
        /// <param name="output">Output file or directory.</param>
        /// <param name="console">When <c>true</c> the logging output will be written to console.</param>
        /// <param name="force">When <c>true</c> Force mode is active.</param>
        private static void Compress(string input, [CanBeNull] string output, bool console, bool force)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException();
            if (string.IsNullOrWhiteSpace(output)) output = null;

            // check if the input is a file or a directory
            try
            {
                if (File.Exists(input))
                {
                    // create file infos
                    var a = new FileInfo(input);
                    var b = new FileInfo(output ?? a.Name + ".bin");

                    if (console)
                    {
                        CompressFile(Console.Out, a, b, force);
                    }
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(Logs);
                            using (var log = File.OpenWrite(Path.Combine(Logs, a.Name + ".log")))
                            {
                                using (var logWriter = new StreamWriter(log))
                                {
                                    CompressFile(logWriter, a, b, force);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CompressFile(Console.Out, a, b, force);
                        }
                    }
                }
                else if (Directory.Exists(input))
                {
                    // create directory infos
                    var a = new DirectoryInfo(input);
                    var b = new DirectoryInfo(output ?? a.Name + ".out");

                    try
                    {
                        CompressDirectory(Console.Out, a, b, console, force, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not compress the directory.");
                    }
                }
                else Console.WriteLine("Could not find the specified file/directory.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid input path.");
            }
        }

        private static bool CompressFile(TextWriter log, FileInfo input, FileInfo output, bool force)
        {
            // check if there is an input file
            if (!input.Exists)
            {
                log.WriteLine("Could not find the specified file.");
                return false;
            }

            // check fi threre is an output file
            if (!force && output.Exists)
            {
                log.WriteLine("The output file already exists. Use fore mode (-f) to override the file.");
                return false;
            }

            // start compression
            try
            {
                // open the output filestream
                using (var stream = output.Open(FileMode.Create))
                {
                    // parameters for the compression
                    var algorithm = new BlockyAlgorithm();
                    var converter = new CompressionDataConverter();

                    // do the compression
                    try
                    {
                        // create a hook which will handle the internal constructs
                        var hook = new MarerHook<OfcNumber>(algorithm, converter, stream);
                        using (var file = new FileInputStream(input.FullName))
                        {
                            var lexer = new OfcLexer(file);
                            var parser = new OfcParser(lexer, hook);
                            hook.PositionProvider = parser;
                            parser.Parse();
                        }

                        // create the data file
                        using (var reader = File.OpenText(input.FullName))
                        {
                            using (var ostream = File.CreateText(output.FullName + ".dat"))
                            {
                                using (var writer = new MarerWriter(reader, ostream, hook.CompressedDataSections))
                                    writer.Do();
                            }
                        }
                    }
                        // catch an error from the lexer
                    catch (LexerException ex)
                    {
                        log.WriteLine("Error while reading the file. [lexing failed]");
                        log.WriteLine();
                        log.WriteLine(ex);
                        return false;
                    }
                        // catch an error from the parser
                    catch (ParserException ex)
                    {
                        log.WriteLine("Error while reading the file. [parsing failed]");
                        log.WriteLine();
                        log.WriteLine(ex);
                        return false;
                    }
                        // catch any other error while the parsing happens
                    catch (Exception ex)
                    {
                        log.WriteLine("Error while reading the file. [unknown]");
                        log.WriteLine();
                        log.WriteLine(ex);
                        return false;
                    }
                }
            }
                // catch no access error
            catch (UnauthorizedAccessException ex)
            {
                log.WriteLine("Could not access the output file.");
                log.WriteLine();
                log.WriteLine(ex);
                return false;
            }
                // catch any other arror
            catch (Exception ex)
            {
                log.WriteLine("Error while trying to create and write the output file.");
                log.WriteLine();
                log.WriteLine(ex);
                return false;
            }

            // Print a done message.
            Console.WriteLine("File was compressed successfully");
            return true;
        }

        private static bool CompressDirectory(TextWriter log, DirectoryInfo root, DirectoryInfo outputRoot, bool console, bool force, bool recursive = false)
        {
            // check if there is an input file
            if (!root.Exists)
            {
                log.WriteLine("Could not find the specified file.");
                return false;
            }

            // check fi threre is an output file
            if (!force && !outputRoot.Exists)
            {
                log.WriteLine("The output directory does not exist. Use fore mode (-f) to create it.");
                return false;
            }

            // start compression
            try
            {
                Directory.CreateDirectory(outputRoot.FullName);

                // base (root) folder
                var rUri = new Uri(root.FullName);

                log.WriteLine($"# {root.FullName}");

                // Start paralell work
                foreach (var e in root.EnumerateFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    var mUri = new Uri(e.FullName);
                    var relative = rUri.MakeRelativeUri(mUri).ToString();

                    log.WriteLine($"\n## {relative} [{e.Length}B]");

                    try
                    {
                        var outp = Path.Combine(outputRoot.FullName, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(outp));
                        var success = CompressFile(log, e, new FileInfo(outp + ".bin"), force);
                        if (!success) e.CopyTo(outp, true);
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("Error:");
                        log.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Error while trying compress the specified directory.");
                log.WriteLine();
                log.WriteLine(ex);
                return false;
            }
            return true;
        }
        */

        /// <summary>
        ///     Represents a layer in the argument parser.
        /// </summary>
        internal enum CommandLineLayers
        {
            /// <summary>
            ///     The Help layer.
            /// </summary>
            /// <remarks>
            ///     ofc.exe [-h|--help]
            /// </remarks>
            Help,

            /// <summary>
            ///     The Version layer.
            /// </summary>
            /// <remarks>
            ///     ofc.exe [--version]
            /// </remarks>
            Version,

            CompressFile,

            CompressDirectory,

            DecompressFile,

            DecompressDirectory
        }
    }
}