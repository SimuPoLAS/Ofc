namespace Ofc.Algorithm.Blocky.Method.NumbersNoExp
{
    using Ofc.Algorithm.Integration;
    using Ofc.Util;

    class NumbersNoExpCompression : CompressionMethod
    {
        public NumbersNoExpCompression(Blockfinding.Blockfinding context) : base(context)
        {
        }

        public override bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff)
        {
            if (value.Exponent != 0) return false; //Todo: maybe not impossible?

            if (block.AbsoluteSign)
            {
                if (block.IsSignNegative != value.IsNegative) // Check if the new value works with the current "absolute sign" block header
                {
                    block.AbsoluteSign = false;
                    bitDiff -= block.Length - 1; // We loose 1 bit per value, because we need to write down the sign now ... but we save 1 because less block header stuffs 
                }
                else
                {
                    bitDiff++;
                }
            }

            if (value.Number > block.BiggestNumber) // If the biggest value in the block is smaller than the new one, we need to set it for future calculations to be correct
            {
                block.BiggestNumber = value.Number;
                if (block.NeededBits < value.NeededBitsNumber) // If the new number needs more bits than specified in the block header, we need to adjust that
                {
                    var nbNewBiggest = value.NeededBitsNumber;
                    bitDiff += block.DifferenceWithNb(Context.Metadata, ref nbNewBiggest); // Adds the difference in bits that comes with changing the block header. May change bigNumNb to the global-header-max! (can be worth, because no header then ...)
                    block.NeededBits = nbNewBiggest; // Need to set this after the call for future calculations to be exact
                }
            }

            block.Length++;

            bitDiff += Context.Metadata.MaxNeededBitsExponent + 1;
            if (block.OverrideGlobalNb)
                bitDiff += Context.Metadata.MaxNeededBitsNumber - block.NeededBits;
            return true;
        }

        public override void Write(StreamBitWriter writer, Block block, ref int valueIndex) //Todo: this method is redundant: same as FloatSimmilar
        {
            WriteDefaultBlockHeader(writer, block);

            if (block.OverrideGlobalNb)
            {
                writer.WriteByte(1, 1);
                writer.Write(Context.Metadata.MaxNeededBitsNumber, Context.Metadata.MaxNeededBitsNeededBitsNumber);
            }
            else
                writer.WriteByte(0, 1);

            if (!Context.Metadata.IsAbsolute)
            {
                if (block.AbsoluteSign)
                {
                    writer.WriteByte(1, 1);
                    writer.WriteByte(block.IsSignNegative ? (byte)1 : (byte)0, 1);
                }
                else
                    writer.WriteByte(0, 1);
            }

            var end = block.Index + block.Length;
            for (var i = block.Index; i < end; i++)
            {
                var value = Values[i];

                if (!block.AbsoluteSign)
                    writer.WriteByte(value.IsNegative ? (byte)1 : (byte)0, 1);

                writer.Write((ulong)value.Number, Context.Metadata.MaxNeededBitsNumber);
            }
            valueIndex += block.Length;
        }



        ///// <summary>
        ///// Changes a block from a pattern method <u><b>where either the first or the new value is the biggest value</b></u>
        ///// </summary>
        ///// <param name="block">the block to change</param>
        ///// <param name="bitDiff">the bitDifference of the block</param>
        ///// <param name="value">the new value to be added tro the block</param>
        ///// <param name="comp">reference to the compression context (needing global headers)</param>
        ///// <param name="index">the index of the new value</param>
        //public static void ChangeFromSpecialPattern(ref Block block, ref int bitDiff, OfcNumber value, Compression comp, int index)
        //{
        //    block.BiggestNumber = Math.Max(comp.Values[index - 1].Number, value.Number);
        //    block.AbsoluteSign = comp.Values[block.Index].Number < 0 == block.BiggestNumber < 0;
        //    block.IsSignNegative = value.IsNegative;
        //    bitDiff -= block.AbsoluteSign ? 2 : 1 + block.Length;

        //    var nb = block.BiggestNumber.GetNeededBits();
        //    block.NeededBits = 0; // the nb per value if we still had the pattern
        //    bitDiff += block.DifferenceWithNb(comp, ref nb) - 2; // -2 because now we have no "Pattern" option
        //    block.NeededBits = nb;
        //    block.HasPattern = false;
        //    block.Method = comp.GetInitializedMethod(Compression.Methods.NumbersNoExp);
        //    block.Length++;
        //}
    }
}
