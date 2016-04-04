using System;
using System.IO;
using OfcAlgorithm.Blocky.Blockfinding;
using OfcAlgorithm.Integration;
using OfcCore;

namespace OfcAlgorithm.Herbert.Integration
{
    public class HerbertAlgorithm : IAlgorithm<OfcNumber>
    {
        public string Id => "FEIH";

        public string Name => "Herbert";

        public Version Version => new Version(0, 1);



        public void Decompress(IFile file, IConverter<OfcNumber> converter, Stream reader, IReporter<OfcNumber> reporter, int width, int elements)
        {
            throw new NotImplementedException();
        }

        public bool SupportsDimension(int width, int elements)
        {
            return width > 0;
        }

        public static void SetBlockfindingDebugConsoleEnabled(bool enabled)
        {
#if DEBUG
            Blockfinding.SetDebugEnabled(enabled);
#endif
        }

        public IReporter<OfcNumber> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            if (width == 1)
                return new HerbertCompression(target, height, output);

            var compressions = new IReporter<OfcNumber>[width];
            for (var i = 0; i < compressions.Length; i++)
                compressions[i] = new HerbertCompression(height, output);
            return new CompressionSplitter(compressions);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<OfcNumber> reporter)
        {
            throw new NotImplementedException();
        }
    }
}