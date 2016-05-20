using System;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace Ofc.Algorithm.Zetty
{
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Algorithm.Integration;
    using Ofc.Core;

    public class ZettyCompression : IReporter<string>
    {
        private readonly Stream _outStream;
        private readonly int _maxBlockSize;
        public IConfiguaration Configuaration { get; }
        private readonly string[] _values;
        private int _valueIndex;

        public ZettyCompression(Stream outStream, int maxBlockSize = 1024)
        {
            if (maxBlockSize <= 0) throw new ArgumentException($"{nameof(maxBlockSize) + " must not be <= 0!"}");

            _outStream = outStream;
            _maxBlockSize = maxBlockSize;
            _values = new string[maxBlockSize];
        }

        public void Dispose()
        {
            _outStream.Dispose();
        }

        public void Finish()
        {
            if (_values.Length == 0) return;
            FinishBlock();
        }

        private void FinishBlock()
        {
            var maxNumberLength = 1;
            var maxTotalLength = 1;
            var numberLengths = new int[_valueIndex];

            for (var i = 0; i < _valueIndex; i++)
            {
                var currentLength = _values[i].Length;

                if (currentLength > maxTotalLength)
                    maxTotalLength = currentLength;

                numberLengths[i] = currentLength;
                for (var j = 0; j < _values[i].Length; j++)
                {
                    if (_values[i][j] == 'e')
                    {
                        numberLengths[i] = j;
                        currentLength = j;
                        break;
                    }
                }

                if (currentLength > maxNumberLength)
                    maxNumberLength = currentLength;

            }
            _outStream.WriteByte((byte)maxNumberLength);

            for (var i = 0; i < maxNumberLength; i++)
            {
                for (var j = 0; j < _valueIndex; j++)
                {
                    var str = _values[j];
                    if (numberLengths[j] <= i)
                        _outStream.WriteByte((byte)':');
                    else
                        _outStream.WriteByte((byte)str[i]);
                }
            }

            var changed = true;
            for (var offset = 0; changed; offset++)
            {
                changed = false;
                for (var i = 0; i < _valueIndex; i++)
                {
                    var index = numberLengths[i] + 1 + offset;
                    if (_values[i].Length <= index)
                        _outStream.WriteByte((byte) ':');
                    else
                    {
                        _outStream.WriteByte((byte)_values[i][index]);
                        changed = true;
                    }
                }
            }

            _outStream.Flush();

            _valueIndex = 0;
        }

        public void Flush()
        {
            _outStream.Flush();
        }

        public void Report(string value)
        {
            _values[_valueIndex++] = value;
            if (_valueIndex == _maxBlockSize)
            {
                FinishBlock();
            }
        }

        public void Report(string[] values, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(values[i]);
            }
        }
    }
}
