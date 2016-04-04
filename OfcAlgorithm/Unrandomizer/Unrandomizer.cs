using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OfcAlgorithm.Integration;

namespace OfcAlgorithm
{
    public static class Unrandomizer
    {
        public static void Unrandomize(OfcNumber[] numbers, IUnrandomizerConfig config)
        {
            throw new NotImplementedException();
            if (numbers.Length < 2) return;
            if (config.Min >= config.Max) throw new ArgumentException("Invalid config");

            var min = config.Min;
            var max = config.Max;
            var minOfc = OfcNumber.Parse(min.ToString(CultureInfo.InvariantCulture));
            var maxOfc = OfcNumber.Parse(max.ToString(CultureInfo.InvariantCulture));
            var epsilon = config.Epsilon;
            var epsilonTwice = epsilon * 2d;

            var currentStart = 0;
            var currentMin = numbers[0].Reconstructed;
            var currentMinOfc = numbers[0];
            var currentMax = currentMin;
            var currentMaxOfc = currentMinOfc;

            for (var i = 1; i < numbers.Length; i++)
            {
                var number = numbers[i];
                var numberRec = number.Reconstructed;

                if (numberRec < min)
                    numbers[i] = minOfc;
                else if (numberRec > max)
                    numbers[i] = maxOfc;

                if (numberRec > currentMax)
                {
                    if (numberRec - currentMin > epsilonTwice)
                    {
                        for (var j = currentStart; j < i; j++)
                        {
                          
                        }
                    }
                }

            }
        }
    }
}
