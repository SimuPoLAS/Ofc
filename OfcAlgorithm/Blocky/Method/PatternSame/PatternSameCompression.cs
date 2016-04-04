using System;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.PatternSame
{
    class PatternSameCompression : CompressionMethod
    {
        public PatternSameCompression(Blockfinding.Blockfinding context) : base(context)
        {
        }

        public override bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff)
        {
            if (block.Length == byte.MaxValue)
                return false;

            if (block.Length == 0)
            {
                block.Length++;
                bitDiff += Context.Headers.StandardSingleValue;
                return true;
            }

            var patternDiff = value.Number - Values[index - 1].Number != 0 || value.Exponent - Values[index - 1].Exponent != 0;

            if (patternDiff)
            {
                if (block.Length == 1)
                {
                    block.Length++;
                    block.Pattern = Block.PatternType.Offset;
                    bitDiff += Context.Headers.StandardBlockPatternSame - Context.Headers.StandardBlockPatternOffset;
                    block.Method = Context.GetInitializedMethod(Blockfinding.Blockfinding.Methods.PatternOffset);
                    return true;
                }

                var firstValue = Values[block.Index];

                //bitDiff -= _singleValueBits * block.Length - block.Length * MaxNeededBitsExponent * 2;

                block.HasPattern = false;
                block.BiggestNumber = Math.Max(firstValue.Number, value.Number);

                if (firstValue.IsNegative == value.IsNegative)
                {
                    block.AbsoluteSign = true;
                    block.IsSignNegative = value.IsNegative;
                    //bitDiff--; commented this out, as the isNegative is now in the default header!
                }
                else
                {
                    bitDiff -= block.Length;
                }

                var nb = block.BiggestNumber.GetNeededBits();
                block.NeededBits = 0; // the nb per value if we still had the pattern
                bitDiff += block.DifferenceWithNb(Context.Metadata, ref nb);
                block.NeededBits = nb;

                if (firstValue.Exponent == 0 && value.Exponent == 0)
                {
                    block.Length++;
                    bitDiff += Context.Headers.StandardBlockPatternSame - Context.Headers.StandardBlockNumbersNoExp;
                    block.Method = Context.GetInitializedMethod(Blockfinding.Blockfinding.Methods.NumbersNoExp);
                    return true;
                }
                var oldMethod = block.Method;
                block.Method = Context.GetInitializedMethod(Blockfinding.Blockfinding.Methods.FloatSimmilar);
                block.Exponent = firstValue.Exponent;
                block.HasExponent = true;
                bitDiff += Context.Headers.StandardBlockPatternSame - Context.Headers.StandardBlockFloatSimmilar;

                if (firstValue.Exponent == value.Exponent)
                {
                    block.Length++;
                    return true;
                }

                var result = block.Method.ProcessValue(ref block, value, index, ref bitDiff);
                if (!result)
                {
                    block.HasPattern = true;
                    block.Method = oldMethod;
                }
                return result;
            }

            block.Length++;

            bitDiff += Context.Headers.StandardSingleValue;
            return true;
        }

        public override void Write(StreamBitWriter writer, Block block, ref int valueIndex)
        {
            WriteDefaultBlockHeader(writer, block);

            WriteSingleValueWithoutControlBit(writer, Values[block.Index]);
            valueIndex += block.Length;
        }

        
    }
}
