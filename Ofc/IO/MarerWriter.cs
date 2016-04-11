namespace Ofc.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Parsing;

    internal class MarerWriter : IDisposable
    {
        private StreamReader _reader;
        private StreamWriter _output;
        private List<CompressedSection> _sections;

        private long _position = 0;
        private char[] _buffer;

        private bool _ended = false;


        public MarerWriter(StreamReader input, StreamWriter output, List<CompressedSection> sections) : this(input, output, sections, 4096)
        {
        }

        public MarerWriter(StreamReader input, StreamWriter output, List<CompressedSection> sections, int bufferSize)
        {
            _reader = input;
            _output = output;
            _sections = sections;
            _buffer = new char[bufferSize];
        }


        internal void Do()
        {
            // Write header
            long t = 0;
            for (var i = 0; i < _sections.Count; i++)
            {
                var section = _sections[i];
                var value = section.Start - t;
                t += section.End - section.Start;
                _output.BaseStream.Write(BitConverter.GetBytes(value), 0, 8);
            }
            _output.BaseStream.Write(BitConverter.GetBytes(-1L), 0, 8);

            // Write real data
            while (_sections.Count > 0)
            {
                if (_ended) throw new NotSupportedException();
                var current = _sections[0];
                _sections.RemoveAt(0);

                JumpTo(current.Start);
                SkipTo(current.End);
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
                _output.Write(_buffer, 0, read);
            }
            _position = position;
        }

        private void SkipTo(long position)
        {
            if (_ended) return;
            if (position < _position) throw new NotSupportedException();
            var toRead = position - _position;
            while (toRead > 0)
            {
                var a = toRead > _buffer.Length ? _buffer.Length : (int)toRead;
                var read = _reader.Read(_buffer, 0, a);
                if (read == 0)
                {
                    _ended = true;
                    return;
                }
                toRead -= read;
            }
            _position = position;
        }


        public void Dispose()
        {
        }
    }
}