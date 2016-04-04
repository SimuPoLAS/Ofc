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

        internal enum CommandLineLayers
        {
            Help,
            Version
        }
    }
}