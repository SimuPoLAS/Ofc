using System;

namespace OfcCore.Utility
{
    public static class Utility
    {
        private const int Wordbits = 32;
        public static readonly ulong[] SectionMasks;

        static Utility()
        {
            SectionMasks = new ulong[65];
            for (var i = 0; i < SectionMasks.Length - 1; i++)
            {
                SectionMasks[i] = (ulong)(Math.Pow(2, i) - 1);
            }
            SectionMasks[64] = ulong.MaxValue;
        }

        public static ulong GetMaxValue(byte bits) => SectionMasks[bits];

        public static byte GetNeededBits(this long x)
        {
            //var y = x & (x - 1);

            //y |= -y;
            //y >>= (Wordbits - 1);
            //x |= (x >> 1);
            //x |= (x >> 2);
            //x |= (x >> 4);
            //x |= (x >> 8);
            //x |= (x >> 16);

            //return (byte)(NumBitsSet(x) - 1 - y);

            if (x == 0) return 1;

            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);

            return NumBitsSet(x);
        }

        public static byte NumBitsSet(long x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);

            return (byte)(x & 0x0000003f);
        }

        /// <summary>
        /// Reads a vaint from the StreamBitReader
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="initialSectionBits">The number of bits after which the first "should-continue-bit" appears</param>
        /// <param name="extendedSectionBits">The number of bits after which the next "should-continue-bit" appears</param>
        /// <returns></returns>
        public static ulong ReadVarint(StreamBitReader stream, byte initialSectionBits, byte extendedSectionBits)
        {
            var offset = initialSectionBits;
            var data = stream.Read(initialSectionBits);
            var hasNext = stream.ReadByte(1);
            while (hasNext == 1)
            {
                var sectionData = stream.Read(extendedSectionBits);
                hasNext = stream.ReadByte(1);
                data |= sectionData << offset;
                offset += extendedSectionBits;
            }
            return data;
        }

        /// <summary>
        /// Writes a varint to the StreamBitWriter
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <param name="initialSectionBits">The number of bits after which the first "should-continue-bit" appears</param>
        /// <param name="extendedSectionBits">The number of bits after which the next "should-continue-bit" appears</param>
        public static void WriteVarint(StreamBitWriter stream, ulong data, byte initialSectionBits, byte extendedSectionBits)
        {
            stream.Write(data, initialSectionBits);
            data = data >> initialSectionBits;
            while (data > 0)
            {
                var d = (data << 1) | 1;
                stream.Write(d, (byte)(extendedSectionBits + 1));
                data = data >> extendedSectionBits;
            }
            stream.WriteByte(0, 1);
        }
    }
}
