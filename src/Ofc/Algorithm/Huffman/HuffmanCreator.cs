namespace Ofc.Algorithm.Huffman
{
    using Ofc.Util;

    class HuffmanCreator
    {
        public readonly int[] OccurenceCount; // number, occurenceCount
        private readonly short _offset;

        /// <summary>
        /// This is a collector class, you pass it all the numbers that exist so it can create an efficient huffman writer
        /// </summary>
        /// <param name="size"></param>
        public HuffmanCreator(ushort size) // Bug: not the most efficient huffman form!! Todo
        {
            OccurenceCount = new int[size];
            _offset = (short)(size / 2);
        }

        public void AddOccurence(short index)
        {
            OccurenceCount[index + _offset]++;
        }

        public void AddOccurences(short[] index)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < index.Length; i++)
            {
                OccurenceCount[index[i] + _offset]++;
            }
        }

        /// <summary>
        /// Returns the count of unique exponents
        /// </summary>
        /// <returns></returns>
        public int UniqueCount()
        {
            var length = OccurenceCount.Length;
            var count = 0;
            for (var i = 0; i < length; i++)
            {
                if (OccurenceCount[i] == 0) continue;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Returns how many bits you'd need to display the biggest exponent (calculated with offset, so that you have no negative exponents! That means it's most likely the needed bits of the biggest absolute exponent, but not always ...)
        /// </summary>
        /// <returns></returns>
        public int GetNeededBitsForBiggest()
        {
            var length = OccurenceCount.Length;
            for (var i = length - 1; i >= 0; i--)
            {
                if (OccurenceCount[i] != 0)
                    return Utility.GetNeededBits(i);
            }
            return 0;
        }

        public HuffmanWriter CreateWriter(byte defaultBitCount, StreamBitWriter writer)
        {
            var length = OccurenceCount.Length;
            var indexMapper = new byte[length]; // number, place
            var reversedIndexMapper = new uint[defaultBitCount]; // place, number
            var buffer = new uint[length]; // buffer for all places woth the same occurenceCount, temporary stuff
            var smallerThan = int.MaxValue;
            byte mapperIndex = 0;

            while (true)
            {
                var highest = 0;
                byte bufferCount = 0;

                for (uint i = 0; i < length; i++)
                {
                    var val = OccurenceCount[i];
                    if (val >= smallerThan) continue;

                    if (val > highest)
                    {
                        bufferCount = 1;
                        highest = val;
                        buffer[0] = i;
                        continue;
                    }
                    if (val == highest)
                        buffer[bufferCount++] = i;
                }

                if (bufferCount == 0 || highest == 0) break;
                smallerThan = highest;
                for (var i = 0; i < bufferCount; i++)
                {
                    reversedIndexMapper[mapperIndex] = buffer[i];
                    indexMapper[buffer[i]] = ++mapperIndex;
                    if (mapperIndex == defaultBitCount)
                        return new HuffmanWriter(indexMapper, reversedIndexMapper, defaultBitCount, _offset, writer);
                }
            }


            return new HuffmanWriter(indexMapper, reversedIndexMapper, defaultBitCount, _offset, writer);
        }
    }
}
