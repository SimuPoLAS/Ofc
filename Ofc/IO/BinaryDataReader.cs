namespace Ofc.IO
{
    using System;
    using System.IO;

    internal class BinaryDataReader : IDataReader, IDisposable
    {
        internal Stream BaseStream { get; }


        private readonly byte[] _buffer = new byte[16];


        public BinaryDataReader(Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            BaseStream = target;
        }


        private void Fill(int amount)
        {
            if (amount == 1)
            {
                var num = BaseStream.ReadByte();
                if (num == -1) throw new EndOfStreamException();
                _buffer[0] = (byte) num;
            }
            else
            {
                var o = 0;
                do
                {
                    var num = BaseStream.Read(_buffer, o, amount - o);
                    if (num == 0) throw new EndOfStreamException();
                    o += num;
                } while (o < amount);
            }
        }


        public int Read(byte[] buffer, int offset, int amount)
        {
            return BaseStream.Read(buffer, offset, amount);
        }

        public byte[] ReadBytes(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return new byte[0];
            var buffer = new byte[amount];
            var o = 0;
            do
            {
                var num = BaseStream.Read(buffer, o, amount - o);
                if (num == 0) throw new EndOfStreamException();
                o += num;
            } while (o < amount);
            return buffer;
        }

        public sbyte ReadSByte()
        {
            Fill(1);
            return (sbyte) _buffer[0];
        }

        public byte ReadByte()
        {
            var num = BaseStream.ReadByte();
            if (num == -1) throw new EndOfStreamException();
            return (byte) num;
        }

        public short ReadShort()
        {
            Fill(2);
            return (short) (_buffer[0] | _buffer[1] << 8);
        }

        public ushort ReadUShort()
        {
            Fill(2);
            return (ushort) (_buffer[0] | _buffer[1] << 8);
        }

        public int ReadInt()
        {
            Fill(4);
            return _buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24;
        }

        public uint ReadUInt()
        {
            Fill(4);
            return (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
        }

        public long ReadLong()
        {
            Fill(8);
            return (long) (uint) (_buffer[4] | _buffer[5] << 8 | _buffer[6] << 16 | _buffer[7] << 24) << 32 | (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
        }

        public ulong ReadULong()
        {
            Fill(8);
            return (ulong) (uint) (_buffer[4] | _buffer[5] << 8 | _buffer[6] << 16 | _buffer[7] << 24) << 32 | (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
        }

        public unsafe float ReadFloat()
        {
            Fill(4);
            var t = (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            return *(float*) &t;
        }

        public unsafe double ReadDouble()
        {
            Fill(8);
            var t = (ulong) (uint) (_buffer[4] | _buffer[5] << 8 | _buffer[6] << 16 | _buffer[7] << 24) << 32 | (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            return *(double*) &t;
        }

        public decimal ReadDecimal()
        {
            Fill(16);
            return new decimal(new[] {_buffer[0] | (_buffer[1] << 8) | (_buffer[2] << 16) | (_buffer[3] << 24), _buffer[4] | (_buffer[5] << 8) | (_buffer[6] << 16) | (_buffer[7] << 24), _buffer[8] | (_buffer[9] << 8) | (_buffer[10] << 16) | (_buffer[11] << 24), _buffer[12] | (_buffer[13] << 8) | (_buffer[14] << 16) | (_buffer[15] << 24)});
        }

        public bool ReadBool()
        {
            Fill(1);
            if (_buffer[0] > 1) throw new FormatException();
            return _buffer[0] != 0;
        }

        public unsafe char ReadChar()
        {
            Fill(2);
            var t = (ushort) (_buffer[0] | _buffer[1] << 8);
            return *(char*) &t;
        }

        public void Flush()
        {
            BaseStream.Flush();
        }

        public void Dispose()
        {
        }
    }
}