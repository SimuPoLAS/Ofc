using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OfcAlgorithm.Integration;
using OfcCore;

namespace OfcAlgorithm.Rounding
{
    public static class Rounder
    {
        public static void Round([NotNull]List<OfcNumber> numbers, [NotNull]IConfiguaration config)
        {
            if (numbers.Count < 2) return;
            if (!config.Has("RoundingMin") || !config.Has("RoundingMax") || !config.Has("RoundingEpsilon")) throw new ArgumentException("Invalid config numbers (Rounding)");

            var min = config.Get<double>("RoundingMin");
            var max = config.Get<double>("RoundingMax");

            if (min >= max) throw new ArgumentException("Invalid config numbers");


            var minOfc = OfcNumber.Parse(min.ToString(CultureInfo.InvariantCulture));
            var maxOfc = OfcNumber.Parse(max.ToString(CultureInfo.InvariantCulture));
            var epsilon = config.Get<double>("RoundingEpsilon");
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
