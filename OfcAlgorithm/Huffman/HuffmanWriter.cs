using OfcCore.Utility;

namespace OfcAlgorithm.Huffman
{
    class HuffmanWriter
    {
        private readonly byte[] _indexMapper;
        private readonly uint[] _reversedIndexMapper;
        private readonly byte _defaultBitCount;
        private readonly StreamBitWriter _writer;
        private static readonly uint[] OccurenceNum = { 1, 2, 4, 8, 16, 32, 64, 126, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824, 2147483648 };


        public HuffmanWriter(byte[] indexMapper, uint[] reversedIndexMapper, byte defaultBitCount, short offset, StreamBitWriter writer)
        {
            _indexMapper = indexMapper;
            _reversedIndexMapper = reversedIndexMapper;
            _defaultBitCount = defaultBitCount;
            _writer = writer;
        }

        /// <summary>
        /// If it's better to write [isHuffman][NormalNum/Huffman] or just [NormalNum]
        /// </summary>
        /// <param name="numberCount"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public bool IsBetterThanDefault(int numberCount, HuffmanCreator creator)
        {
            var totalSum = numberCount * _defaultBitCount;
            var sumReduction = 0;
            for (var i = 0; i < _defaultBitCount; i++)
                sumReduction += creator.OccurenceCount[_reversedIndexMapper[i]] * (_defaultBitCount - i + 1);
            return totalSum > totalSum - sumReduction + numberCount;
        }

        /// <summary>
        /// Writes a value to the stored bit-writer. Will write the value as huffman representation if possible. Otherwise, the value is written as normal value
        /// </summary>
        /// <param name="num"></param>
        public void Write(ushort num)
        {
            var occurencePlace = _indexMapper[num];
            if (occurencePlace == 0)
            {
                _writer.WriteByte(0, 1); // isHuffman flag
                _writer.Write(num, _defaultBitCount); // Writing the normal value, NO huffman representation 
            }
            else
            {
                _writer.WriteByte(1, 1); // isHuffman flag
                _writer.Write(OccurenceNum[occurencePlace], occurencePlace); // Writing huffman representation
            }
        }

        /// <summary>
        /// Writes the metadata it needs to reconstruct all numbers to the stored bit-writer
        /// </summary>
        public void WriteDictionary()
        {
            var length = _reversedIndexMapper.Length;
            for (ushort i = 0; i < length; i++) // Dictionary length = maxnb exp
                _writer.Write(_reversedIndexMapper[i], _defaultBitCount);
        }
    }
}
