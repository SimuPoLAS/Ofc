using System;
using System.Diagnostics;
using System.Linq;
using ConsoleApp1;
using Ofc.LZMA.Compatibility;
using Ofc.Util;

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
        private readonly StreamBitWriter _bitWriter;
        private readonly int _maxBlockSize;
        public IConfiguaration Configuaration { get; }
        private readonly string[] _values;
        private int _valueIndex;
        private int[] VORKOMMEN = new int[10];

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

            //     _bitWriter.Write(0, 32);
            //   _bitWriter.Flush();
        }

        private void FinishBlock()
        {
            var maxNumberLength = 1;
            var minNumberLength = int.MaxValue;
            var minExpLength = int.MaxValue;
            var maxTotalLength = 1;
            var numberLengths = new int[_valueIndex];
            var expLengths = new int[_valueIndex];
            var memStream = new MemoryStream();

            //  var countArr = BitConverter.GetBytes(_valueIndex);
            //  memStream.Write(countArr, 0, countArr.Length);

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
                        if (expLen < minExpLength)
                            minExpLength = expLen;

                        numberLengths[i] = j;
                        currentLength = j;
                        break;
                    }
                }

                if (currentLength > maxNumberLength)
                    maxNumberLength = currentLength;

                if (currentLength < minNumberLength)
                    minNumberLength = currentLength;
            }
            #endregion

            for (var i = 0; i < maxNumberLength; i++)
            {
                for (var j = 0; j < _valueIndex; j++)
                {
                    var str = _values[j];
                    if (numberLengths[j] > i)
                        memStream.WriteByte((byte)(str[i]/* - 48*/));
                }
            }

            var changed = true;
            for (var offset = 0; changed; offset++)
            {
                changed = false;
                for (var i = 0; i < _valueIndex; i++)
                {
                    var index = numberLengths[i] + 1 + offset;
                    if (_values[i].Length > index)
                    {
                        memStream.WriteByte((byte)(_values[i][index]/* - 48*/));
                        changed = true;
                    }
                }
            }


            var data = memStream.ToArray();

            //foreach (var b in data)
            //{
            //    if (b < 10)
            //        VORKOMMEN[b]++;
            //}

            //var dict = new Dictionary<int, int>();
            //for (var i = 0; i < VORKOMMEN.Length; i++)
            //{
            //    while (dict.ContainsKey(VORKOMMEN[i]))
            //    {
            //        VORKOMMEN[i]++;
            //    }
            //    dict.Add(VORKOMMEN[i], i);
            //}
            //var huffmanCoding = new HuffmanCoding<int>(dict);
            //var tree = huffmanCoding.CreateTree();
            //var binRep = new Tuple<int, byte>[10];
            //for (var i = 0; i < 10; i++)
            //{
            //    binRep[i] = tree.GetLeafNode(i).GetBooleanEncoding().GetBinaryPresentation();
            //}


            memStream.Close();
            _bitWriter.Write((ulong)_valueIndex, 32);
            _bitWriter.WriteByte((byte)minNumberLength, 8);
            _bitWriter.WriteByte((byte)minExpLength, 8);

            var avg = (int)numberLengths.Average();

            for (var i = 0; i < _valueIndex; i++)
            {
                var diff = numberLengths[i] - avg;
                _bitWriter.WriteByte((byte)(diff < 0 ? 1 : 0), 1);
                _bitWriter.Write((ulong)(1 << Math.Abs(diff)), (byte)(Math.Abs(diff) + 1)); // Writing NUM ending masks 001 -> ending in 3 chars
            }


            var avgExp = (int)expLengths.Average();
            for (var i = 0; i < _valueIndex; i++)
            {
                var diff = expLengths[i] - avgExp;
                _bitWriter.WriteByte((byte)(diff < 0 ? 1 : 0), 1);
                _bitWriter.Write((ulong)(1 << Math.Abs(diff)), (byte)(Math.Abs(diff) + 1)); // Writing NUM ending masks 001 -> ending in 3 chars
            }

            _bitWriter.Stream.Write(data, 0, data.Length);

            //  Console.WriteLine("MASK IS " + (_bitWriter.Stream.Position - pos) + " BYTE LONG");

         //   CompressData(data);
            _valueIndex = 0;
        }

        private void CompressData(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] -= 48;
            }

            for (var i = 0; i < data.Length; i++)
            {
                if (i + 2 < data.Length && data[i + 1] == data[i] && data[i] == data[i + 2])
                {
                    var length = 3;
                    for (var j = i + 3; j < data.Length && length < 34; j++) // 34 -> 31 (5b) + 3 std offset
                    {
                        if (data[j] == data[i])
                        {
                            length++;
                            continue;
                        }
                        break;
                    }

                    var encLength = length - 3;
                    var encLengthNb = Utility.GetNeededBits(encLength);
                    _bitWriter.WriteByte((byte)(10 + encLengthNb), 4);
                    _bitWriter.WriteByte((byte)length, encLengthNb);
                    _bitWriter.WriteByte(data[i], 4);

                 //   Console.WriteLine((char)(data[i] + 48) + " x " + length);

                    /*
                     * NOTES:
Compression is good (suppesedly to 7KB), but the encoding sucks (size x2)
idea: get min number length. for each digit after that length, add 1 bit that says if theres a next digit -> no :
                     * */
                    i += length - 1;
                }
                else
                {
                   // Console.WriteLine((char)(data[i] + 48));
                   // if (data[i] < 10)
                        _bitWriter.WriteByte(data[i], 4);
                   // else
                    {
                       // Console.WriteLine("INVALID SYMBOL: " + ((char)data[i] + 48));
                        //_bitWriter.WriteByte((byte)(data[i] + 48), 8);
                    }
                }
            }
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
