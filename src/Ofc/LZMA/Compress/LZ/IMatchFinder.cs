// IMatchFinder.cs

namespace Ofc.LZMA.Compress.LZ
{
    internal interface IMatchFinder : IInWindowStream
	{
		void Create(uint historySize, uint keepAddBufferBefore,
				uint matchMaxLen, uint keepAddBufferAfter);
		uint GetMatches(uint[] distances);
		void Skip(uint num);
	}
}
