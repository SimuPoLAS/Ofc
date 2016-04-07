namespace Ofc.Util
{
    using System;
    using System.Text;
    using Ofc.IO;

    /// <summary>
    ///     Provides extention methods for easier use of IDataWriter and IDataReader.
    /// </summary>
    internal static class Extentions
    {
        /// <summary>
        ///     Writes a special encoded id to the stream.
        /// </summary>
        /// <param name="writer">Target output writer.</param>
        /// <param name="primary">Primary group of the id.</param>
        /// <param name="secondary">Secondary group of the id.</param>
        internal static void WriteId(this IDataWriter writer, int primary, int secondary)
        {
            writer.WriteId(false, primary, secondary);
        }

        /// <summary>
        ///     Writes a special encoded id to the stream.
        /// </summary>
        /// <param name="writer">Target output writer.</param>
        /// <param name="same">The Same-Flag of the id.</param>
        /// <param name="primary">Primary group of the id.</param>
        /// <param name="secondary">Secondary group of the id.</param>
        internal static void WriteId(this IDataWriter writer, bool same, int primary, int secondary)
        {
            writer.WriteByte((byte) ((same ? 0x80 : 0x00) | ((primary & 0x7) << 4) | (secondary & 0xF)));
        }

        /// <summary>
        ///     Writes a varint prefixed UTF-8 encoded sting to the writer.
        /// </summary>
        /// <param name="writer">Target output writer.</param>
        /// <param name="value">Target String which will be written to the output.</param>
        internal static void WriteString(this IDataWriter writer, string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            writer.WriteVarInt(buffer.Length);
            writer.WriteBytes(buffer);
        }

        /// <summary>
        ///     Writes a special encoded integer to the output.
        /// </summary>
        /// <param name="writer">Target output writer.</param>
        /// <param name="value">Target int value which will be written to the output.</param>
        internal static void WriteVarInt(this IDataWriter writer, int value)
        {
            var v = (uint) value;
            for (; v >= 0x80; v >>= 7)
                writer.WriteByte((byte) (v | 0x80));
            writer.WriteByte((byte) v);
        }

        /// <summary>
        ///     Writes a special encoded integer to the output.
        /// </summary>
        /// <param name="writer">Target output writer.</param>
        /// <param name="value">Target long value which will be written to the output.</param>
        internal static void WriteVarLong(this IDataWriter writer, long value)
        {
            var v = (ulong) value;
            for (; v >= 0x80; v >>= 7)
                writer.WriteByte((byte) (v | 0x80));
            writer.WriteByte((byte) v);
        }

        /// <summary>
        ///     Reads a varint prefixed UTF-8 encoded string.
        /// </summary>
        /// <param name="reader">Target reader.</param>
        /// <returns>The read string.</returns>
        internal static string ReadString(this IDataReader reader)
        {
            var length = reader.ReadVarInt();
            var buffer = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        ///     Reads a special encoded integer.
        /// </summary>
        /// <param name="reader">Target reader.</param>
        /// <returns>The read integer.</returns>
        internal static int ReadVarInt(this IDataReader reader)
        {
            int count = 0, shift = 0;
            byte b;
            do
            {
                if (shift == 5*7) throw new FormatException();
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }

        /// <summary>
        ///     Reads a special encoded integer.
        /// </summary>
        /// <param name="reader">Target reader.</param>
        /// <returns>The read long value.</returns>
        internal static long ReadVarLong(this IDataReader reader)
        {
            long count = 0;
            var shift = 0;
            byte b;
            do
            {
                if (shift == 10*7) throw new FormatException();
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }
    }
}