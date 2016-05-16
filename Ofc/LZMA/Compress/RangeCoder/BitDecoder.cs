namespace Ofc.LZMA.Compress.RangeCoder
{
    internal struct BitDecoder
    {
        public const int KNumBitModelTotalBits = 11;
        public const uint KBitModelTotal = 1 << KNumBitModelTotalBits;
        private const int KNumMoveBits = 5;

        private uint _prob;

        public void UpdateModel(int numMoveBits, uint symbol)
        {
            if (symbol == 0)
                _prob += (KBitModelTotal - _prob) >> numMoveBits;
            else
                _prob -= _prob >> numMoveBits;
        }

        public void Init() { _prob = KBitModelTotal >> 1; }

        public uint Decode(Decoder rangeDecoder)
        {
            var newBound = (rangeDecoder.Range >> KNumBitModelTotalBits) * _prob;
            if (rangeDecoder.Code < newBound)
            {
                rangeDecoder.Range = newBound;
                _prob += (KBitModelTotal - _prob) >> KNumMoveBits;
                if (rangeDecoder.Range < Decoder.KTopValue)
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }
                return 0;
            }
            else
            {
                rangeDecoder.Range -= newBound;
                rangeDecoder.Code -= newBound;
                _prob -= _prob >> KNumMoveBits;
                if (rangeDecoder.Range < Decoder.KTopValue)
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }
                return 1;
            }
        }
    }
}