using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ofc.Core;
using Ofc.Util;

namespace Ofc.Algorithm.Zetty
{
    public class ZettyDecompression
    {
        private readonly IReporter<string> _output;
        private readonly int _maxBlockSize;
        private readonly StreamBitReader _streamBitReader;

        public ZettyDecompression(Stream input, IReporter<string> output, int maxBlockSize = 102400)
        {
            _output = output;
            _maxBlockSize = maxBlockSize;
            _streamBitReader = new StreamBitReader(input);
        }

        public bool DecompressNext()
        {
            var blockLength = (int)_streamBitReader.Read(32);
            if (blockLength == 0) return false;
            var numberSize = _streamBitReader.ReadByte(8);
            var expSize = _streamBitReader.ReadByte(8);
            var numbers = new char[blockLength][];
            for (var i = 0; i < numbers.Length; i++)
            {
                numbers[i] = new char[numberSize + 1 + expSize];
            }

            var memStream = new MemoryStream(); 

            for (var i = 0; i < numberSize; i++) //Stream pos needs to be 6 here
            {
                for (var j = 0; j < blockLength; j++)
                {
                    var token = _streamBitReader.ReadByte(4);
                    if (token > 10)
                    {
                        var nbBlockSize = token - 10;
                        var blockSize = _streamBitReader.ReadByte((byte)nbBlockSize);
                        var symbol = _streamBitReader.ReadByte(4);
                        for (var k = 0; k < blockSize; k++)
                        {
                            memStream.WriteByte((byte)(symbol + 48));
                        }
                    }
                    else
                    {
                        memStream.WriteByte((byte)(token + 48));
                    }
                }
            }
            memStream.Position = 0;
            for (var i = 0; i < numberSize; i++)
            {
                for (var j = 0; j < blockLength; j++)
                {
                    var chr = (char)memStream.ReadByte();
                    if(chr == ':') continue;
                    numbers[j][i] = chr;
                }
            }
            for (var i = 0; i < blockLength; i++)
            {
                numbers[i][numberSize] = 'e';
            }
            for (var i = 0; i < numberSize; i++)
            {
                for (var j = numberSize + 1; j < numberSize + 1 + expSize; j++)
                {
                    var chr = (char)memStream.ReadByte();
                    if (chr == ':') continue;
                    numbers[j][i] = chr;
                }
            }


            return true;//(int) blockLength == _maxBlockSize;
        }
    }
}
