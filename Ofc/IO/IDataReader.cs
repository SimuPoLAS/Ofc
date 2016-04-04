namespace Ofc.IO
{
    using System.IO;

    internal interface IDataReader
    {
        Stream BaseStream { get; }


        int Read(byte[] buffer, int offset, int amount);

        byte[] ReadBytes(int amount);

        sbyte ReadSByte();

        byte ReadByte();

        short ReadShort();

        ushort ReadUShort();

        int ReadInt();

        uint ReadUInt();

        long ReadLong();

        ulong ReadULong();

        float ReadFloat();

        double ReadDouble();

        decimal ReadDecimal();

        bool ReadBool();

        char ReadChar();

        void Flush();
    }
}