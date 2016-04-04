using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.PatternOffset
{
    class PatternOffsetDecompression : DecompressionMethod
    {
        public PatternOffsetDecompression(BlockyMetadata metadata) : base(metadata)
        {
        }

        public override int Read(IOfcNumberWriter writer, Block block, StreamBitReader reader)
        {
            var val1 = ReadSingleValueWithoutControlBit(reader, Metadata);
            var val2 = ReadSingleValueWithoutControlBit(reader, Metadata);

            writer.Write(val1);
            writer.Write(val2);

            var difference = val2.SubtractEach(val1);

            for (var i = 1; i < block.Length - 1; i++)
            {
                writer.Write(difference.LinearMultiplyEach(i).AddEach(val2));
            }

            return block.Length;
        }
    }
}
