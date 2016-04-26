namespace Ofc.Util
{
    using System;
    using System.IO;
    using LZMA.Core.Helper;
    using Ofc.IO;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;
    using OfcAlgorithm.Blocky.Integration;
    using OfcAlgorithm.Integration;

    internal class CompressAction : IOfcAction
    {
        public string Code => "COMP";

        public string Message { get; set; }

        public bool Faulty => _faulty;

        public string Path => _relativePath;

        public int Status { get; set; } = -1;

        public OfcActionResult Result => _result;


        public bool Force { get; set; }


        private string _basePath;
        private string _sourcePath;
        private string _binaryPath;
        private string _textualPath;
        private string _relativePath;
        private string _lzmaPath;

        private bool _faulty;
        private OfcActionResult _result = OfcActionResult.Done;


        public CompressAction(string basePath, string sourcePath, string binaryPath, string textualPath, string lzmaPath)
        {
            _basePath = basePath;
            _sourcePath = sourcePath;
            _binaryPath = binaryPath;
            _textualPath = textualPath;
            _lzmaPath = lzmaPath;
            _relativePath = _sourcePath.StartsWith(_basePath) ? _sourcePath.Substring(_basePath.Length) : _sourcePath;
        }


        public void Conduction()
        {
            try
            {
                Status = 0;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_binaryPath));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_textualPath));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lzmaPath));
                // open the binary output file
                using (var binaryOutput = File.OpenWrite(_binaryPath + ".tmp"))
                {
                    Status = 1;
                    var algorithm = new BlockyAlgorithm();
                    var converter = new CompressionDataConverter();

                    Status = 2;
                    var hook = new MarerHook<OfcNumber>(algorithm, converter, binaryOutput);

                    Status = 3;
                    var f = false;
                    using (var source = new FileInputStream(_sourcePath))
                    {
                        Status = 4;
                        var lexer = new OfcLexer(source);
                        var parser = new OfcParser(lexer, hook);
                        hook.PositionProvider = parser;
                        Status = 5;
                        try
                        {
                            parser.Parse();
                        }
                        catch (Exception)
                        {
                            f = true;
                        }
                    }
                    Status = 6;
                    if (f)
                    {
                        Status = 100;
                        using (var source = File.OpenRead(_sourcePath))
                        {
                            Status = 101;
                            using (var outp = File.OpenWrite(_lzmaPath + ".tmp"))
                            {
                                Status = 102;
                                Helper.CompressLzma(source, outp);
                                Status = 103;
                            }
                        }
                        Status = 104;
                    }
                    else
                    {
                        Status = 200;
                        using (var source = File.OpenText(_sourcePath))
                        {
                            Status = 201;
                            using (var outp = File.CreateText(_textualPath + ".tmp"))
                            {
                                Status = 202;
                                using (var writer = new MarerWriter(source, outp, hook.CompressedDataSections))
                                {
                                    Status = 203;
                                    writer.Do();
                                    Status = 204;
                                }
                            }
                        }
                        Status = 205;
                        using (var source = File.OpenRead(_textualPath + ".tmp"))
                        {
                            Status = 206;
                            using (var outp = File.OpenWrite(_textualPath + ".x.tmp"))
                            {
                                Status = 207;
                                Helper.CompressLzma(source, outp);
                                Status = 208;
                            }
                        }
                        Status = 209;
                        File.Delete(_textualPath + ".tmp");
                        Status = 210;
                        File.Move(_textualPath + ".x.tmp", _textualPath + ".tmp");
                        Status = 211;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Message = "Access failed.";
                throw;
            }
            catch (Exception)
            {
                Message = "?";
                throw;
            }
        }

        public void Cleanup()
        {
            if (Status == 104)
            {
                _result = OfcActionResult.Lzma;

                if (File.Exists(_binaryPath + ".tmp")) File.Delete(_binaryPath + ".tmp");
                if (File.Exists(_lzmaPath)) File.Delete(_lzmaPath);
                File.Move(_lzmaPath + ".tmp", _lzmaPath);

                return;
            }
            if (Status == 211)
            {
                _result = OfcActionResult.Done;

                if (File.Exists(_binaryPath)) File.Delete(_binaryPath);
                File.Move(_binaryPath + ".tmp", _binaryPath);

                if (File.Exists(_textualPath)) File.Delete(_textualPath);
                File.Move(_textualPath + ".tmp", _textualPath);

                return;
            }

            if (Status > 1 && File.Exists(_binaryPath + ".tmp")) File.Delete(_binaryPath + ".tmp");
            if (Status >= 100 && Status < 200 && File.Exists(_lzmaPath + ".tmp")) File.Delete(_lzmaPath + ".tmp");
            if (Status >= 200 && Status < 300 && File.Exists(_textualPath + ".tmp")) File.Delete(_textualPath + ".tmp");
        }

        public void Preperation()
        {
            if (!File.Exists(_sourcePath)) Throw<FileNotFoundException>("Could not find the specified file.");
            if (!Force)
            {
                if (File.Exists(_binaryPath)) Throw<Exception>("Not allowed to override existing file.");
                if (File.Exists(_textualPath)) Throw<Exception>("Not allowed to override existing file.");
            }
        }


        private void Throw<T>(string message) where T : Exception, new()
        {
            Message = message;
            throw new T();
        }
    }
}