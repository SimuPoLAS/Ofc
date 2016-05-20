namespace Ofc.Algorithm.Blocky.Method
{
    using Ofc.Algorithm.Integration;
    using Ofc.Util;

    abstract class DecompressionMethod
    {
        protected readonly BlockyMetadata Metadata;

        protected DecompressionMethod(BlockyMetadata metadata)
        {
            Metadata = metadata;
        }

        public abstract int Read(IOfcNumberWriter writer, Block block, StreamBitReader reader);

        /// <summary>
        /// Exclusive IsBlock
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static Block ReadDefaultBlockHeader(StreamBitReader reader, BlockyMetadata metadata)
        {
            var block = new Block();

            // ReSharper disable once AssignmentInConditionalExpression
            if (block.HasExponent = reader.ReadByte(1) > 0)
            {
                var negative = reader.ReadByte(1) > 0;
                block.Exponent = (short)reader.Read(metadata.MaxNeededBitsExponent);
                if (negative)
                    block.Exponent *= -1;
            }

            // ReSharper disable once AssignmentInConditionalExpression
            if (block.HasPattern = reader.ReadByte(1) > 0)
            {
                block.Pattern = (Block.PatternType)reader.ReadByte(2);
            }

            block.Length = reader.ReadByte(8);
            return block;
        }

        /// <summary>
        /// Reads a Single Value from its binary representation without the control bit (isBlock)
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static OfcNumber ReadSingleValueWithoutControlBit(StreamBitReader reader, BlockyMetadata metadata)
        {
            var value = new OfcNumber();
            if (!metadata.IsAbsolute)
                value.IsNegative = reader.ReadByte(1) > 0;
            else
                value.IsNegative = metadata.IsNegative;
            value.Number = (long)reader.Read(metadata.MaxNeededBitsNumber);
            var isExpNegative = reader.ReadByte(1) > 0;
            value.Exponent = (short)reader.Read(metadata.MaxNeededBitsExponent); // Bug: Potentually reading exp even though NoExp is set. Same in writing method! (Ineffizient)
            if (isExpNegative)
                value.Exponent *= -1;
            return value;
        }
    }
}
