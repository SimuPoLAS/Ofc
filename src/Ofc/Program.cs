/**
 * ~~~ FRIENDLY REMINDER ~~~
 * Please do not sync your own testing code in Program.cs (or in any other file for that matter)
 * Just DONT
 * And if you are so malicious and do so - AT LEAST REMOVE IT AFTERWARDS 
 * 
 * Zankyou no terror
 * - a-ctor
 * */

namespace Ofc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Actions;
    using Algorithm.Blocky.Integration;
    using Algorithm.Zetty;
    using CLI;
    using CLI.Validators;
    using Core;
    using Core.Configurations;
    using JetBrains.Annotations;
    using Util.Converters;

    /// <summary>
    ///   Contains the main entrypoint for the application and the all the CLI functionality.
    /// </summary>
    [UsedImplicitly]
    internal static class Program
    {
        /// <summary>
        ///   Main entrypoint for the application.
        /// </summary>
        /// <param name="args"></param>
        [UsedImplicitly]
        public static void Main(string[] args)
        {
            // debug stuff
#if DEBUG
            if (args.Length == 0)
            {
                // input for arguments
                Console.Write("args: ");
                var input = Console.ReadLine();
                Console.Clear();

                // string builder for string operations
                var sb = new StringBuilder();

                // if not empty
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var rargs = new List<string>();
                    var inLiteral = false;
                    for (var i = 0; i < input.Length; i++)
                    {
                        var c = input[i];
                        if (inLiteral)
                        {
                            if (c == '"')
                                if (i != input.Length - 1 && input[i + 1] == '"') sb.Append('"');
                                else inLiteral = false;
                            else sb.Append(c);
                        }
                        else
                        {
                            if (char.IsWhiteSpace(c))
                            {
                                if (sb.Length == 0) continue;
                                rargs.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            else if (c == '"') inLiteral = true;
                            else sb.Append(c);
                        }
                    }
                    if (sb.Length != 0) rargs.Add(sb.ToString());
                    args = rargs.ToArray();
                }

                // provide info
                Console.WriteLine($"calling with {args.Length} arguments");

                // show the arguments that will be passed over
                sb.Length = 0;
                sb.Append('[');
                var l = args.Length;
                for (var i = 0; i < l; i++)
                    sb.Append($"\"{args[i]}\"{(i != l - 1 ? ", " : string.Empty)}");
                sb.Append(']');
                Console.WriteLine(sb.ToString());
                Console.WriteLine();
            }
#endif

            try
            {
                BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);

                // initiate the parser
                IArgumentParser<CommandLineLayers> argumentParser = new ArgumentParser<CommandLineLayers>();
                argumentParser.Description = "A command line tool for compressing Open Foam (r) files.";
                argumentParser.Name = "ofc.exe";

                // add validators
                argumentParser.Validator<int>(new FuncValidator((string v, ref object d) =>
                {
                    int value;
                    if (!int.TryParse(v, out value)) return false;
                    d = value;
                    return true;
                }));

                // add parser definitions
                argumentParser.NewLayer(CommandLineLayers.Help).AddOption(e => e.SetShortName('h').SetLongName("help").Description("Display this help message."));
                argumentParser.NewLayer(CommandLineLayers.Version).AddOption(e => e.SetLongName("version").Description("Display the current version of the tool."));

                argumentParser.NewLayer(CommandLineLayers.CompressDirectory).Command("compress").Command("directory", "Compress the specified directory.").Argument("input").Argument("output").Option("rounding", e => e.SetName("digits").Type<int>()).Option('f').Option('r').Option('p').Option('s');
                argumentParser.NewLayer(CommandLineLayers.CompressFile).Command("compress").Command("file", "Compress the specified file.").Argument("input").Argument("output").Option("rounding", e => e.SetName("digits").Type<int>()).Option('f').Option('s');

                argumentParser.NewLayer(CommandLineLayers.DecompressDirectory).Command("decompress").Command("directory", "Decompress the specified compressed directory.").Argument("input").Argument("output").Option('f').Option('r').Option('p');
                argumentParser.NewLayer(CommandLineLayers.DecompressFile).Command("decompress").Command("file", "Decompress the specified compressed file or directory.").Argument("input").Argument("output").Argument("data", true).Option('f');

                argumentParser.NewOption().SetLongName("rounding").Description("Enable rounding to the specified amount of digits.");
                argumentParser.NewOption().SetShortName('f').Description("Force overriding of files.");
                argumentParser.NewOption().SetShortName('r').Description("Enable recursive compression/decompression.");
                argumentParser.NewOption().SetShortName('p').Description("Enable parallel compression/decompression.");
                argumentParser.NewOption().SetShortName('s').Description("Treat anonymous lists as lists of one type.");

                // parse the arguments
                var result = argumentParser.Parse(args);

                var ok = false;
                // check if the parser succeeded 
                if (result.Success)
                {
                    ok = true;
                    var manager = new OfcActionManager();
                    IConfiguaration config = new SimpleConfiguration();

                    // check for rounding
                    if (result.GetFlag("rounding"))
                    {
                        config["rounding"] = true;
                        config["roundingDecimals"] = result.GetOption<int>("rounding");
                    }
                    if (result.GetFlag("s"))
                    {
                        config["simplealists"] = true;
                        Console.WriteLine("Simple ano lists :D");
                    }

                    switch (result.LayerId)
                    {
                        // Displays the CLI help
                        case CommandLineLayers.Help:
                            Console.Write(argumentParser.GenerateHelp());
                            break;
                        // Displays the current version
                        case CommandLineLayers.Version:
                            Console.WriteLine($"{argumentParser.Name} [v1.0]");
                            break;

                        // Compresses the specified file
                        case CommandLineLayers.CompressFile:
                            manager.AddCompressFileAction(new ZettyAlgorithm(config), NoDataConverter.Instance, config, result[0], result[1]);
                            manager.Override = result['f'];
                            manager.Handle();
                            break;
                        // Compresses the specified directory
                        case CommandLineLayers.CompressDirectory:
                            manager.AddCompressDirectoryAction(new ZettyAlgorithm(config), NoDataConverter.Instance, config, result[0], result[1], result['r']);
                            if (manager.Empty) Console.WriteLine(" WARNING: input folder is empty");
                            manager.Override = result['f'];
                            manager.Parallel = result['p'];
                            manager.Handle();
                            break;

                        // Decompresses the specified file
                        case CommandLineLayers.DecompressFile:
                            manager.AddDecompressFileAction(new ZettyAlgorithm(config), NoDataConverter.Instance, result[0], result[2] ?? Path.ChangeExtension(result[0], ActionUtils.DataFileExtention), result[1]);
                            manager.Handle();
                            break;
                        // Decompresses the specified directory
                        case CommandLineLayers.DecompressDirectory:
                            manager.AddDecompressDirectoryAction(new ZettyAlgorithm(config), NoDataConverter.Instance, result[0], result[1], result['r']);
                            if (manager.Empty) Console.WriteLine(" WARNING: input folder is empty");
                            manager.Override = result['f'];
                            manager.Parallel = result['p'];
                            manager.Handle();
                            break;
                    }
                }

                // Write an error message
                if (ok) return;
                Console.WriteLine("Invalid arguments.\n");
                Console.Write(argumentParser.GenerateHelp());
            }
            catch (Exception ex)
            {
                Console.WriteLine("fatal application error: \n" + ex);
            }
        }


        /// <summary>
        ///   Represents a layer in the argument parser.
        /// </summary>
        internal enum CommandLineLayers
        {
            Help,
            Version,
            CompressFile,
            CompressDirectory,
            DecompressFile,
            DecompressDirectory
        }
    }
}