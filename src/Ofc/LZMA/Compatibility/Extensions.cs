namespace Ofc.LZMA.Compatibility
{
    using System.IO;

    public static class Extensions
    {
        public static void Close(this Stream stream)
        {
            stream.Dispose();
        }
    }
}
