namespace Ofc.IO
{
    using System.IO;

    internal interface IDataWriter
    {
        Stream BaseStream { get; }


        void Write(byte[] buffer, int offset, int amount);

        void WriteBytes(byte[] value);

        void WriteSByte(sbyte value);

        void WriteByte(byte value);

        void WriteShort(short value);

        void WriteUShort(ushort value);

        void WriteInt(int value);

        void WriteUInt(uint value);

        void WriteLong(long value);

        void WriteULong(ulong value);

        void WriteFloat(float value);

        void WriteDouble(double value);

        void WriteDecimal(decimal value);

        void WriteBool(bool value);

        void WriteChar(char value);

        void Flush();
    }
}