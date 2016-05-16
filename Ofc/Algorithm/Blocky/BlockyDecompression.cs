using Methods = Ofc.Algorithm.Blocky.Blockfinding.Blockfinding.Methods;

namespace Ofc.Algorithm.Blocky
{
    using System;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.Algorithm.Blocky.Method;
    using Ofc.Algorithm.Blocky.Method.FloatSimmilar;
    using Ofc.Algorithm.Blocky.Method.NumbersNoExp;
    using Ofc.Algorithm.Blocky.Method.PatternOffset;
    using Ofc.Algorithm.Blocky.Method.PatternPingPong;
    using Ofc.Algorithm.Blocky.Method.PatternSame;
    using Ofc.Algorithm.Integration;
    using Ofc.Core;
    using Ofc.Util;

    class BlockyDecompression : IOfcNumberWriter
    {
        private readonly StreamBitReader _bitReader;
        private readonly IReporter<OfcNumber> _writer;
        public readonly BlockyMetadata Metadata;
        private readonly DecompressionMethod[] _decompressionMethods = new DecompressionMethod[(int)Methods.Count];
        private readonly IOfcNumberWriter _numberWriter;

        public BlockyDecompression([NotNull]Stream reader, [NotNull]IReporter<OfcNumber> target) : this(reader)
        {
            _writer = target;
            _numberWriter = this;
        }

        public BlockyDecompression([NotNull]Stream reader, [NotNull]IOfcNumberWriter writer) : this(reader)
        {
            _numberWriter = writer;
        }

        protected BlockyDecompression([NotNull]Stream reader)
        {
            _bitReader = new StreamBitReader(reader);
            Metadata = BlockyMetadata.FromBitStream(_bitReader);

            _decompressionMethods[(int)Methods.PatternSame] = new PatternSameDecompression(Metadata);
            _decompressionMethods[(int)Methods.PatternPingPong] = new PatternPingPongDecompression(Metadata);
            _decompressionMethods[(int)Methods.FloatSimmilar] = new FloatSimmilarDecompression(Metadata);
            _decompressionMethods[(int)Methods.NumbersNoExp] = new NumbersNoExpDecompression(Metadata);
            _decompressionMethods[(int)Methods.PatternOffset] = new PatternOffsetDecompression(Metadata);

            if (_bitReader.ReadByte(1) > 0) // use huffman
            {
                throw new NotImplementedException();
            }
        }


        public void Decompress()
        {
            var valueCount = 0;
            while (valueCount < Metadata.ValueCount)
            {
                if (_bitReader.ReadByte(1) > 0) // isBlock
                {
                    var block = DecompressionMethod.ReadDefaultBlockHeader(_bitReader, Metadata);
                    var method = GetMethodForBlock(block); // Get decompressor class for block type
                    //((DummyReporter)_writer).FileStream.WriteLine(method.GetType().Name + " Start");
                    valueCount += method.Read(_numberWriter, block, _bitReader);
                    //((DummyReporter)_writer).FileStream.WriteLine(method.GetType().Name + " End");
                }
                else
                {
                    _numberWriter.Write(DecompressionMethod.ReadSingleValueWithoutControlBit(_bitReader, Metadata));
                    valueCount++;
                }
            }
            _writer?.Flush();
        }

        private DecompressionMethod GetMethodForBlock(Block block)
        {
            if (!block.HasPattern) return block.HasExponent ? _decompressionMethods[(int)Methods.FloatSimmilar] : _decompressionMethods[(int)Methods.NumbersNoExp];
            switch (block.Pattern)
            {
                case Block.PatternType.Same:
                    return _decompressionMethods[(int)Methods.PatternSame];
                case Block.PatternType.Offset:
                    return _decompressionMethods[(int)Methods.PatternOffset];
                case Block.PatternType.Pingpong:
                    return _decompressionMethods[(int)Methods.PatternPingPong];
                case Block.PatternType.Reserved:
                    throw new NotImplementedException("Invalid pattern type: " + block.Pattern);
                default:
                    throw new NotImplementedException("Pattern type not implemented!");
            }
        }

        //public bool CheckIntegrity()
        //{

        //}

        public void Write(OfcNumber value)
        {
            _writer.Report(value);
        }
    }
}
