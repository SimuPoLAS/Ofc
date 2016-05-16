namespace Ofc.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.Core;
    using Ofc.Core.Configurations;
    using Ofc.Parsing;

    internal class MarerReader<T> : IDisposable
    {
        private StreamReader _reader;
        private StreamWriter _writer;
        private IAlgorithm<T> _algorithm;
        private Stream _stream;
        private MarerReporter<T> _reporter;

        private List<CompressedSection> _sections = new List<CompressedSection>();
        private char[] _buffer;
        private long _position;
        private bool _ended;


        public MarerReader(StreamReader reader, StreamWriter writer, IAlgorithm<T> algorithm, IConverter<T> converter, [CanBeNull] Stream stream) : this(reader, writer, algorithm, converter, stream, 4096)
        {
        }

        public MarerReader(StreamReader reader, StreamWriter writer, IAlgorithm<T> algorithm, IConverter<T> converter, [CanBeNull] Stream stream, int bufferSize)
        {
            _reader = reader;
            _writer = writer;
            _algorithm = algorithm;
            _stream = stream;
            _buffer = new char[bufferSize];
            _reporter = new MarerReporter<T>(_writer, converter);
        }


        internal void Do()
        {
            // Read header
            var buffer = new byte[8];
            long value = 0;
            while (_reader.BaseStream.Read(buffer, 0, 8) == 8 && (value = BitConverter.ToInt64(buffer, 0)) != -1)
                _sections.Add(new CompressedSection((uint) value, 0, (byte) _reader.BaseStream.ReadByte()));
            if (value != -1) throw new FormatException("Bad header.");

            // Read data
            while (_sections.Count > 0)
            {
                var section = _sections[0];
                _sections.RemoveAt(0);

                JumpTo(section.Start);
                _reporter.Size = section.Size;
                _algorithm.Decompress(null, EmptyConfiguration.Instance, _stream, _reporter, section.Size);
            }
            JumpTo(long.MaxValue);
        }

        private void JumpTo(long position)
        {
            if (_ended) return;
            if (position < _position) throw new NotSupportedException();
            var toRead = position - _position;
            while (toRead > 0)
            {
                var a = toRead > _buffer.Length ? _buffer.Length : (int) toRead;
                var read = _reader.Read(_buffer, 0, a);
                if (read == 0)
                {
                    _ended = true;
                    return;
                }
                toRead -= read;
                _writer.Write(_buffer, 0, read);
            }
            _position = position;
        }


        public void Dispose()
        {
        }


        private class MarerReporter<T> : IReporter<T>
        {
            public IConfiguaration Configuaration { get; }

            internal int Size
            {
                get { return _buffer.Length; }
                set
                {
                    _buffer = new string[value];
                    _position = 0;
                }
            }


            private string[] _buffer;
            private int _position;

            private StreamWriter _writer;
            private IConverter<T> _converter; 


            public MarerReporter(StreamWriter writer, IConverter<T> converter)
            {
                _writer = writer;
                _converter = converter;
                Size = 1;
            }


            public void Dispose()
            {
            }

            public void Finish()
            {
            }

            public void Flush()
            {
            }

            public void Report(T value)
            {
                _buffer[_position++] = _converter.ToString(value);
                if (_position == _buffer.Length)
                {
                    switch (Size)
                    {
                        case 1:
                            _writer.WriteLine(_buffer[0]);
                            break;
                        case 3:
                        case 9:
                            _writer.WriteLine("(" + string.Join(" ", _buffer) + ")");
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    _position = 0;
                }
            }

            public void Report(T[] values, int offset, int amount)
            {
                for (var i = offset; i < offset + amount; i++)
                    Report(values[i]);
            }
        }
    }
}