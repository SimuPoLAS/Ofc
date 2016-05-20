namespace Ofc.Util
{
    using System;
    using System.IO;

    public class StreamBitReader : IDisposable
    {
        private readonly Stream _stream;
        private byte _buffer;
        private byte _offset;

        public StreamBitReader(Stream stream)
        {
            _stream = stream;
            _buffer = (byte) _stream.ReadByte();
        }

        private byte ReadUnalignedByte()
        {
            var value = _stream.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (byte) value;
        }

        public ulong Read(byte count)
        {
            if (_offset == 8)
            {
                _buffer = ReadUnalignedByte();
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
                    _buffer = (byte) (_buffer >> count);
                    return data;
                }
                data |= (ulong) _buffer << offset;
                count -= (byte) bitsLeft;
                offset += bitsLeft;
                _offset = 0;
                _buffer = ReadUnalignedByte();
            } while (count > 0);
            return data;
        }

        public byte ReadByte(byte count)
        {
            if (_offset == 8) return ReadUnalignedByte();
            return (byte) Read(count);
        }

        public void Dispose()
        {
            // _stream.Close(); bug to close or not to close
        }
    }
}