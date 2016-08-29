using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ofc.Core;

namespace Ofc.Algorithm.RoundingDigits
{
    public class ConvertingReporter<T> : IReporter<string>
    {
        private readonly IReporter<T> _nextReporter;
        private readonly IConverter<T> _converter;
        public IConfiguaration Configuaration => _nextReporter.Configuaration;

        public ConvertingReporter(IReporter<T> nextReporter, IConverter<T> converter)
        {
            _nextReporter = nextReporter;
            _converter = converter;
        }

        public void Dispose()
        {
            _nextReporter.Dispose();
        }

        public void Finish()
        {
            _nextReporter.Finish();
        }

        public void Flush()
        {
            _nextReporter.Flush();
        }

        public void Report(string value)
        {
            _nextReporter.Report(_converter.FromString(value));
        }

        public void Report(string[] values, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(values[i]);
            }
        }
    }
}
