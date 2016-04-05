using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OfcAlgorithm.Integration;
using OfcCore;

namespace OfcAlgorithm.Rounding
{
    public class RounderAlgorithm : IAlgorithm<OfcNumber>
    {
        private readonly IAlgorithm<OfcNumber> _embeddedAlgorithm;

        public string Id => _embeddedAlgorithm.Id;
        public string Name => _embeddedAlgorithm.Name;
        public Version Version => _embeddedAlgorithm.Version;

        public RounderAlgorithm([NotNull]IAlgorithm<OfcNumber> embeddedAlgorithm)
        {
            _embeddedAlgorithm = embeddedAlgorithm;
        }

        public bool SupportsDimension(int width, int height)
        {
            return _embeddedAlgorithm.SupportsDimension(width, height);
        }

        public IReporter<OfcNumber> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            return new RounderReporter(_embeddedAlgorithm.Compress(target, configuaration, output, width, height), configuaration);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<OfcNumber> reporter)
        {
            _embeddedAlgorithm.Decompress(target, configuaration, input, reporter);
        }
    }
}
