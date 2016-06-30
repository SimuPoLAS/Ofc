using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ofc.Algorithm.Integration;
using Ofc.Core;

namespace Ofc.Algorithm.RoundingDigits
{
    public class OfcConvertingReporter : IReporter<OfcNumber>
    {
        private readonly IReporter<string> _next;

        public OfcConvertingReporter(IReporter<string> next)
        {
            _next = next;
        }

        public void Dispose()
        {
            _next.Dispose();
        }

        public IConfiguaration Configuaration { get; }
        public void Finish()
        {
            _next.Finish();
        }

        public void Flush()
        {
            _next.Flush();
        }

        public void Report(OfcNumber value)
        {
            _next.Report(value.Reconstructed.ToString(CultureInfo.InvariantCulture));
        }

        public void Report(OfcNumber[] values, int offset, int amount)
        {
            for (var i = offset; i < amount + offset; i++)
            {
                Report(values[i]);
            }
        }
    }
}
