namespace Ofc.Algorithm.RoundingDigits
{
    using System;
    using System.IO;
    using Ofc.Core;

    public class RoundingDigitsAlgorithm : IAlgorithm<double>
    {
        public string Id => "RND2";
        public string Name => "Rounding Digits";
        public Version Version => new Version(1, 0, 0, 0);


        public bool SupportsDimension(int width, int height)
        {
            return true;
        }

        public IReporter<double> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            return new RoundingDigitsReporter(output, configuaration.Get<int>("roundingdecimals"));
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<double> reporter, int width)
        {
            var reader = new StreamReader(input);
            var count = int.Parse(reader.ReadLine());
            for (var i = 0; i < count; i++)
            {
                reporter.Report(double.Parse(reader.ReadLine()));
            }
        }
    }
}