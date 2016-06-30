namespace Ofc.Parsing.Hooks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Core;
    using Ofc.Core.Configurations;

    internal abstract class MarerHook : IParserHook<string>
    {
        internal abstract List<CompressedSection> CompressedDataSections { get; set; }

        internal abstract IPositionProvider PositionProvider { get; set; }


        public abstract void EnterDictionary(string name);

        public abstract void LeaveDictionary();

        public abstract void EnterCodeStreamDictionary(string name);

        public abstract void LeaveCodeStreamDictionary();

        public abstract void EnterEntry(string name);

        public abstract void LeaveEntry();

        public abstract void EnterList(OfcListType type, int capacity);

        public abstract void HandleListEntry(string value);

        public abstract void HandleListEntries(string[] values);

        public abstract void LeaveList();

        public abstract void HandleMacro(OfcDirectiveType directive, string data);

        public abstract void HandleDimension(string[] values);

        public abstract void HandleScalar(string value);

        public abstract void HandleKeyword(string value);

        public abstract void HandleString(string data);

        public abstract void Flush();
    }

    internal class MarerHook<T> : MarerHook
    {
        private IAlgorithm<T> _algorithm;
        private IConverter<T> _converter;
        private IReporter<T> _reporter;
        private Stream _output;
        private IConfiguaration _configuaration;

        private bool _inList;
        private uint _start;
        private byte _size;

        internal override List<CompressedSection> CompressedDataSections { get; set; } = new List<CompressedSection>();

        internal override IPositionProvider PositionProvider { get; set; }


        public MarerHook(IAlgorithm<T> algorithm, IConverter<T> converter, Stream output, IConfiguaration configuaration)
        {
            _algorithm = algorithm;
            _converter = converter;
            _output = output;
            _configuaration = configuaration;
        }


        public override void EnterDictionary(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void LeaveDictionary()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void EnterCodeStreamDictionary(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void LeaveCodeStreamDictionary()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void EnterEntry(string name)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void LeaveEntry()
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void EnterList(OfcListType type, int capacity)
        {
            if (type == OfcListType.Anonymous) return;
            if (_inList) throw new NotSupportedException();
            _inList = true;
            _reporter = _algorithm.Compress(null, _configuaration ?? EmptyConfiguration.Instance, _output, (int) type, capacity);
            _size = (byte) type;
            if (_reporter == null) throw new NotSupportedException();
            _start = PositionProvider.Position;
        }

        public override void HandleListEntry(string value)
        {
            _reporter.Report(_converter.FromString(value));
        }

        public override void HandleListEntries(string[] values)
        {
            for (var i = 0; i < values.Length; i++)
                _reporter.Report(_converter.FromString(values[i]));
        }

        public override void LeaveList()
        {
            if (!_inList) return;
            _reporter.Finish();
            CompressedDataSections.Add(new CompressedSection(_start, PositionProvider.Position, _size));
            _inList = false;
        }

        public override void HandleMacro(OfcDirectiveType directive, string data)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void HandleDimension(string[] values)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void HandleScalar(string value)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void HandleKeyword(string value)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void HandleString(string data)
        {
            if (_inList) throw new InvalidOperationException();
        }

        public override void Flush()
        {
        }
    }
}