namespace Ofc.LZMA.Compress.LZ
{
    internal interface IInWindowStream
    {
        void SetStream(System.IO.Stream inStream);
        void Init();
        void ReleaseStream();
        byte GetIndexByte(int index);
        uint GetMatchLen(int index, uint distance, uint limit);
        uint GetNumAvailableBytes();
    }
}