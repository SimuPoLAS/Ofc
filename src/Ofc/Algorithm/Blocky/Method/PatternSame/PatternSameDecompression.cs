namespace Ofc.Algorithm.Blocky.Method.PatternSame
{
    using Ofc.Util;

    class PatternSameDecompression : DecompressionMethod
    {
        public PatternSameDecompression(BlockyMetadata metadata) : base(metadata)
        {
        }

        public override int Read(IOfcNumberWriter writer, Block block, StreamBitReader reader)
        {
            var value = ReadSingleValueWithoutControlBit(reader, Metadata);
            for (var i = 0; i < block.Length; i++)
            {
                writer.Write(value);
            }

            return block.Length;
        }
    }
}
