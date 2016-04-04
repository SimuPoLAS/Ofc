using System;
using System.Collections.Generic;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method
{
    abstract class CompressionMethod
    {
        protected readonly List<OfcNumber> Values;
        protected readonly Blockfinding.Blockfinding Context;

        protected CompressionMethod(Blockfinding.Blockfinding context)
        {
            Values = context.Values;
            Context = context;
        }

        public abstract bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff);
        public abstract void Write(StreamBitWriter writer, Block block, ref int valueIndex);

        protected void WriteDefaultBlockHeader(StreamBitWriter writer, Block block)
        {
            writer.WriteByte(1, 1); // IsBlock
            if (block.HasExponent)
            {
                writer.WriteByte(1, 1);
                writer.WriteByte((byte)(block.Exponent < 0 ? 1 : 0), 1);
                writer.Write((ushort)Math.Abs(block.Exponent), Context.Metadata.MaxNeededBitsExponent);
            }
            else
                writer.WriteByte(0, 1);

            if (block.HasPattern)
            {
                writer.WriteByte(1, 1);
                writer.WriteByte((byte)block.Pattern, 2);
            }
            else
                writer.WriteByte(0, 1);

            writer.WriteByte(block.Length, 8);
        }



        /// <summary>
        /// Writes Number + (Sign) + Exponent + Sign
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        protected void WriteSingleValueWithoutControlBit(StreamBitWriter writer, OfcNumber value)
        {
            if (!Context.Metadata.IsAbsolute)
                writer.WriteByte(value.IsNegative ? (byte)1 : (byte)0, 1);
            writer.Write((ulong)Math.Abs(value.Number), Context.Metadata.MaxNeededBitsNumber);
            writer.WriteByte(value.Exponent < 0 ? (byte)1 : (byte)0, 1);
            writer.Write((ushort)Math.Abs(value.Exponent), Context.Metadata.MaxNeededBitsExponent);
        }
    }
}
