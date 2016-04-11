using System;
using System.IO;
using System.Text;

namespace OfcCore.Utility
{
    public class StreamBitReader : IDisposable
    {
        private readonly Stream _stream;
        private byte _buffer;
        private byte _offset;

        public StreamBitReader(Stream stream)
        {
            _stream = stream;
            _buffer = (byte)_stream.ReadByte();
        }

        public ulong Read(byte count)
        {
            if (_offset == 8)
            {
                _buffer = (byte)_stream.ReadByte();
                _offset = 0;
            }
            ulong data = 0;
            var offset = 0;
            do
            {
                var bitsLeft = 8 - _offset;
                if (bitsLeft >= count)
                {
                    data |= (_buffer & Utility.SectionMasks[count]) << offset;
                    _offset += count;
                    _buffer = (byte)(_buffer >> count);
                    return data;
                }
                data |= ((ulong)_buffer << offset);
                count -= (byte)bitsLeft;
                offset += bitsLeft;
                _offset = 0;
                _buffer = (byte)_stream.ReadByte();
            } while (count > 0);
            return data;
        }

        [Obsolete]
        public byte ReadByte(byte count)
        {
            return (byte)Read(count);
            var bitsLeft = 8 - _offset;
            var data = _buffer;
            if (count < bitsLeft)
            {
                _offset += count;
                _buffer = (byte)(_buffer >> count);
                return (byte)(data & Utility.SectionMasks[count]);
            }
            _buffer = (byte)_stream.ReadByte();
            _offset = (byte)(count - bitsLeft);
            data |= (byte)(_buffer & Utility.SectionMasks[_offset] << bitsLeft);
            return data;
        }

        public void Dispose()
        {
            //Stream.Dispose(); //Bug
        }
    }
}
