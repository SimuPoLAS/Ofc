using System;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.PatternOffset
{
    /// <summary>
    /// A linear function pattern
    /// </summary>
    class PatternOffsetCompression : CompressionMethod
    {
        public PatternOffsetCompression(Blockfinding.Blockfinding context) : base(context)
        {
        }

        public override bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff) // The block length will always be at least 1 at this point!
        {
            if (block.Length == byte.MaxValue) return false;

            var firstValue = Values[block.Index];
            var secondValue = Values[block.Index + 1];
            var lastValue = Values[index - 1];

            if (firstValue.Number - secondValue.Number != lastValue.Number - value.Number || firstValue.Exponent - secondValue.Exponent != lastValue.Exponent - value.Exponent)
            {
                if (firstValue.Exponent == 0 && secondValue.Exponent == 0)
                {
                    bitDiff += Context.Headers.StandardBlockPatternOffset - Context.Headers.StandardBlockNumbersNoExp;
                    block.Method = Context.GetInitializedMethod(Blockfinding.Blockfinding.Methods.NumbersNoExp);
                }
                else if (firstValue.Exponent == secondValue.Exponent && secondValue.Exponent == value.Exponent)
                {
                    bitDiff += Context.Headers.StandardBlockPatternOffset - Context.Headers.StandardBlockFloatSimmilar;
                    block.Method = Context.GetInitializedMethod(Blockfinding.Blockfinding.Methods.FloatSimmilar);
                    block.Exponent = firstValue.Exponent;
                }
                else
                {
                    return false; //todo: maybe the floatsimmilar algorithm could adjust that ...
                }

                //bitDiff -= _singleValueBits * block.Length - block.Length * MaxNeededBitsExponent * 2;

                block.HasPattern = false;
                block.BiggestNumber = Math.Max(value.Number, Math.Max(firstValue.Number, lastValue.Number));

                if (firstValue.IsNegative == lastValue.IsNegative && lastValue.IsNegative == value.IsNegative)
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

                block.Length++;
                return true;
            }

            block.Length++;
            bitDiff += Context.Headers.StandardSingleValue;
            return true;
        }

        public override void Write(StreamBitWriter writer, Block block, ref int valueIndex)
        {
            WriteDefaultBlockHeader(writer, block);

            WriteSingleValueWithoutControlBit(writer, Values[block.Index]);
            WriteSingleValueWithoutControlBit(writer, Values[block.Index + 1]);

            valueIndex += block.Length;
        }

    }
}
