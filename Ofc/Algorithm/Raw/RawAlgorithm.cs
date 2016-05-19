using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ofc.Core;

namespace Ofc.Algorithm.Raw
{
    public class RawAlgorithm : IAlgorithm<string>
    {
        public string Id { get; }
        public string Name { get; }
        public Version Version { get; }
        public bool SupportsDimension(int width, int height)
        {
            return true;
        }

        public IReporter<string> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            return new RawReporter(output);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<string> reporter, int width)
        {

        }
    }
}
