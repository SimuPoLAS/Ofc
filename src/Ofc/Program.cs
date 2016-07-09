/**
 * ~~~ FRIENDLY REMINDER ~~~
 * Please do not sync your own testing code in Program.cs (or in any other file for that matter)
 * Just DONT
 * And if you are so malicious and do so - AT LEAST REMOVE IT AFTERWARDS 
 * 
 * Zankyou no terror
 * - Widi
 * */

namespace Ofc
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.Actions;
    using Ofc.Algorithm.Blocky.Integration;
    using Ofc.CLI;
    using Ofc.CLI.Validators;
    using Ofc.Core;
    using Ofc.Core.Configurations;

    /// <summary>
    ///     Contains the main entrypoint for the application and the all the CLI functionality.
    /// </summary>
    [UsedImplicitly]
    internal static class Program
    {
        /// <summary>
        ///     Main entrypoint for the application.
        /// </summary>
        /// <param name="args"></param>
        [UsedImplicitly]
        public static void Main(string[] args)
        {
            try
            {
                BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);

                // initiate the parser
                IArgumentParser<CommandLineLayers> argumentParser = new ArgumentParser<CommandLineLayers>();
                argumentParser.Description = "A command line tool for compressing Open Foam files.";
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
                argumentParser.NewLayer(CommandLineLayers.Help).AddOption(e => e.SetShortName('h').SetLongName("help").Description("Displays this help message."));
                argumentParser.NewLayer(CommandLineLayers.Version).AddOption(e => e.SetLongName("version").Description("Displays the current version of the tool."));

                argumentParser.NewLayer(CommandLineLayers.CompressDirectory).Command("compress").Command("directory", "Compresses the specified directory.").Argument("input").Argument("output").Option("rounding", e => e.SetName("digits").Type<int>()).Option('f').Option('r').Option('p').Option('s');
                argumentParser.NewLayer(CommandLineLayers.CompressFile).Command("compress").Command("file", "Compresses the specified file.").Argument("input").Argument("output").Option("rounding", e => e.SetName("digits").Type<int>()).Option('f').Option('s');

                argumentParser.NewLayer(CommandLineLayers.DecompressDirectory).Command("decompress").Command("directory", "Decompresses the specified compressed directory.").Argument("input").Argument("output").Option('f').Option('r').Option('p');
                argumentParser.NewLayer(CommandLineLayers.DecompressFile).Command("decompress").Command("file", "Decompresses the specified compressed file or set of files.").Argument("input").Argument("output").Argument("data", true).Option('f');

                argumentParser.NewOption().SetShortName('f').Description("Enables force mode.");
                argumentParser.NewOption().SetShortName('r').Description("Enables recursive compression/decompression.");
                argumentParser.NewOption().SetShortName('p').Description("Enables parallel compression/decompression.");
                
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
                            manager.AddCompressFileAction(result[0], result[1], config);
                            manager.Override = result['f'];
                            manager.Handle();
                            break;
                        // Compresses the specified directory
                        case CommandLineLayers.CompressDirectory:
                            manager.AddCompressDirectoryAction(result[0], result[1], result['r'], config);
                            if (manager.Empty) Console.WriteLine(" WARNING: input folder is empty");
                            manager.Override = result['f'];
                            manager.Parallel = result['p'];
                            manager.Handle();
                            break;

                        // Decompresses the specified file
                        case CommandLineLayers.DecompressFile:
                            manager.AddDecompressFileAction(result[0], result[2] ?? Path.ChangeExtension(result[0], ActionUtils.DataFileExtention), result[1]);
                            manager.Handle();
                            break;
                        // Decompresses the specified directory
                        case CommandLineLayers.DecompressDirectory:
                            manager.AddDecompressDirectoryAction(result[0], result[1], result['r']);
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
        ///     Represents a layer in the argument parser.
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