using System;
using Ofc.Util;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace Ofc.Algorithm.Zetty
{
    using System.IO;
    using Ofc.Core;

    public class ZettyCompression : IReporter<string>
    {
        private readonly StreamBitWriter _bitWriter;
        private readonly int _maxBlockSize;
        public IConfiguaration Configuaration { get; }
        private readonly string[] _values;
        private int _valueIndex;

        public ZettyCompression(Stream outStream, int maxBlockSize = 102400)
        {
            if (maxBlockSize <= 0) throw new ArgumentException($"{nameof(maxBlockSize) + " must not be <= 0!"}");

            _bitWriter = new StreamBitWriter(outStream);
            _maxBlockSize = maxBlockSize;
            _values = new string[maxBlockSize];
        }

        public void Dispose()
        {
            _bitWriter.Dispose();
        }

        public void Finish()
        {
            if (_valueIndex != 0)
                FinishBlock();

            _bitWriter.Write(0, 32);
            _bitWriter.Flush();
        }

        private void FinishBlock()
        {
            var maxNumberLength = 1;
            var maxTotalLength = 1;
            var numberLengths = new int[_valueIndex];
            var expLengths = new int[_valueIndex];
            var averageNumberLength = 0;
            var averageExpLength = 0;

            #region Metadata analysis
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
                        var expLen = _values[i].Length - (j + 1);
                        expLengths[i] = expLen;

                        numberLengths[i] = j;
                        currentLength = j;
                        break;
                    }
                }
                averageNumberLength += numberLengths[i];
                averageExpLength += expLengths[i];

                if (currentLength > maxNumberLength)
                    maxNumberLength = currentLength;

            }
            averageNumberLength /= _valueIndex;
            averageExpLength /= _valueIndex;
            #endregion


            _bitWriter.Write((ulong)_valueIndex, 32); // block size
            _bitWriter.WriteByte((byte)averageNumberLength, 8);
            _bitWriter.WriteByte((byte)averageExpLength, 8);


            for (var i = 0; i < _valueIndex; i++)
            {
                var diff = numberLengths[i] - averageNumberLength;
                _bitWriter.WriteByte((byte)(diff < 0 ? 1 : 0), 1);
                _bitWriter.Write((ulong)(1 << Math.Abs(diff)), (byte)(Math.Abs(diff) + 1)); // Writing NUM ending masks 001 -> ending in 4 chars
            }


            for (var i = 0; i < _valueIndex; i++)
            {
                var diff = expLengths[i] - averageExpLength;
                _bitWriter.WriteByte((byte)(diff < 0 ? 1 : 0), 1);
                _bitWriter.Write((ulong)(1 << Math.Abs(diff)), (byte)(Math.Abs(diff) + 1)); // Writing NUM ending masks 001 -> ending in 4 chars
            }

            _bitWriter.Flush();

            for (var i = 0; i < maxNumberLength; i++) // writing numbers in 111000 notation
            {
                for (var j = 0; j < _valueIndex; j++)
                {
                    var str = _values[j];
                    if (numberLengths[j] > i)
                        _bitWriter.Stream.WriteByte((byte)(str[i]/* - 48*/));
                }
            }

            var changed = true;
            for (var offset = 0; changed; offset++) // writing exponents in 111000 notation
            {
                changed = false;
                for (var i = 0; i < _valueIndex; i++)
                {
                    var index = numberLengths[i] + 1 + offset;
                    if (_values[i].Length > index)
                    {
                        _bitWriter.Stream.WriteByte((byte)(_values[i][index]/* - 48*/));
                        changed = true;
                    }
                }
            }

            _valueIndex = 0;
        }

        public void Flush()
        {
            _bitWriter.Flush();
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
