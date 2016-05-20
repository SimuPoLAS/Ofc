// OutBuffer.cs

namespace Ofc.LZMA.Common
{
    using Ofc.LZMA.Compatibility;

    public class OutBuffer
	{
	    private readonly byte[] _mBuffer;
	    private uint _mPos;
	    private readonly uint _mBufferSize;
	    private System.IO.Stream _mStream;
	    private ulong _mProcessedSize;

		public OutBuffer(uint bufferSize)
		{
			_mBuffer = new byte[bufferSize];
			_mBufferSize = bufferSize;
		}

		public void SetStream(System.IO.Stream stream) { _mStream = stream; }
		public void FlushStream() { _mStream.Flush(); }
		public void CloseStream() { _mStream.Close(); }
		public void ReleaseStream() { _mStream = null; }

		public void Init()
		{
			_mProcessedSize = 0;
			_mPos = 0;
		}

		public void WriteByte(byte b)
		{
			_mBuffer[_mPos++] = b;
			if (_mPos >= _mBufferSize)
				FlushData();
		}

		public void FlushData()
		{
			if (_mPos == 0)
				return;
			_mStream.Write(_mBuffer, 0, (int)_mPos);
			_mPos = 0;
		}

		public ulong GetProcessedSize() { return _mProcessedSize + _mPos; }
	}
}
