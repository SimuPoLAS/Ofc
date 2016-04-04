using System.Globalization;
using OfcAlgorithm.Blocky.Integration;
using OfcAlgorithm.Integration;
using OfcAlgorithm.Integration.Dummy;
using OfcCore;
using OfcCore.Configurations;

namespace Ofc
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using Ofc.CommandLine;
    using Ofc.IO;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;
    using Ofc.Util;

    internal class Program
    {
        public static void Main(string[] args)
        {
            var ofcNumber = OfcNumber.Parse(93434.45234d.ToString(CultureInfo.InvariantCulture));
            CompressFolder(@"C:\damBreak4phase_wp8", "out.kappa.bin");


            return;
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryDataWriter(memory))
                {
                    writer.WriteId(true, 7, 0);
                }

                Console.WriteLine($"[{string.Join(", ", BitConverter.GetBytes(2))}]");

                Console.WriteLine($"l: {memory.Length}");
                memory.Position = 0;
                using (var reader = new BinaryDataReader(memory))
                {

                }
            }

            return;


            Console.SetOut(File.CreateText("W:/desktop/z/log"));
            var lexer = new OfcLexer(new FileInputStream("W:/desktop/z/p"), false);
            var parser = new OfcParser(lexer, new DebugHook<string>());
            parser.Parse();

            return;

            // initiate the parser
            IArgumentParser<CommandLineLayers> argumentParser = new ArgumentParser<CommandLineLayers>();
            argumentParser.Description = "A command line tool for compressing Open Foam files.";
            argumentParser.Name = "ofc.exe";

            // add parser definitions
            argumentParser.NewLayer(CommandLineLayers.Help).AddOption(e => e.SetShortName('h').SetLongName("help").Description("Displays this help message."));
            argumentParser.NewLayer(CommandLineLayers.Version).AddOption(e => e.SetLongName("version").Description("Displays the current version of the tool."));

            // parse the arguments
            var result = argumentParser.Parse(args);

            // check if the parser succeeded 
            if (result.Success)
            {
                switch (result.LayerId)
                {
                    case CommandLineLayers.Help:
                        Console.Write(argumentParser.GenerateHelp());
                        return;
                    case CommandLineLayers.Version:
                        Console.WriteLine($"{argumentParser.Name} [v1.0.000]");
                        return;
                }
            }

            // Write an error message
            Console.WriteLine("Invalid arguments.\n");
            Console.Write(argumentParser.GenerateHelp());
        }

        static void CompressFolder(string srcPath, string targetPath)
        {
            using (new FileStream(targetPath, FileMode.Create)) { }
            foreach (var file in Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories))
            {
                CompressFile(file, targetPath, FileMode.Append);
            }
        }

        static void CompressFile(string srcPath, string targetPath, FileMode mode = FileMode.Create)
        {
            using (var str = new FileStream(targetPath, mode))
            {
                BlockyAlgorithm.SetBlockfindingDebugConsoleEnabled(false);
                try
                {
                    var lexi = new OfcLexer(new FileInputStream(srcPath));
                    var purser = new OfcParser(lexi, new AlgorithmHook(new BlockyAlgorithm(), str));
                    purser.Parse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(srcPath );
                }

            }
        }

        static void DecompressFile(string srcPath, string targetPath)
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

        internal enum CommandLineLayers
        {
            Help,
            Version
        }
    }
}