using OfcCore.Utility;

namespace OfcAlgorithm.Blocky.Method.PatternPingPong
{
    class PatternPingPongDecompression : DecompressionMethod
    {
        public PatternPingPongDecompression(BlockyMetadata metadata) : base(metadata)
        {
        }

        public override int Read(IOfcNumberWriter writer, Block block, StreamBitReader reader)
        {
            var val1 = ReadSingleValueWithoutControlBit(reader, Metadata);
            var val2 = ReadSingleValueWithoutControlBit(reader, Metadata);

            var ppLength = reader.ReadByte(8);
            var total = ppLength * block.Length;

            for (var i = 0; i < total; i++)
            {
                writer.Write(i % (ppLength * 2) >= ppLength ? val2 : val1);
            }

            return total;
        }
    }
}
