using OfcAlgorithm;
using OfcAlgorithm.Integration;

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


        internal enum CommandLineLayers
        {
            Help,
            Version
        }
    }
}