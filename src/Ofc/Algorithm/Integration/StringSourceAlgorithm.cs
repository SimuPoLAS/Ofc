using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ofc.Core;

namespace Ofc.Algorithm.RoundingDigits
{
    public class StringSourceAlgorithm<T> : IAlgorithm<string>
    {
        public string Id => _algorithm.Id;
        public string Name => _algorithm.Name;
        public Version Version => _algorithm.Version;
        private readonly IConverter<T> _converter;
        private readonly IAlgorithm<T> _algorithm;

        public StringSourceAlgorithm(IConverter<T> converter, IAlgorithm<T> algorithm)
        {
            _converter = converter;
            _algorithm = algorithm;
        }


        public bool SupportsDimension(int width, int height) => _algorithm.SupportsDimension(width, height);

        public IReporter<string> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height)
        {
            return new ConvertingReporter<T>(_algorithm.Compress(target, configuaration, output, width, height), _converter);
        }

        public void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<string> reporter, int width)
        {
            _algorithm.Decompress(target, configuaration, input, new ReverseConvertingReporter<T>(reporter, _converter),width);
        }
    }
}
