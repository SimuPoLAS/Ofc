using System;
using System.Collections.Generic;
using System.Linq;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky
{
    class BlockyMetadata
    {
        /// <summary>
        /// How much values there are
        /// </summary>
        public int ValueCount;

        /// <summary>
        /// How much bits you need to be able to represent every value
        /// </summary>
        public byte MaxNeededBitsNumber;

        /// <summary>
        /// Needed bits for the needed bits number
        /// </summary>
        public byte MaxNeededBitsNeededBitsNumber;

        /// <summary>
        /// How much bits you need to be able to represent every exponent
        /// </summary>
        public byte MaxNeededBitsExponent;

        /// <summary>
        /// If all values have the same sign
        /// </summary>
        public bool IsAbsolute;

        /// <summary>
        /// If the absolute sign is negative
        /// </summary>
        public bool IsNegative;

        /// <summary>
        /// If no value has an exponent
        /// </summary>
        public bool NoExponent;

        /// <summary>
        /// The largest possible value with the set needed bits
        /// </summary>
        public ulong LargestPossibleValue;

        protected BlockyMetadata() { }

        /// <summary>
        /// Reads the metadata in its binary form
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the metadata from the given values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static BlockyMetadata FromData(List<OfcNumber> values)
        {
            if(values.Count == 0) throw new InvalidOperationException("Cannot gather compression metadata with 0 values!");
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


        /// <summary>
        /// Writes the metadata in binary form
        /// </summary>
        /// <param name="writer"></param>
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
