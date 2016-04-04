using System;
using System.IO;

namespace OfcCore.Utility
{
    public class StreamBitWriter : IDisposable
    {
        public readonly Stream Stream;
        private byte _buffer;
        private byte _offset;

        public StreamBitWriter(Stream stream)
        {
            Stream = stream;
        }

        public void Write(ulong data, byte count)
        {
            do
            {
                var bitsLeft = 8 - _offset;
                if (bitsLeft > count)
                {
                    _buffer |= (byte)((data & Utility.SectionMasks[count]) << _offset);
                    _offset += count;
                    return;
                }
                _buffer |= (byte)((data & Utility.SectionMasks[bitsLeft]) << _offset);
                Stream.WriteByte(_buffer);
                _offset = 0;
                _buffer = 0;
                count -= (byte)bitsLeft;
                data = data >> bitsLeft;
            } while (count > 0);
        }

        public void WriteByte(byte data, byte count)
        {
            Write(data, count);
            return;
#if DEBUG
            if (((long)data).GetNeededBits() > count)
            {
                //throw new InvalidOperationException("You are writing data with less bits than needed!");
            }
#endif
            var bitsLeft = 8 - _offset;
            if (bitsLeft > count)
            {
                _buffer |= (byte)((data & Utility.SectionMasks[count]) << _offset);
                _offset += count;
                return;
            }
            _buffer |= (byte)((data & Utility.SectionMasks[bitsLeft]) << _offset);
            Stream.WriteByte(_buffer);
            _buffer = (byte)(data >> bitsLeft);
            _offset = (byte)((bitsLeft + count) % 8);
        }

        public void Flush()
        {
            if (_buffer == 0 && _offset == 0) return;
            Stream.WriteByte(_buffer);
            _buffer = 0;
            _offset = 0;
        }

        public void Dispose()
        {
            Flush();
            Stream.Dispose(); //Bug
        }
    }
}
