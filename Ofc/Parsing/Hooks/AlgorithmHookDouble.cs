namespace Ofc.Parsing.Hooks
{
    using System;
    using System.IO;
    using Ofc.Core;

    [Obsolete]
    internal class AlgorithmHookDouble : IParserHook<string>
    {
        private readonly IAlgorithm<double> _algorithm;
        private readonly Stream _output;
        private readonly IConfiguaration _config;
        private IReporter<double> _compress;

        public AlgorithmHookDouble(IAlgorithm<double> algorithm, Stream output, IConfiguaration config)
        {
            _algorithm = algorithm;
            _output = output;
            _config = config;
        }


        public void EnterDictionary(string name)
        {
        }

        public void LeaveDictionary()
        {
        }

        public void EnterCodeStreamDictionary(string name)
        {
        }

        public void LeaveCodeStreamDictionary()
        {
        }

        public void EnterEntry(string name)
        {
        }

        public void LeaveEntry()
        {
        }

        public void EnterList(OfcListType type, int capacity)
        {
            if (_compress != null) return; //throw new NotSupportedException();

            _compress = _algorithm.Compress(null, _config, _output, (int) type, capacity);
        }

        public void HandleListEntry(string value)
        {
            _compress.Report(double.Parse(value));
        }

        public void HandleListEntries(string[] values)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < values.Length; index++)
            {
                HandleListEntry(values[index]);
            }
        }

        public void LeaveList()
        {
            _compress?.Finish();
            _compress = null;
        }

        public void HandleMacro(OfcDirectiveType directive, string data)
        {
        }

        public void HandleDimension(string[] values)
        {
        }

        public void HandleScalar(string value)
        {
        }

        public void HandleKeyword(string value)
        {
        }

        public void HandleString(string data)
        {
        }

        public void Flush()
        {
        }
    }
}