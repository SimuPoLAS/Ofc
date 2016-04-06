namespace Ofc.Util
{
    using System;
    using System.Text;
    using Ofc.IO;

    internal static class Extentions
    {
        internal static void WriteId(this IDataWriter writer, int primary, int secondary)
        {
            writer.WriteId(false, primary, secondary);
        }

        internal static void WriteId(this IDataWriter writer, bool same, int primary, int secondary)
        {
            writer.WriteByte((byte)((same ? 0x80 : 0x00) | ((primary & 0x7) << 4) | (secondary & 0xF)));
        }

        internal static void WriteString(this IDataWriter writer, string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            writer.WriteVarInt(buffer.Length);
            writer.WriteBytes(buffer);
        }

        internal static void WriteVarInt(this IDataWriter writer, int value)
        {
            var v = (uint)value;
            for (; v >= 0x80; v >>= 7)
                writer.WriteByte((byte)(v | 0x80));
            writer.WriteByte((byte)v);
        }

        internal static void WriteVarLong(this IDataWriter writer, long value)
        {
            var v = (ulong)value;
            for (; v >= 0x80; v >>= 7)
                writer.WriteByte((byte)(v | 0x80));
            writer.WriteByte((byte)v);
        }

        internal static string ReadString(this IDataReader reader)
        {
            var length = reader.ReadVarInt();
            var buffer = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(buffer);
        }

        internal static int ReadVarInt(this IDataReader reader)
        {
            int count = 0, shift = 0;
            byte b;
            do
            {
                if (shift == 5 * 7) throw new FormatException();
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }

        internal static long ReadVarLong(this IDataReader reader)
        {
            long count = 0;
            var shift = 0;
            byte b;
            do
            {
                if (shift == 10 * 7) throw new FormatException();
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }
    }
}