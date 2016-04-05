using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OfcAlgorithm.Integration;

namespace OfcAlgorithm.Rounding
{
    public static class Rounder
    {
        public static void Round(List<OfcNumber> numbers, IUnrandomizerConfig config)
        {
            if (numbers.Count < 2) return;
            if (config.Min >= config.Max) throw new ArgumentException("Invalid config");

            var min = config.Min;
            var max = config.Max;
            var minOfc = OfcNumber.Parse(min.ToString(CultureInfo.InvariantCulture));
            var maxOfc = OfcNumber.Parse(max.ToString(CultureInfo.InvariantCulture));
            var epsilon = config.Epsilon;
            var epsilonTwice = epsilon * 2d;
            var epsilonOfc = OfcNumber.Parse(epsilon.ToString(CultureInfo.InvariantCulture));

            var currentStart = 0;
            var currentMin = numbers[0].Reconstructed;
            var currentMinOfc = numbers[0];
            var currentMax = currentMin;
            var currentMaxOfc = currentMinOfc;

            for (var i = 1; i < numbers.Count; i++)
            {
                var number = numbers[i];
                var numberRec = number.Reconstructed;

                if (numberRec < min)
                    numbers[i] = minOfc;
                else if (numberRec > max)
                    numbers[i] = maxOfc;

                if (numberRec > currentMax)
                {
                    if (currentStart == i - 1)
                    {
                        currentStart = i;
                        continue;
                    }

                    if (numberRec - currentMin > epsilonTwice)
                    {
                        var allNum = currentMaxOfc - epsilonOfc;
                        for (var j = currentStart; j < i; j++)
                        {
                            numbers[j] = allNum;
                        }
                    }
                }

            }
        }
    }
}
