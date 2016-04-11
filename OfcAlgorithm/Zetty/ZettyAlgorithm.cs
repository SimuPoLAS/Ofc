using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfcAlgorithm.Integration;
using OfcAlgorithm.Integration.Dummy;
using OfcAlgorithm.Rounding;
using OfcCore;

namespace OfcAlgorithm.Zetty
{
    public class ZettyAlgorithm : IAlgorithm<OfcNumber>
    {
        private readonly IConfiguaration _config;
        public string Id { get; }
        public string Name { get; }
        public Version Version { get; }

        public ZettyAlgorithm(IConfiguaration config)
        {
            _config = config;
        }

        public bool SupportsDimension(int width, int height)
        {
            return true;
        }

        public IReporter<OfcNumber> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            return new RounderReporter(output, _config);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<OfcNumber> reporter)
        {
            throw new NotImplementedException();
        }
    }
}
