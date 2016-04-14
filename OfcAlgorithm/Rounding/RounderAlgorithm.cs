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
    /// <summary>
    /// Just rounding numbers with the Rounder. Basically returns a RoundingReporter, and is able to pass the rounded numbers to the next algorithm
    /// </summary>
    public class RounderAlgorithm : IAlgorithm<OfcNumber>
    {
        private readonly Stream _stream;
        private readonly IAlgorithm<OfcNumber> _embeddedAlgorithm;
        private readonly bool _routeMode;

        public string Id => _embeddedAlgorithm.Id;
        public string Name => _embeddedAlgorithm.Name;
        public Version Version => _embeddedAlgorithm.Version;

        public RounderAlgorithm([NotNull]IAlgorithm<OfcNumber> embeddedAlgorithm)
        {
            _embeddedAlgorithm = embeddedAlgorithm;
            _routeMode = true;
        }

        public RounderAlgorithm([NotNull]Stream stream)
        {
            _stream = stream;
            _routeMode = false;
        }

        public bool SupportsDimension(int width, int height)
        {
            return _embeddedAlgorithm.SupportsDimension(width, height);
        }

        public IReporter<OfcNumber> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            if (_routeMode)
                return new RounderReporter(_embeddedAlgorithm.Compress(target, configuaration, output, width, height), configuaration);
            return new RounderReporter(_stream, configuaration);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<OfcNumber> reporter, int width)
        {
            _embeddedAlgorithm.Decompress(target, configuaration, input, reporter, width);
        }
    }
}
