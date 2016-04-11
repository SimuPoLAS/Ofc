using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LZMA.Core.Compatibility;

namespace LZMA.Core.Helper
{
    public class Helper
    {
        private static void CompressLZMA(Stream inStream, Stream outStream)
        {
            SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();

            // Write the encoder properties
            coder.WriteCoderProperties(outStream);

            // Write the decompressed file size.
            outStream.Write(BitConverter.GetBytes(inStream.Length), 0, 8);

            // Encode the file.
            coder.Code(inStream, outStream, inStream.Length, -1, null);
            outStream.Flush();
            outStream.Close();
        }

        private static void DecompressFileLZMA(Stream inStream, Stream outStream)
        {
            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            // Read the decoder properties
            byte[] properties = new byte[5];
            inStream.Read(properties, 0, 5);

            // Read in the decompress file size.
            byte[] fileLengthBytes = new byte[8];
            inStream.Read(fileLengthBytes, 0, 8);
            long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            coder.SetDecoderProperties(properties);
            coder.Code(inStream, outStream, inStream.Length, fileLength, null);
            outStream.Flush();
            outStream.Close();
        }
    }
}
