namespace Ofc.Algorithm.Blocky.Blockfinding
{
    /// <summary>
    /// Pre-calculates the sizes of various (block) headers
    /// </summary>
    class HeaderSizes
    {
        // Common Block headers
        public const int GlobalExponentNb = 4;
        public const int GlobalNumberNb = 6;
        public const int GlobalIsAbsolute = 1;
        public const int Sign = 1;
        public const int IsBlock = 1;
        public const int BlockHasExponent = 1;
        public const int BlockHasPattern = 1;
        public const int BlockPatternType = 2;
        public const int BlockOverrideGlobalNb = 1;
        public const int BlockLength = 8;

        // Global headers
        public readonly int GlobalIsAbsoluteNegative;
        public readonly int BlockIsAbsolute;
        public readonly int BlockIsAbsoluteNegative;
        public readonly int BlockOverriddenNb;
        public readonly int BlockExponent;

        // Special block headers
        public readonly int StandardBlockHeader;
        public readonly int StandardBlockFloatSimmilar;
        public readonly int StandardBlockNumbersNoExp;
        public readonly int StandardSingleValue;
        public readonly int StandardBlockPatternOffset;
        public readonly int StandardBlockPatternSame;
        public readonly int StandardBlockPatternPingPong;

        
        public HeaderSizes(BlockyMetadata metadata)
        {
            GlobalIsAbsoluteNegative = metadata.IsAbsolute ? 1 : 0;
            BlockExponent = metadata.MaxNeededBitsExponent;
            BlockIsAbsolute = metadata.IsAbsolute ? 0 : 1;
            BlockIsAbsoluteNegative = BlockIsAbsolute;
            BlockOverriddenNb = metadata.MaxNeededBitsNeededBitsNumber;

            StandardSingleValue = IsBlock + GlobalNumberNb + (metadata.IsAbsolute ? 0 : Sign) + (metadata.NoExponent ? 0 : Sign + GlobalExponentNb);

            StandardBlockHeader = IsBlock + BlockHasExponent + BlockHasPattern  + BlockLength;
            StandardBlockFloatSimmilar = StandardBlockHeader + Sign + BlockExponent + BlockOverrideGlobalNb + BlockIsAbsolute + BlockIsAbsoluteNegative;
            StandardBlockNumbersNoExp = StandardBlockHeader + BlockOverrideGlobalNb + BlockIsAbsolute + BlockIsAbsoluteNegative;
            StandardBlockPatternOffset = StandardBlockHeader + (StandardSingleValue - IsBlock) * 2 + BlockPatternType;
            StandardBlockPatternSame = StandardBlockHeader + StandardSingleValue - IsBlock + BlockPatternType;
            StandardBlockPatternPingPong = StandardBlockPatternOffset + 8; // 8 = repeat count
        }


    }
}
