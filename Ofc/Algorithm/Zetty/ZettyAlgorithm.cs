namespace Ofc.Algorithm.Zetty
{
    using System;
    using System.IO;
    using Ofc.Algorithm.Integration;
    using Ofc.Algorithm.Rounding;
    using Ofc.Core;

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

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<OfcNumber> reporter, int width)
        {
            throw new NotImplementedException();
        }
    }
}
