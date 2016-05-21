using Ofc.Algorithm.Raw;
using Ofc.Algorithm.Zetty;
using Ofc.Core.Configurations;
using Ofc.Parsing;
using Ofc.Parsing.Hooks;

namespace Ofc
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Ofc.Actions;
    using Ofc.Algorithm.Blocky.Integration;
    using Ofc.Algorithm.Integration;
    using Ofc.CLI;
    using Ofc.IO;
    using Ofc.LZMA.Helper;
    using Ofc.Util;

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
        public static void Main(string[] args)
        {
            try
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
                    switch (result.LayerId)
                    {
                        case CommandLineLayers.Help:
                            Console.Write(argumentParser.GenerateHelp());
                            break;
                        case CommandLineLayers.Version:
                            Console.WriteLine($"{argumentParser.Name} [v1.0.000]");
                            break;

                        case CommandLineLayers.CompressFile:
                            manager.AddFile(result[0], result[1]);
                            manager.Handle();
                            break;
                        case CommandLineLayers.CompressDirectory:
                            manager.AddDirectory(result[0], result[1], result['r']);
                            if (manager.Empty) Console.WriteLine("WARNING: Input folder is empty.");
                            manager.Override = result['f'];
                            manager.Parallel = result['p'];
                            manager.Handle();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("fatal application error: \n" + ex);
            }
        }

        private static void TestZetty()
        {
            var outTotal = File.Open("roffel.bin", FileMode.Append);
            var outTotal2 = File.Open("roffel2.bin", FileMode.Append);
            foreach (var file in Directory.EnumerateFiles(@"C:\damBreak4phase_wp8", "*", SearchOption.AllDirectories))
            //var file = @"C:\damBreak4phase_wp8\0.2\T";
            {
                using (var stream = new FileInputStream(file))
                {

                    Console.WriteLine(file);
                    var outStream = new MemoryStream();
                    try
                    {

                        var lexer = new OfcLexer(stream, true);

                        var parser = new OfcParser(lexer, new AlgorithmHookString(new ZettyAlgorithm(new SimpleConfiguration()), outStream, new SimpleConfiguration()));
                        parser.Parse();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cant parse " + file + " bcus " + ex);
                        stream.Dispose();
                        Console.WriteLine("Reopening ...");
                        outStream = new MemoryStream(File.ReadAllBytes(file));
                    }

                    outStream.Position = 0;
                    var buf = outStream.ToArray();
                    outTotal.Write(buf, 0, buf.Length);
                    outTotal.Flush();
                    //Helper.CompressLzma(outStream, outTotal);
                }

                using (var stream = new FileInputStream(file))
                {

                    Console.WriteLine(file);
                    var outStream = new MemoryStream();
                    try
                    {

                        var lexer = new OfcLexer(stream, true);

                        var parser = new OfcParser(lexer, new AlgorithmHookString(new RawAlgorithm(), outStream, new SimpleConfiguration()));
                        parser.Parse();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cant parse " + file + " bcus " + ex);
                        stream.Dispose();
                        Console.WriteLine("Reopening ...");
                        outStream = new MemoryStream(File.ReadAllBytes(file));
                    }

                    outStream.Position = 0;
                    var buf = outStream.ToArray();
                    outTotal2.Write(buf, 0, buf.Length);
                    outTotal2.Flush();
                    //   Helper.CompressLzma(outStream, outTotal);
                }
            }

        }

        [Obsolete]
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

        [Obsolete]
        private static bool DecompressDirectory(string input, string output, bool force, bool recursive, bool para)
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

                if (para)
                {
                    Parallel.ForEach(Directory.EnumerateFiles(input, "*.dat", option).Union(Directory.EnumerateFiles(input, "*.datu", option)), e =>
                    {
                        Interlocked.Increment(ref z);
                        try
                        {
                            var outp = Path.Combine(output, e.Substring(input.Length));
                            outp = Path.ChangeExtension(outp, "");
                            Directory.CreateDirectory(Path.GetDirectoryName(outp));
                            if (DecompressFile(e, outp, true))
                                Interlocked.Increment(ref z);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });
                }
                else
                {
                    foreach (var file in Directory.EnumerateFiles(input, "*.dat", option).Union(Directory.EnumerateFiles(input, "*.datu", option)))
                    {
                        z++;
                        try
                        {
                            var outp = Path.Combine(output, file.Substring(input.Length));
                            outp = Path.ChangeExtension(outp, "");
                            Directory.CreateDirectory(Path.GetDirectoryName(outp));
                            if (DecompressFile(file, outp, true))
                                r++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
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