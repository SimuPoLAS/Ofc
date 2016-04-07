using System;
using System.Collections.Generic;
using OfcAlgorithm.Integration;
using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.PatternPingPong
{
    /// <summary>
    /// 111000111000111000111000111000
    /// </summary>
    class PatternPingPongCompression : CompressionMethod
    {
        public readonly List<PatternPingPongMetadata> PingPongPatternLengths = new List<PatternPingPongMetadata>(); // Index = n'th. pingping block, value = length until value changes
        private int _probabilityMetadataIndex;

        public struct PatternPingPongMetadata
        {
            public readonly int BlockIndex;
            public readonly byte Length;

            public PatternPingPongMetadata(byte length, int blockIndex)
            {
                Length = length;
                BlockIndex = blockIndex;
            }
        }

        public PatternPingPongCompression(Blockfinding.Blockfinding context) : base(context)
        {
        }

        public override bool ProcessValue(ref Block block, OfcNumber value, int index, ref int bitDiff)
        {
            throw new InvalidOperationException("The PingPong pattern should not be processing values, as it is created in post-compression optimisation. This CompressionMethod is just for writing!");
            //todo: Dirty!
            //if (value.Exponent != 0)
            //    return false;

            ////if (block.Length == 0) return true;

            //var patternDiff = value.Number - Values[index - 1].Number;

            //if (patternDiff != 0 && value.Number == block.PatternProperties.PatternNum1)
            //{
            //    if (block.PatternProperties.RepeatCount == byte.MaxValue)
            //        return false;
            //    block.PatternProperties.RepeatCount++;
            //}

            //var patternTurnDiff = block.Length % block.PatternProperties.Length; // 0 = at end of pattern, will repeat now. else = in pattern
            //if ((patternDiff != 0 && (patternTurnDiff != 0 || value.Number != block.PatternProperties.PatternNum1 && value.Number != block.PatternProperties.PatternNum2))
            //    || patternDiff == 0 && patternTurnDiff == 0)
            ////|| (block.Pattern == Block.PatternType.Increasing && patternDiff != 1))
            //{
            //    if (index - block.Index >= byte.MaxValue) return false; 
            //    var nb = block.NeededBits;
            //    block.NeededBits = 0; // the nb per value if we still had the pattern
            //    bitDiff += block.DifferenceWithNb(Compression, ref nb) - 2; // -2 because now we have no "Pattern" option
            //    block.HasPattern = false;
            //}
            //else
            //{
            //    bitDiff += patternTurnDiff == 0 ? block.PatternProperties.Length * 1000 : -1000; //This is a cheap fix to make blocks with a not finished pingpongpattern "invalid". Everytime it finishes, the cost gets refunded
            //}

            //bitDiff += _singleValueBits;
            //return true;
        }

        public override void Write(StreamBitWriter writer, Block block, ref int valueIndex)
        {
            WriteDefaultBlockHeader(writer, block);

            if (_probabilityMetadataIndex > PingPongPatternLengths.Count)
                _probabilityMetadataIndex = 0;

            var meta = PingPongPatternLengths[_probabilityMetadataIndex++];
            if (meta.BlockIndex != block.Index)
            {
                for (var i = 0; i < PingPongPatternLengths.Count; i++)
                {
                    if (PingPongPatternLengths[i].BlockIndex == block.Index)
                    {
                        meta = PingPongPatternLengths[i];
                        _probabilityMetadataIndex = i++;
                        break;
                    }
                }
            }

            WriteSingleValueWithoutControlBit(writer, Values[block.Index]);
            WriteSingleValueWithoutControlBit(writer, Values[block.Index + meta.Length]);

            writer.WriteByte(meta.Length, 8);
            valueIndex += meta.Length * block.Length;
        }

    }
}
