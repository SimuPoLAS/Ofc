namespace Ofc.Parsing.Hooks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OfcCore;
    using OfcCore.Configurations;

    internal class MarerHook<T> : IParserHook<string>
    {
        private IAlgorithm<T> _algorithm;
        private IConverter<T> _converter;
        private IReporter<T> _reporter;
        private Stream _output;

        private bool _inList;
        private uint _start;

        internal List<CompressedSection> CompressedDataSections { get; set; } = new List<CompressedSection>();

        internal IPositionProvider PositionProvider { get; set; }


        public MarerHook(IAlgorithm<T> algorithm, IConverter<T> converter, Stream output)
        {
            _algorithm = algorithm;
            _converter = converter;
            _output = output;
        }


        public void EnterDictionary(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void LeaveDictionary()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void EnterCodeStreamDictionary(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void LeaveCodeStreamDictionary()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void EnterEntry(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void LeaveEntry()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void EnterList(OfcListType type, int capacity)
        {
            if (type == OfcListType.Anonymous) return;
            if (_inList) throw new NotSupportedException();
            _inList = true;
            _reporter = _algorithm.Compress(null, EmptyConfiguration.Instance, _output, (int) type, capacity);
            if (_reporter == null) throw new NotSupportedException();
            _start = PositionProvider.Position;
        }

        public void HandleListEntry(string value)
        {
            _reporter.Report(_converter.FromString(value));
        }

        public void HandleListEntries(string[] values)
        {
            for (var i = 0; i < values.Length; i++)
                _reporter.Report(_converter.FromString(values[i]));
        }

        public void LeaveList()
        {
            if (!_inList) return;
            _reporter.Finish();
            CompressedDataSections.Add(new CompressedSection(_start, PositionProvider.Position));
            _inList = false;
        }

        public void HandleMacro(OfcDirectiveType directive, string data)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void HandleDimension(string[] values)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void HandleScalar(string value)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void HandleKeyword(string value)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void HandleString(string data)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public void Flush()
        {
        }
    }
}