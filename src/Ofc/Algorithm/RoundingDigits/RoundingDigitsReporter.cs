namespace Ofc.Algorithm.RoundingDigits
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Ofc.Core;

    public class RoundingDigitsReporter : IReporter<double>
    {
        private readonly int _decimalDigits;
        public IConfiguaration Configuaration { get; }
        private readonly StreamWriter _outStream;
        private List<double> _values = new List<double>();

        public RoundingDigitsReporter(Stream outStream, int decimalDigits)
        {
            _decimalDigits = decimalDigits;
            _outStream = new StreamWriter(outStream);
        }

        public void Dispose()
        {
        }

        public void Finish()
        {
            _outStream.WriteLine(_values.Count);
            for (var i = 0; i < _values.Count; i++)
            {
                _outStream.WriteLine(string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F}", _values[i]));
            }
            _outStream.Flush();
        }

        public void Flush()
        {
            _outStream.Flush();
        }

        public void Report(double value)
        {
            _values.Add(value);
        }

        public void Report(double[] values, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(values[offset + i]);
            }
        }
    }
}