using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OfcAlgorithm.Integration;
using OfcCore;

namespace OfcAlgorithm.Rounding
{
    public static class Rounder
    {
        /// <summary>
        /// Readjusts the numbers so that they are easier to compress. Loses some precision (of course)
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="config"></param>
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
            var currentMax = currentMin;
            var currentMaxOfc = numbers[0];

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
                    currentMax = numberRec;
                    if (numberRec - currentMin > epsilonTwice)
                    {
                        if (currentStart != i - 1)
                            AdjustNumbers(numbers, currentStart, i, currentMaxOfc, epsilonOfc);
                        currentStart = i;
                        currentMin = numberRec;
                        currentMaxOfc = number;
                    }
                }

                if (numberRec < currentMin)
                {
                    currentMin = numberRec;
                    if (currentMax - numberRec > epsilonTwice)
                    {
                        if (currentStart != i - 1)
                            AdjustNumbers(numbers, currentStart, i, currentMaxOfc, epsilonOfc);
                        currentStart = i;
                        currentMax = numberRec;
                        currentMaxOfc = number;
                    }
                }
            }

            //using (var writer = new StreamWriter(new FileStream(new Random().Next(0, 100) + "raw123.txt", FileMode.OpenOrCreate)))
            //{
            //    for (var i = 0; i < numbers.Count; i++)
            //    {
            //        writer.WriteLine(numbers[i].Reconstructed);
            //    }
            //}
        }

        /// <summary>
        /// Sets all values in the given List within the range from index to index2 to currentMax - epsilonOfc
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="index"></param>
        /// <param name="index2"></param>
        /// <param name="currentMax"></param>
        /// <param name="epsilonOfc"></param>
        private static void AdjustNumbers(List<OfcNumber> numbers, int index, int index2, OfcNumber currentMax, OfcNumber epsilonOfc)
        {
            var allNum = currentMax - epsilonOfc;
            for (var j = index; j < index2; j++)
            {
                numbers[j] = allNum;
            }
        }
    }
}
