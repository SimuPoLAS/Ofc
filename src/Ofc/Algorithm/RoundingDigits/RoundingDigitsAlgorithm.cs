using Ofc.Algorithm.Integration;

namespace Ofc.Algorithm.RoundingDigits
{
    using System;
    using System.IO;
    using Ofc.Core;

    public class RoundingDigitsAlgorithm : IAlgorithm<string>
    {
        private readonly IAlgorithm<OfcNumber> _next;
        public string Id => "RND2";
        public string Name => "Rounding Digits";
        public Version Version => new Version(1, 0, 0, 0);

        public RoundingDigitsAlgorithm(IAlgorithm<OfcNumber> next)
        {
            _next = next;
        }

        public bool SupportsDimension(int width, int height)
        {
            return _next.SupportsDimension(width, height);
        }

        public IReporter<string> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            var nextReporter = _next.Compress(target, configuaration, output, width, height);
            return new RoundingDigitsReporter(configuaration.Get<int>("roundingdecimals"), nextReporter);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<string> reporter, int width)
        {
            _next.Decompress(target, configuaration, input, new OfcConvertingReporter(reporter), width);
        }
    }
}