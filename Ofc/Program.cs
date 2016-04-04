namespace Ofc
{
    using System;
    using System.IO;
    using Ofc.CommandLine;
    using Ofc.IO;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;
    using OfcAlgorithm.Blocky.Integration;
    using OfcAlgorithm.Integration.Dummy;
    using OfcCore.Configurations;

    internal class Program
    {
        public static void Main(string[] args)
        {
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

        private static void CompressFolder(string srcPath, string targetPath)
        {
            using (new FileStream(targetPath, FileMode.Create))
            {
            }
            foreach (var file in Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories))
            {
                CompressFile(file, targetPath, FileMode.Append);
            }
        }

        private static void CompressFile(string srcPath, string targetPath, FileMode mode = FileMode.Create)
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
                    Console.WriteLine(srcPath + ": " + ex);
                }
            }
        }

        private static void DecompressFile(string srcPath, string targetPath)
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