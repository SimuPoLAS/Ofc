namespace Ofc.IO
{
    using System;
    using System.IO;

    internal class BinaryDataWriter : IDataWriter, IDisposable
    {
        internal Stream BaseStream { get; }


        private readonly byte[] _buffer = new byte[16];


        public BinaryDataWriter(Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            BaseStream = target;
        }


        public void Write(byte[] buffer, int offset, int amount)
        {
            BaseStream.Write(buffer, offset, amount);
        }

        public void WriteBytes(byte[] value)
        {
            BaseStream.Write(value, 0, value.Length);
        }

        public void WriteSByte(sbyte value)
        {
            BaseStream.WriteByte((byte) value);
        }

        public void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
        }

        public void WriteShort(short value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            BaseStream.Write(_buffer, 0, 2);
        }

        public void WriteUShort(ushort value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            BaseStream.Write(_buffer, 0, 2);
        }

        public void WriteInt(int value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            _buffer[2] = (byte) (value >> 16);
            _buffer[3] = (byte) (value >> 24);
            BaseStream.Write(_buffer, 0, 4);
        }

        public void WriteUInt(uint value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            _buffer[2] = (byte) (value >> 16);
            _buffer[3] = (byte) (value >> 24);
            BaseStream.Write(_buffer, 0, 4);
        }

        public void WriteLong(long value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            _buffer[2] = (byte) (value >> 16);
            _buffer[3] = (byte) (value >> 24);
            _buffer[4] = (byte) (value >> 32);
            _buffer[5] = (byte) (value >> 40);
            _buffer[6] = (byte) (value >> 48);
            _buffer[7] = (byte) (value >> 56);
            BaseStream.Write(_buffer, 0, 8);
        }

        public void WriteULong(ulong value)
        {
            _buffer[0] = (byte) value;
            _buffer[1] = (byte) (value >> 8);
            _buffer[2] = (byte) (value >> 16);
            _buffer[3] = (byte) (value >> 24);
            _buffer[4] = (byte) (value >> 32);
            _buffer[5] = (byte) (value >> 40);
            _buffer[6] = (byte) (value >> 48);
            _buffer[7] = (byte) (value >> 56);
            BaseStream.Write(_buffer, 0, 8);
        }

        public unsafe void WriteFloat(float value)
        {
            var t = *(uint*) &value;
            _buffer[0] = (byte) t;
            _buffer[1] = (byte) (t >> 8);
            _buffer[2] = (byte) (t >> 16);
            _buffer[3] = (byte) (t >> 24);
            BaseStream.Write(_buffer, 0, 4);
        }

        public unsafe void WriteDouble(double value)
        {
            var t = *(ulong*) &value;
            _buffer[0] = (byte) t;
            _buffer[1] = (byte) (t >> 8);
            _buffer[2] = (byte) (t >> 16);
            _buffer[3] = (byte) (t >> 24);
            _buffer[4] = (byte) (t >> 32);
            _buffer[5] = (byte) (t >> 40);
            _buffer[6] = (byte) (t >> 48);
            _buffer[7] = (byte) (t >> 56);
            BaseStream.Write(_buffer, 0, 8);
        }

        public void WriteDecimal(decimal value)
        {
            var t = decimal.GetBits(value);
            _buffer[0] = (byte) t[0];
            _buffer[1] = (byte) (t[0] >> 8);
            _buffer[2] = (byte) (t[0] >> 16);
            _buffer[3] = (byte) (t[0] >> 24);
            _buffer[4] = (byte) t[1];
            _buffer[5] = (byte) (t[1] >> 8);
            _buffer[6] = (byte) (t[1] >> 16);
            _buffer[7] = (byte) (t[1] >> 24);
            _buffer[8] = (byte) t[2];
            _buffer[9] = (byte) (t[2] >> 8);
            _buffer[10] = (byte) (t[2] >> 16);
            _buffer[11] = (byte) (t[2] >> 24);
            _buffer[12] = (byte) t[3];
            _buffer[13] = (byte) (t[3] >> 8);
            _buffer[14] = (byte) (t[3] >> 16);
            _buffer[15] = (byte) (t[3] >> 24);
            BaseStream.Write(_buffer, 0, 16);
        }

        public void WriteBool(bool value)
        {
            BaseStream.WriteByte(value ? (byte) 1 : (byte) 0);
        }

        public unsafe void WriteChar(char value)
        {
            var t = *(ushort*) &value;
            _buffer[0] = (byte) t;
            _buffer[1] = (byte) (t >> 8);
            BaseStream.Write(_buffer, 0, 2);
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