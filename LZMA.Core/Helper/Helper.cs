using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LZMA.Core.Compatibility;
using LZMA.Core.Compress.LZMA;

namespace LZMA.Core.Helper
{
    public class Helper
    {
        private static void CompressLzma(Stream inStream, Stream outStream)
        {
            var coder = new Encoder();

            // Write the encoder properties
            coder.WriteCoderProperties(outStream);

            // Write the decompressed file size.
            outStream.Write(BitConverter.GetBytes(inStream.Length), 0, 8);

            // Encode the file.
            coder.Code(inStream, outStream, inStream.Length, -1, null);
            outStream.Flush();
            outStream.Close();
        }

        private static void DecompressFileLzma(Stream inStream, Stream outStream)
        {
            var coder = new Decoder();
            // Read the decoder properties
            var properties = new byte[5];
            inStream.Read(properties, 0, 5);

            // Read in the decompress file size.
            var fileLengthBytes = new byte[8];
            inStream.Read(fileLengthBytes, 0, 8);
            var fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            coder.SetDecoderProperties(properties);
            coder.Code(inStream, outStream, inStream.Length, fileLength, null);
            outStream.Flush();
            outStream.Close();
        }
    }
}
