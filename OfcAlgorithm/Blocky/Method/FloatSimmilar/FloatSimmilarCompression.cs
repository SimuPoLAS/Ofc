using System;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.FloatSimmilar
{
    /// <summary>
    /// Numbers with the same exponent
    /// </summary>
    class FloatSimmilarCompression : CompressionMethod
    {

        public FloatSimmilarCompression(Blockfinding.Blockfinding context) : base(context)
        {
        }

        public override bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff)
        {
            if (block.Length == byte.MaxValue) return false;
            if (block.Exponent < value.Exponent) // The exp is bigger, which means we will set the exp equal and add stuff to the value to balance things
            {
                var expDiff = value.Exponent - block.Exponent;

                if (expDiff > 18) return false; // long can't hold 10^19 or higher
                var multiplier = (long)Math.Pow(10, expDiff);
                var newNum = value.Number;
                if (value.Number != 0)
                {
                    if ((long)Context.Metadata.LargestPossibleValue / value.Number < multiplier) return false; // overflow check

                    newNum = value.Number * multiplier; // This balances the expDiff we subtraced from Exponent
                }

                var newNb = newNum.GetNeededBits(); // Recalculate the needed bits for the number, because that may have changed
                if (value.NeededBitsNumber > Context.Metadata.MaxNeededBitsNumber) return false; //Todo: you can't write with more than max bits (even in blocks when the nb are overridden), because of "nbnbmaxnumber" ... maybe we can work our way around that? Currently we just throw the block away ...

                // We don't actually apply the changes before checking if we can even write the new value ...
                value.Exponent -= (short)expDiff;
                value.NeededBitsNumber = newNb;
                value.Number = newNum;

                if (value.NeededBitsNumber > block.NeededBits) // Value cannot be added to block, we have to check what would change if we change the NeededBits in the block header!
                {
                    var nbNewNumber = value.NeededBitsNumber;
                    bitDiff += block.DifferenceWithNb(Context.Metadata, ref nbNewNumber); // Adds the difference in bits if we would change the block header. May change nbNewNumber to the global-header-max! (can be worth, because no header then ...)
                    block.NeededBits = nbNewNumber; // Need to set this after the call for future calculations to be exact
                }
                // Values[index] = value; 
            }
            else if (block.Exponent > value.Exponent) // The exp is smaller, which means we will recalculate the whole block to fit this value
            {
                var expDiff = (block.Exponent - value.Exponent);

                if (expDiff > 18) return false; // long can't hold 10^19 or higher
                var multiplier = (long)Math.Pow(10, expDiff);


                if (block.BiggestNumber > 0 && (long)Context.Metadata.LargestPossibleValue / block.BiggestNumber < multiplier) return false; // overflow check
                var newNum = block.BiggestNumber * multiplier; // Recalculate the biggest number of the blog

                block.BiggestNumber = Math.Max(value.Number, newNum);// value.Number > newNum ? value.Number : newNum; // Recalculate the biggest number of the blog
                block.Exponent = value.Exponent; // Setting the new exponent for the whole block

                var bigNumNb = Math.Max(block.BiggestNumber.GetNeededBits(), value.NeededBitsNumber); // Math.Max because the new value could be bigger than the oldest biggest value, which would give a wrong result
                if (bigNumNb > Context.Metadata.MaxNeededBitsNumber) return false; //Todo: you can't write with more than max bits (even in blocks when the nb are overridden), because of "nbnbmaxnumber" ... maybe we can work our way around that? Currently we just throw the block away ...


                if (bigNumNb > block.NeededBits) // The change in exp made the numbers bigger -> we need more Nb to store the numbers
                {
                    var diff = block.DifferenceWithNb(Context.Metadata, ref bigNumNb); // Adds the difference in bits that comes with changing the block header. May change bigNumNb to the global-header-max! (can be worth, because no header then ...)
                    bitDiff += diff;
                    block.NeededBits = bigNumNb; // Need to set this after the call for future calculations to be exact
                }
            }

            if (block.AbsoluteSign) // Check if the new value works with the current "absolute sign" block header
            {
                if (block.IsSignNegative != value.IsNegative)
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

            bitDiff += Context.Metadata.MaxNeededBitsExponent + 2;
            if (block.OverrideGlobalNb)
                bitDiff += Context.Metadata.MaxNeededBitsNumber - block.NeededBits;
            return true;
        }

        public override void Write(StreamBitWriter writer, Block block, ref int valueIndex) //Todo: this method is redundant: same as NoExp. Need more common methods
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

                if (value.Exponent > block.Exponent)
                    writer.Write((ulong)(Math.Pow(10, value.Exponent - block.Exponent) * value.Number), Context.Metadata.MaxNeededBitsNumber);
                else
                    writer.Write((ulong)value.Number, Context.Metadata.MaxNeededBitsNumber);
            }
            valueIndex += block.Length;
        }

    }
}
