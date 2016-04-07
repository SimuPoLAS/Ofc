using OfcAlgorithm.Blocky.Method;

namespace OfcAlgorithm.Blocky
{
    struct Block
    {
        public enum PatternType { Same, Offset, Pingpong, Reserved }
        public enum SavingGrade { Exp, NoExp, Pattern }

        public readonly int Index;
        public bool HasExponent;
        public bool HasPattern;
        public PatternType Pattern;

        public short Exponent; // _context.MaxNeededBitsExponent
        public bool OverrideGlobalNb; // 1b
        public byte NeededBits; // GetNeededBits(_context.MaxNeededBitsNumber)
        public byte Length; // 8b
        public bool AbsoluteSign, IsSignNegative; // 1b, 1b, when !_Context.isAbsoluteNumber
        public long BiggestNumber;

        public CompressionMethod Method;

        public bool IsValid => Length > 0;
        public static readonly Block InvalidBlock = default(Block);


        public Block(int index, short exponent, byte nb, bool isSignNegative, long biggestNumber, Blockfinding.Blockfinding context, Blockfinding.Blockfinding.Methods method, bool hasPattern)
        {
            Index = index;
            HasExponent = exponent != 0;
            HasPattern = hasPattern;
            Pattern = PatternType.Same;
            Method = context.GetInitializedMethod(method);
            Exponent = exponent;
            NeededBits = nb;
            AbsoluteSign = true;
            IsSignNegative = isSignNegative;
            BiggestNumber = biggestNumber;
            OverrideGlobalNb = false;
            Length = 0;
        }

        //public int GetHeaderSize(Compression context)
        //{
        //    var header = 1 + 1 + 8; // isBlock + hasExponent / hasPattern  + Length
        //    if (!context.NoExponent)
        //    {
        //        if (HasExponent)
        //            header += context.MaxNeededBitsExponent + 1; //Exp bits + sign
        //        else
        //            header++; //has pattern option
        //    }


        //    if (HasPattern)
        //    {
        //        header += 2; // pattern type
        //        switch (Pattern)
        //        {
        //            case PatternType.Same:
        //                header += context.MaxNeededBitsNumber;
        //                break;
        //            case PatternType.Offset:
        //                header += context.MaxNeededBitsNumber;
        //                break;
        //            case PatternType.Pingpong:
        //                throw new NotImplementedException();
        //                break;
        //            case PatternType.Reserved:
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    }
        //    else
        //    {
        //        if (!context.IsAbsolute)
        //            header += AbsoluteSign ? 2 : 1; // hasAbsolute + isnegative or just hasAbsolute
        //        header++; //override global nb option
        //        if (OverrideGlobalNb)
        //            header += context.MaxNeededBitsNeededBitsNumber; //override bit 
        //    }

        //    return header;


        //    //return 1 + 1 + 8 + // isBlock + hasExponen
        //    //       (HasExponent ?
        //    //           context.MaxNeededBitsExponent + (AbsoluteSign ? 2 : 1) + (OverrideGlobalNb ? context.MaxNeededBitsNeededBitsNumber + 1 : 1) : // absolute sign + nb data if overridden
        //    //            1 + (HasPattern ? 2 : // Has pattern + patternType if there
        //    //            (AbsoluteSign ? 2 : 1) + (OverrideGlobalNb ? context.MaxNeededBitsNeededBitsNumber + 1 : 1))); // no exp and no pattern. adding absolute sign + nb data if overridden
        //}

        /// <summary>
        /// Returns a "category", describing how much the block will save in the long run. Higher = better. So basically a still growing grade 2 block will always be better than a still growing grade 1 block. Keep in mind that this is a guess ...
        /// </summary>
        /// <returns>0, 1 or 2 as SavingGrade</returns>
        public SavingGrade GetSavingGrade()
        {
            if (HasPattern)
            {
                return SavingGrade.Pattern;
            } // Note: removed NoExpSmallNum due to low real-world occurences
            return HasExponent ? SavingGrade.Exp : SavingGrade.NoExp;
        }

        public bool ShouldOverrideNb(BlockyMetadata metadata)
        {
            return metadata.MaxNeededBitsNeededBitsNumber < (metadata.MaxNeededBitsNumber - NeededBits) * Length && !HasPattern;
        }

        /// <remarks>This will set OverrideGlobalNb but not the NeededBits</remarks>
        /// <param name="metadata"></param>
        /// <param name="newNb"></param>
        /// <returns>The bit difference that would occur when changing the NeededBits in the header to newNb</returns>
        public int DifferenceWithNb(BlockyMetadata metadata, ref byte newNb) //Bug: wrong calculatons if nb > maxNb ?
        {
            if (metadata.MaxNeededBitsNeededBitsNumber < (metadata.MaxNeededBitsNumber - newNb) * Length) // If the space we'd save in the header by not overriding the nb is smaller than the space we'd save at the values because of the overriden nb # TL;DR: If it's worth to override the nb in the header
            {
                if (!OverrideGlobalNb)
                {
                    OverrideGlobalNb = true;
                    return (NeededBits - newNb) * Length - metadata.MaxNeededBitsNeededBitsNumber; // The bits we'll save with the header change - the bits we'll waste by overriding the nb in the header (adds some overhead ...)
                }
            }
            else if (OverrideGlobalNb) // If the nb are overriden but it's not worth to override them (which is why we got to 'else' ...)
            {
                OverrideGlobalNb = false;
                newNb = metadata.MaxNeededBitsNumber; // Bug: moved this from the if above. Not sure if i have missed something or fixed a bug :S
                return (NeededBits - newNb) * Length + metadata.MaxNeededBitsNeededBitsNumber; // The bits we'll save with the header change + the bits we'll save by not overriding the nb in the header (removes some overhead ...)
            }
            return (NeededBits - newNb) * Length; // the bits we'll save if the nb were already overriden and have changed value
        }

        public override string ToString()
        {
            return $"Index: {Index}, Length: {Length}, Pattern: {(HasPattern ? Pattern.ToString() : false.ToString())}, Exponent: {(HasExponent ? Exponent.ToString() : false.ToString())}, Nb: {(OverrideGlobalNb ? NeededBits.ToString() : false.ToString())}, Method: {Method?.GetType().Name}";
        }
    }
}
