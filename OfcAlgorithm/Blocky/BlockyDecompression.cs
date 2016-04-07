using System;
using System.IO;
using OfcAlgorithm.Blocky.Method;
using OfcAlgorithm.Blocky.Method.FloatSimmilar;
using OfcAlgorithm.Blocky.Method.NumbersNoExp;
using OfcAlgorithm.Blocky.Method.PatternOffset;
using OfcAlgorithm.Blocky.Method.PatternPingPong;
using OfcAlgorithm.Blocky.Method.PatternSame;
using OfcAlgorithm.Integration;
using OfcAlgorithm.Integration.Dummy;
using OfcCore;
using OfcCore.Utility;
using Methods = OfcAlgorithm.Blocky.Blockfinding.Blockfinding.Methods;

namespace OfcAlgorithm.Blocky
{
    class BlockyDecompression : IOfcNumberWriter
    {
        private readonly StreamBitReader _bitReader;
        private readonly IReporter<OfcNumber> _writer;
        private readonly BlockyMetadata _metadata;

        private readonly DecompressionMethod[] _decompressionMethods = new DecompressionMethod[(int)Methods.Count];

        public BlockyDecompression(Stream reader, IReporter<OfcNumber> target)
        {
            _bitReader = new StreamBitReader(reader);
            _writer = target;
            _metadata = BlockyMetadata.FromBitStream(_bitReader);

            _decompressionMethods[(int)Methods.PatternSame] = new PatternSameDecompression(_metadata);
            _decompressionMethods[(int)Methods.PatternPingPong] = new PatternPingPongDecompression(_metadata);
            _decompressionMethods[(int)Methods.FloatSimmilar] = new FloatSimmilarDecompression(_metadata);
            _decompressionMethods[(int)Methods.NumbersNoExp] = new NumbersNoExpDecompression(_metadata);
            _decompressionMethods[(int)Methods.PatternOffset] = new PatternOffsetDecompression(_metadata);

            if (_bitReader.ReadByte(1) > 0) // use huffman
            {
                throw new NotImplementedException();
            }
        }

        public void Decompress()
        {
            var valueCount = 0;
            while (valueCount < _metadata.ValueCount)
            {
                if (_bitReader.ReadByte(1) > 0) // isBlock
                {
                    var block = DecompressionMethod.ReadDefaultBlockHeader(_bitReader, _metadata);
                    var method = GetMethodForBlock(block); // Get decompressor class for block type
                    //((DummyReporter)_writer).FileStream.WriteLine(method.GetType().Name + " Start");
                    valueCount += method.Read(this, block, _bitReader);
                    //((DummyReporter)_writer).FileStream.WriteLine(method.GetType().Name + " End");
                }
                else
                {
                    Write(DecompressionMethod.ReadSingleValueWithoutControlBit(_bitReader, _metadata));
                    valueCount++;
                }
            }
            _writer.Flush();
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
