#define DBGIN

namespace Ofc
{
    using System;
    using System.IO;
    using JetBrains.Annotations;
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
            //*
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

            argumentParser.NewLayer(CommandLineLayers.Compress).AddCommand(e => e.Name("compress").Description("Compresses the specified file or directory.")).AddArgument(e => e.SetName("input").Description("File or directory which will be compressed.")).AddArgument(e => e.SetName("output").Description("Output file or directory where all the compressed data goes.").Optional()).AddOption(e => e.SetShortName('c').Description("Write output to the console.").Optional()).AddOption(e => e.SetShortName('f').Description("Force mode.").Optional());

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

                    case CommandLineLayers.Compress:
                        Compress(result[0], result[1], result['c'], result['f']);
                        Console.WriteLine();
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
                        using (var hook = new BinaryOutputHook<OfcNumber>(stream, algorithm, converter))
                        {
                            // open input stream
                            using (var istream = new FileInputStream(input.OpenText()))
                            {
                                // create a lexer
                                var lexer = new OfcLexer(istream);
                                // create a parser on top of the lexer
                                var parser = new OfcParser(lexer, hook);
                                // parse the file
                                parser.Parse();
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

            /// <summary>
            ///     The Compress layer.
            /// </summary>
            /// <remarks>
            ///     ofc.exe compress &lt;input&gt; [output] [-c] [-f]
            /// </remarks>
            Compress
        }
    }
}