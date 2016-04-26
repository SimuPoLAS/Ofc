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
        ///     Main entrypoint for the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);

            // initiate the parser
            IArgumentParser<CommandLineLayers> argumentParser = new ArgumentParser<CommandLineLayers>();
            argumentParser.Description = "A command line tool for compressing Open Foam files.";
            argumentParser.Name = "ofc.exe";

            // add parser definitions
            argumentParser.NewLayer(CommandLineLayers.Help).AddOption(e => e.SetShortName('h').SetLongName("help").Description("Displays this help message."));
            argumentParser.NewLayer(CommandLineLayers.Version).AddOption(e => e.SetLongName("version").Description("Displays the current version of the tool."));

            argumentParser.NewLayer(CommandLineLayers.CompressDirectory).Command("compress").Command("directory", "Compresses the specified directory.").Argument("input").Argument("output", true).Option('f').Option('r').Option('p');
            argumentParser.NewLayer(CommandLineLayers.CompressFile).Command("compress").Command("file", "Compresses the specified file.").Argument("input").Argument("output", true);

            argumentParser.NewLayer(CommandLineLayers.DecompressDirectory).Command("decompress").Command("directory", "Decompresses the specified compressed directory.").Argument("input").Argument("output", true).Option('f').Option('r').Option('p');
            argumentParser.NewLayer(CommandLineLayers.DecompressFile).Command("decompress").Command("file", "Decompresses the specified compressed file or set of files.").Argument("input").Argument("output", true).Option('f');

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
                            CompressDirectory(result[0], result[1], result['f'], result['r'], result['p']);
                            break;

                        case CommandLineLayers.DecompressFile:
                            DecompressFile(result[0], result[1], result['f']);
                            break;
                        case CommandLineLayers.DecompressDirectory:
                            DecompressDirectory(result[0], result[1], result['f'], result['r'], result['p']);
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

        private static bool CompressFile(string input, string output, bool force)
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

        private static bool CompressDirectory(string input, string output, bool force, bool recursive, bool parallel)
        {
            try
            {
                input = Path.GetFullPath(input);
                output = Path.GetFullPath(output);

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

                // start compression
                try
                {
                    Directory.CreateDirectory(output);

                    Console.WriteLine($"# {input}");

                    // Start paralell work
                    foreach (var e in Directory.EnumerateFiles(input, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        var relative = e.Substring(input.Length);
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


        private static bool DecompressFile(string input, string output, bool force)
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

        private static bool DecompressDirectory(string input, string output, bool force, bool recursive, bool parallel)
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