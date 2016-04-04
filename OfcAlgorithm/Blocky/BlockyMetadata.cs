using System.Collections.Generic;
using System.Linq;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky
{
    class BlockyMetadata
    {
        public int ValueCount;
        public byte MaxNeededBitsNumber;
        public byte MaxNeededBitsNeededBitsNumber;
        public byte MaxNeededBitsExponent;
        public bool IsAbsolute;
        public bool IsNegative;
        public bool NoExponent;
        public ulong LargestPossibleValue;

        protected BlockyMetadata() { }

        public static BlockyMetadata FromBitStream(StreamBitReader reader)
        {
            var metadata = new BlockyMetadata { ValueCount = (int)reader.Read(31) };
            // ReSharper disable once AssignmentInConditionalExpression
            if (metadata.IsAbsolute = reader.ReadByte(1) > 0)
                metadata.IsNegative = reader.ReadByte(1) > 0;
            metadata.MaxNeededBitsExponent = reader.ReadByte(4);
            metadata.MaxNeededBitsNumber = reader.ReadByte(6);
            metadata.MaxNeededBitsNeededBitsNumber = Utility.GetNeededBits(metadata.MaxNeededBitsNumber);
            metadata.LargestPossibleValue = Utility.GetMaxValue(metadata.MaxNeededBitsNumber);
            return metadata;
        }

        public static BlockyMetadata FromData(List<OfcNumber> values)
        {
            var metadata = new BlockyMetadata() { IsNegative = values.First().IsNegative, IsAbsolute = true, ValueCount = values.Count };
            foreach (var ofcNumber in values)
            {
                if (ofcNumber.NeededBitsNumber > metadata.MaxNeededBitsNumber)
                {
                    metadata.MaxNeededBitsNumber = ofcNumber.NeededBitsNumber;
                    metadata.LargestPossibleValue = Utility.GetMaxValue(ofcNumber.NeededBitsNumber);
                }
                if (ofcNumber.NeededBitsExponent > metadata.MaxNeededBitsExponent) metadata.MaxNeededBitsExponent = ofcNumber.NeededBitsExponent; //Todo: this is redundant af, we should be able to calculate it from the huffman creator
                if (metadata.IsAbsolute && ofcNumber.IsNegative != metadata.IsNegative) metadata.IsAbsolute = false;
                if (metadata.NoExponent && ofcNumber.Exponent != 0) metadata.NoExponent = false;
            }
            return metadata;
        }

        public void Write(StreamBitWriter writer)
        {
            writer.Write((ulong)ValueCount, 31);
            writer.WriteByte(IsAbsolute ? (byte)1 : (byte)0, 1);
            if (IsAbsolute)
                writer.WriteByte(IsNegative ? (byte)1 : (byte)0, 1);
            writer.WriteByte(MaxNeededBitsExponent, 4);
            writer.WriteByte(MaxNeededBitsNumber, 6); // The MaxNeededBitsNeededBitsNumber and LargestPossibleNumber can be calculated from that ...
        }
    }
}
