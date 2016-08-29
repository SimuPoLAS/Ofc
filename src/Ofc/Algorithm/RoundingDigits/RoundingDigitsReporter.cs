using System;
using System.Text;
using Ofc.Algorithm.Integration;

namespace Ofc.Algorithm.RoundingDigits
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Ofc.Core;

    public class RoundingDigitsReporter : IReporter<string>
    {
        private readonly int _decimalDigits;
        private readonly IReporter<string> _nextReporter;
        public IConfiguaration Configuaration { get; }

        public RoundingDigitsReporter(int decimalDigits, IReporter<string> nextReporter)
        {
            _decimalDigits = decimalDigits;
            _nextReporter = nextReporter;
        }

        public void Dispose()
        {
            _nextReporter.Dispose();
        }

        public void Finish()
        {
            _nextReporter.Finish();
        }

        private static string RoundNumber(string number, int didgits)
        {
            if (didgits == 0) return number;

            var sb = new StringBuilder(32);
            var cursor = 0;
            var negative = false;
            var exponent = 0;
            var setDot = false;

            // extract information
            for (var i = 0; i < number.Length; i++)
            {
                var c = number[i];
                if (i == 0 && c == '-') negative = true;
                else if (i == 0 && c == '+') negative = false;
                if (c >= '0' && c <= '9') sb.Append(c);
                else if (c == '.')
                {
                    cursor = sb.Length;
                    setDot = true;
                }
                else if (c == 'e' || c == 'E')
                {
                    exponent = int.Parse(number.Substring(i + 1));
                    break;
                }
            }
            if (!setDot) cursor = sb.Length;

            // do transformation
            var rcursor = cursor + exponent;
            var split = rcursor + didgits;
            if (split < 0) return "0";
            var up = split >= 0 && split <= sb.Length - 1 && sb[split] - '5' >= 0;
            if (split < sb.Length) sb.Length = split;
            if (up)
            {
                var l = sb.Length - 1;
                for (; l >= 0; l--)
                {
                    var c = sb[l];
                    if (c == '9')
                    {
                        sb[l] = '0';
                        continue;
                    }
                    sb[l] = ++c;
                    break;
                }
                if (l < 0)
                {
                    sb.Insert(0, '1');
                    cursor++;
                }

            }
            while (sb.Length < cursor) sb.Append('0');
            if (sb.Length > cursor) sb.Insert(cursor, '.');
            if (exponent != 0) sb.Append("e" + exponent);
            if (negative) sb.Insert(0, '-');
            return sb.ToString();
        }

        public void Flush()
        {
            _nextReporter.Flush();
        }

        public void Report(string value)
        {
            _nextReporter.Report(RoundNumber(value, _decimalDigits));
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