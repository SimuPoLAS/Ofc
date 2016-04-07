// ReSharper disable StaticMemberInGenericType
// ReSharper disable ForCanBeConvertedToForeach

namespace Ofc.Parsing.Hooks
{
    using System;
    using System.IO;
    using System.Text;
    using Ofc.IO;
    using Ofc.Util;
    using OfcCore;
    using OfcCore.Configurations;

    internal class BinaryOutputHook<T> : IParserHook<string>, IDisposable
    {
        private IDataWriter _writer;
        private IAlgorithm<T> _algorithm;
        private IConverter<T> _converter;
        private Encoding _encoding;

        private IReporter<T> _reporter;
        private bool _listOpen = false;


        public BinaryOutputHook(Stream output, IAlgorithm<T> algorithm, IConverter<T> converter)
        {
            _writer = new BinaryDataWriter(output);
            _algorithm = algorithm;
            _converter = converter;
            _encoding = new UTF8Encoding();
        }



        public void EnterDictionary(string name)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(1);
            _writer.WriteString(name);
        }

        public void LeaveDictionary()
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(0);
        }

        public void EnterCodeStreamDictionary(string name)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(2);
            _writer.WriteString(name);
        }

        public void LeaveCodeStreamDictionary()
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(0);
        }

        public void EnterEntry(string name)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(3);
        }

        public void LeaveEntry()
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(0);
        }

        public void EnterList(OfcListType type, int capacity)
        {
            if (_listOpen) throw new NotSupportedException("Does not support list stacking.");
            _listOpen = true;
            switch (type)
            {
                case OfcListType.Scalar:
                    _writer.WriteByte(4);
                    _reporter = _algorithm.Compress(null, EmptyConfiguration.Instance, _writer.BaseStream, 1, capacity);
                    break;
                case OfcListType.Vector:
                    _writer.WriteByte(5);
                    _reporter = _algorithm.Compress(null, EmptyConfiguration.Instance, _writer.BaseStream, 3, capacity);
                    break;
                case OfcListType.Tensor:
                    _writer.WriteByte(6);
                    _reporter = _algorithm.Compress(null, EmptyConfiguration.Instance, _writer.BaseStream, 9, capacity);
                    break;
                case OfcListType.Anonymous:
                    _writer.WriteByte(7);
                    _reporter = _algorithm.Compress(null, EmptyConfiguration.Instance, _writer.BaseStream, 1, capacity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void HandleListEntry(string value)
        {
            if (!_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _reporter.Report(_converter.FromString(value));
        }

        public void HandleListEntries(string[] values)
        {
            if (!_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            for (var i = 0; i < values.Length; i++)
                _reporter.Report(_converter.FromString(values[i]));
        }

        public void LeaveList()
        {
            if (!_listOpen) throw new InvalidOperationException("A list is not opend.");
            _reporter.Finish();
            _reporter.Dispose();
            _reporter = null;
            _listOpen = false;
        }

        public void HandleMacro(OfcDirectiveType directive, string data)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            switch (directive)
            {
                case OfcDirectiveType.Include:
                    _writer.WriteByte(8);
                    _writer.WriteString(data);
                    break;
                case OfcDirectiveType.InputMode:
                    _writer.WriteByte(9);
                    _writer.WriteString(data);
                    break;
                case OfcDirectiveType.Remove:
                    _writer.WriteByte(10);
                    _writer.WriteString(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(directive), directive, null);
            }
        }

        public void HandleDimension(string[] values)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(11);
            for (var i = 0; i < values.Length; i++)
                _converter.Write(_writer.BaseStream, _converter.FromString(values[i]));
        }

        public void HandleScalar(string value)
        {
            if (_listOpen)
            {
                HandleListEntry(value);
                return;
            }
            _writer.WriteByte(12);
            _converter.Write(_writer.BaseStream, _converter.FromString(value));
        }

        public void HandleKeyword(string value)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(13);
            _writer.WriteString(value);
        }

        public void HandleString(string data)
        {
            if (_listOpen) throw new NotSupportedException("Can not write a non list element tag when inside of a list.");
            _writer.WriteByte(14);
            _writer.WriteString(data);
        }

        public void Flush()
        {
            _reporter?.Flush();
        }

        public void Dispose()
        {
        }
    }
}