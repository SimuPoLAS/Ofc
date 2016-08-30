namespace Ofc.Actions
{
    using System;
    using System.IO;
    using Algorithm.RoundingDigits;
    using Core;
    using IO;
    using LZMA.Helper;
    using Parsing;
    using Parsing.Hooks;
    using Util.Converters;

    internal class CompressAction<TAlgorithm> : IOfcAction
    {
        public string Code => "COM";

        public string Message { get; set; }

        public bool Faulty => _faulty;

        public string Path => _relativePath;

        public int Status { get; set; } = -1;

        public OfcActionResult Result => _result;


        public bool Force { get; set; }


        private readonly IAlgorithm<TAlgorithm> _algorithm;
        private readonly IConverter<TAlgorithm> _converter;
        private readonly IConfiguaration _configuaration;
        private readonly string _sourcePath;
        private readonly string _dataPath;
        private readonly string _metaPath;
        private readonly string _relativePath;
        private readonly string _lzmaPath;

        private bool _generatedDataFile;

        private bool _faulty;
        private OfcActionResult _result = OfcActionResult.Done;


        public CompressAction(IAlgorithm<TAlgorithm> algorithm, IConverter<TAlgorithm> converter, IConfiguaration config, string basePath, string sourcePath, string dataPath, string metaPath, string lzmaPath)
        {
            _algorithm = algorithm;
            _converter = converter;
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
                    MarerHook hook;
                    if (_configuaration.True("rounding"))
                        if (typeof(TAlgorithm) == typeof(string)) hook = new MarerHook<string>(new RoundingDigitsAlgorithm<string>((IAlgorithm<string>) _algorithm), NoDataConverter.Instance, binaryOutput, _configuaration);
                        // if not we have to use the converting algorithm
                        else hook = new MarerHook<string>(new RoundingDigitsAlgorithm<TAlgorithm>(new StringSourceAlgorithm<TAlgorithm>(_converter, _algorithm)), NoDataConverter.Instance, binaryOutput, _configuaration);
                    else hook = new MarerHook<TAlgorithm>(_algorithm, _converter, binaryOutput, _configuaration);

                    Status = 2;
                    //var hook = new MarerHook<OfcNumber>(algorithm, converter, binaryOutput, _configuaration);

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
            catch (Exception exception)
            {
                Message = "Internal: " + exception.Message;
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