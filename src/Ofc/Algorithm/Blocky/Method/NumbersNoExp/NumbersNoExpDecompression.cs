namespace Ofc.Algorithm.Blocky.Method.NumbersNoExp
{
    using Ofc.Algorithm.Integration;
    using Ofc.Util;

    class NumbersNoExpDecompression : DecompressionMethod
    {
        public NumbersNoExpDecompression(BlockyMetadata metadata) : base(metadata)
        {
        }

        public override int Read(IOfcNumberWriter writer, Block block, StreamBitReader reader)
        {
            // ReSharper disable once AssignmentInConditionalExpression
            if (block.OverrideGlobalNb = reader.ReadByte(1) > 0)
            {
                block.NeededBits = reader.ReadByte(Metadata.MaxNeededBitsNeededBitsNumber);
            }

            // ReSharper disable once AssignmentInConditionalExpression
            if (!Metadata.IsAbsolute)
                if (block.AbsoluteSign = reader.ReadByte(1) > 0)
                {
                    block.IsSignNegative = reader.ReadByte(1) > 0;
                }

            var hasAbsoluteSign = block.AbsoluteSign || Metadata.IsAbsolute;
            var isNegative = block.AbsoluteSign && block.IsSignNegative || Metadata.IsAbsolute && Metadata.IsNegative;

            for (var i = 0; i < block.Length; i++)
            {
                var num = new OfcNumber();

                if (!hasAbsoluteSign)
                    isNegative = reader.ReadByte(1) > 0;

                num.Number = ((long)reader.Read(Metadata.MaxNeededBitsNumber)) * (isNegative ? -1 : 1);
                writer.Write(num);
            }

            return block.Length;
        }
    }
}
