namespace Ofc.Actions
{
    using System;
    using System.IO;
    using Ofc.Algorithm.Blocky.Integration;
    using Ofc.Algorithm.Integration;
    using Ofc.Algorithm.Rounding;
    using Ofc.Core;
    using Ofc.IO;
    using Ofc.LZMA.Helper;
    using Ofc.Parsing;
    using Ofc.Parsing.Hooks;

    internal class CompressAction : IOfcAction
    {
        public string Code => "COM";

        public string Message { get; set; }

        public bool Faulty => _faulty;

        public string Path => _relativePath;

        public int Status { get; set; } = -1;

        public OfcActionResult Result => _result;


        public bool Force { get; set; }


        private string _sourcePath;
        private string _dataPath;
        private string _metaPath;
        private string _relativePath;
        private string _lzmaPath;
        private bool _generatedDataFile;
        private IConfiguaration _configuaration;

        private bool _faulty;
        private OfcActionResult _result = OfcActionResult.Done;


        public CompressAction(string basePath, string sourcePath, string dataPath, string metaPath, string lzmaPath, IConfiguaration config)
        {
            _sourcePath = sourcePath;
            _dataPath = dataPath;
            _metaPath = metaPath;
            _lzmaPath = lzmaPath;
            _relativePath = basePath != null && _sourcePath.StartsWith(basePath) ? _sourcePath.Substring(basePath.Length) : _sourcePath;
            _configuaration = config;
        }


        public void Preperation()
        {
            if (!File.Exists(_sourcePath)) Throw<FileNotFoundException>("Could not find the specified file.");
            if (!Force)
            {
                if (File.Exists(_metaPath)) Throw<Exception>("The destination file does already exist.");
                if (File.Exists(_dataPath)) Throw<Exception>("The destination file does already exist.");
            }
        }

        public void Conduction()
        {
            try
            {
                Status = 0;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_dataPath));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_metaPath));
                // open the binary output file
                using (var binaryOutput = File.OpenWrite(_dataPath + ActionUtils.TempFileExtention))
                {
                    Status = 1;
                    var algorithm = _configuaration.True("rounding") ? (IAlgorithm<OfcNumber>) new RounderAlgorithm(new BlockyAlgorithm()) : new BlockyAlgorithm();
                    var converter = new CompressionDataConverter();

                    Status = 2;
                    var hook = new MarerHook<OfcNumber>(algorithm, converter, binaryOutput, _configuaration);

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
                            _generatedDataFile = hook.CompressedDataSections.Count != 0;
                        }
                        catch (Exception)
                        {
                            f = true;
                        }
                    }
                    Status = 6;
                    if (f || hook.CompressedDataSections.Count == 0)
                    {
                        _generatedDataFile = false;
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lzmaPath));
                        Status = 100;
                        using (var source = File.OpenRead(_sourcePath))
                        {
                            Status = 101;
                            using (var outp = File.OpenWrite(_lzmaPath))
                            {
                                Status = 102;
                                LzmaHelper.CompressLzma(source, outp);
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
                            using (var outp = File.CreateText(_metaPath + ActionUtils.TempFileExtention))
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
                        using (var source = File.OpenRead(_metaPath + ActionUtils.TempFileExtention))
                        {
                            Status = 206;
                            using (var outp = File.OpenWrite(_metaPath))
                            {
                                Status = 207;
                                LzmaHelper.CompressLzma(source, outp);
                                Status = 208;
                            }
                        }
                        Status = 209;
                        File.Delete(_metaPath + ActionUtils.TempFileExtention);
                        Status = 210;
                    }
                }

                if (_generatedDataFile)
                {
                    Status = 212;
                    using (var input = File.OpenRead(_dataPath + ActionUtils.TempFileExtention))
                    {
                        Status = 213;
                        using (var output = File.OpenWrite(_dataPath))
                        {
                            Status = 214;
                            LzmaHelper.CompressLzma(input, output);
                            Status = 215;
                        }
                        Status = 216;
                    }
                    Status = 217;
                }
                File.Delete(_dataPath + ActionUtils.TempFileExtention);
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
                return;
            }
            if (Status == 211)
            {
                _result = OfcActionResult.Done;
                return;
            }

            if (Status > 1 && File.Exists(_dataPath + ActionUtils.TempFileExtention)) File.Delete(_dataPath + ActionUtils.TempFileExtention);
            if (Status >= 100 && Status < 200 && File.Exists(_lzmaPath + ActionUtils.TempFileExtention)) File.Delete(_lzmaPath + ActionUtils.TempFileExtention);
            if (Status >= 200 && Status < 300 && File.Exists(_metaPath + ActionUtils.TempFileExtention)) File.Delete(_metaPath + ActionUtils.TempFileExtention);
        }


        private void Throw<T>(string message) where T : Exception, new()
        {
            Message = message;
            _faulty = true;
            throw new T();
        }
    }
}