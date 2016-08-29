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
        private readonly StreamBitReader _streamBitReader;
        private readonly Stream _stream;

        public ZettyDecompression(Stream input, IReporter<string> output)
        {
            _output = output;
            _stream = input;
            _streamBitReader = new StreamBitReader(input);
        }

        public bool DecompressNext()
        {
            var blockLength = (int)_streamBitReader.Read(32);
            if (blockLength == 0) return false; // The compression will write 0 as int32 at the end - a block with 0 length = the end
            var numbers = new char[blockLength][];
            var numberLengths = new int[blockLength];
            var expLengths = new int[blockLength];
            var averageNumberLength = _streamBitReader.ReadByte(8);
            var averageExpLength = _streamBitReader.ReadByte(8);

            #region Reconstructing value lengths from bit mask and initializing numbers array
            for (var i = 0; i < blockLength; i++)
            {
                var isSmallerThanAvg = _streamBitReader.ReadByte(1) * -2 + 1; // * -2 + 1 transforms a 0 - 1 isNegative bool into a -1 or 1 multiplier ;)
                var diff = 0;
                while (_streamBitReader.ReadByte(1) == 0) diff++;
                numberLengths[i] = averageNumberLength + diff * isSmallerThanAvg;
            }

            for (var i = 0; i < blockLength; i++)
            {
                var isSmallerThanAvg = _streamBitReader.ReadByte(1) * -2 + 1; // * -2 + 1 transforms a 0 - 1 isNegative bool into a -1 or 1 multiplier ;)
                var diff = 0;
                while (_streamBitReader.ReadByte(1) == 0) diff++;
                expLengths[i] = averageExpLength + diff * isSmallerThanAvg;

                var thisNumLength = numberLengths[i];
                if (expLengths[i] > 0)
                    thisNumLength += expLengths[i] + 1;
                numbers[i] = new char[thisNumLength];
            }
            #endregion


            var changed = true;
            for (var digitIndex = 0; changed; digitIndex++)
            {
                changed = false;
                for (var i = 0; i < blockLength; i++)
                {
                    if (numberLengths[i] > digitIndex)
                    {
                        changed = true;
                        numbers[i][digitIndex] = (char)_stream.ReadByte();
                    }
                }
            }

            for (var i = 0; i < blockLength; i++)
            {
                if (expLengths[i] > 0)
                {
                    numbers[i][numberLengths[i]] = 'e';
                }
            }


            changed = true;
            for (var digitIndex = 0; changed; digitIndex++)
            {
                changed = false;
                for (var i = 0; i < blockLength; i++)
                {
                    if (expLengths[i] > digitIndex)
                    {
                        changed = true;
                        numbers[i][numberLengths[i] + 1 + digitIndex] = (char)_stream.ReadByte();
                    }
                }
            }

            for (var i = 0; i < numbers.Length; i++)
            {
                _output.Report(new string(numbers[i]));
            }
            return true;
        }
    }
}
