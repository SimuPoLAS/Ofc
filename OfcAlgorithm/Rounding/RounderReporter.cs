using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OfcAlgorithm.Integration;
using OfcCore;
using OfcCore.Configurations;

namespace OfcAlgorithm.Rounding
{
    public class RounderReporter : IReporter<OfcNumber>
    {
        private readonly IReporter<OfcNumber> _nextReporter;
        public IConfiguaration Configuaration { get; }
        private readonly List<OfcNumber> _numbers = new List<OfcNumber>(); //Todo: needs estimate for capacity argument! 

        /// <summary>
        /// A reporter that rounds the numbers given to him, and gives them to the next reporter
        /// </summary>
        /// <param name="nextReporter"></param>
        /// <param name="config"></param>
        public RounderReporter([NotNull] IReporter<OfcNumber> nextReporter, [NotNull]IConfiguaration config)
        {
            _nextReporter = nextReporter;
            Configuaration = config;
        }

        public void Dispose()
        {

        }

        public void Finish()
        {
            Rounder.Round(_numbers, Configuaration);
            _nextReporter.Report(_numbers.ToArray(), 0, _numbers.Count);
            _nextReporter.Finish();
        }

        public void Flush()
        {
            _nextReporter.Flush();
        }

        public void Report(OfcNumber value)
        {
            _numbers.Add(value);
        }

        public void Report(OfcNumber[] values, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(values[i]);
            }
        }
    }
}
