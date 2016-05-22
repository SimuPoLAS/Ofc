namespace Ofc.LZMA.Compress.RangeCoder
{
    using Ofc.LZMA.Compatibility;

    internal class Encoder
	{
		public const uint KTopValue = 1 << 24;

	    private System.IO.Stream _stream;

		public ulong Low;
		public uint Range;
	    private uint _cacheSize;
	    private byte _cache;

	    private long _startPosition;

		public void SetStream(System.IO.Stream stream)
		{
			_stream = stream;
		}

		public void ReleaseStream()
		{
			_stream = null;
		}

		public void Init()
		{
			_startPosition = _stream.Position;

			Low = 0;
			Range = 0xFFFFFFFF;
			_cacheSize = 1;
			_cache = 0;
		}

		public void FlushData()
		{
			for (var i = 0; i < 5; i++)
				ShiftLow();
		}

		public void FlushStream()
		{
			_stream.Flush();
		}

		public void CloseStream()
		{
			_stream.Close();
		}

		public void Encode(uint start, uint size, uint total)
		{
			Low += start * (Range /= total);
			Range *= size;
			while (Range < KTopValue)
			{
				Range <<= 8;
				ShiftLow();
			}
		}

		public void ShiftLow()
		{
			if ((uint)Low < 0xFF000000 || (uint)(Low >> 32) == 1)
			{
				var temp = _cache;
				do
				{
					_stream.WriteByte((byte)(temp + (Low >> 32)));
					temp = 0xFF;
				}
				while (--_cacheSize != 0);
				_cache = (byte)((uint)Low >> 24);
			}
			_cacheSize++;
			Low = (uint)Low << 8;
		}

		public void EncodeDirectBits(uint v, int numTotalBits)
		{
			for (var i = numTotalBits - 1; i >= 0; i--)
			{
				Range >>= 1;
				if (((v >> i) & 1) == 1)
					Low += Range;
				if (Range < KTopValue)
				{
					Range <<= 8;
					ShiftLow();
				}
			}
		}

		public void EncodeBit(uint size0, int numTotalBits, uint symbol)
		{
			var newBound = (Range >> numTotalBits) * size0;
			if (symbol == 0)
				Range = newBound;
			else
			{
				Low += newBound;
				Range -= newBound;
			}
			while (Range < KTopValue)
			{
				Range <<= 8;
				ShiftLow();
			}
		}

		public long GetProcessedSizeAdd()
		{
			return _cacheSize +
				_stream.Position - _startPosition + 4;
			// (long)Stream.GetProcessedSize();
		}
	}
}
