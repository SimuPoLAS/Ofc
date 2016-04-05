using System;
using System.IO;
using OfcAlgorithm.Integration;
using OfcCore;

namespace OfcAlgorithm.Blocky.Integration
{
    public class BlockyAlgorithm : IAlgorithm<OfcNumber>
    {
        public string Id => "BLKY";

        public string Name => "Blocky";

        public Version Version => new Version(0, 1);

        public IReporter<OfcNumber> Compress(IFile file, IConfiguaration config, Stream writer, int width, int elements)
        {
            if (width == 1)
                return new BlockyCompression(elements, writer, config);

            var compressions = new IReporter<OfcNumber>[width];
            for (var i = 0; i < compressions.Length; i++)
                compressions[i] = new BlockyCompression(elements, writer, config);
            return new CompressionSplitter(compressions);
        }


        public void Decompress(IFile file, IConfiguaration config, Stream reader, IReporter<OfcNumber> reporter)
        {
            new BlockyDecompression(reader, reporter).Decompress();
        }

        public bool SupportsDimension(int width, int elements)
        {
            return width > 0;
        }

        public static void SetBlockfindingDebugConsoleEnabled(bool enabled)
        {
#if DEBUG
            Blockfinding.Blockfinding.SetDebugEnabled(enabled);
#endif
        }
    }
}