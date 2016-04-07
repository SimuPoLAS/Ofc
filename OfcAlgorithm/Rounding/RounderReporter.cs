using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OfcAlgorithm.Integration;
using OfcCore;
using OfcCore.Configurations;

namespace OfcAlgorithm.Rounding
{
    /// <summary>
    /// A reporter that rounds the numbers given to him (with the Rounder class), and gives them to the next reporter
    /// </summary>
    public class RounderReporter : IReporter<OfcNumber>
    {
        private readonly IReporter<OfcNumber> _nextReporter;
        public IConfiguaration Configuaration { get; }
        private readonly List<OfcNumber> _numbers = new List<OfcNumber>(); //Todo: needs estimate for capacity argument! 
        private readonly Stream _outStream;
        private readonly bool _routeMode;

        public RounderReporter([NotNull] IReporter<OfcNumber> nextReporter, [NotNull]IConfiguaration config)
        {
            _nextReporter = nextReporter;
            Configuaration = config;
            _routeMode = true;
        }

        public RounderReporter([NotNull] Stream outStream, [NotNull]IConfiguaration config)
        {
            _outStream = outStream;
            Configuaration = config;
            _routeMode = false;
        }

        public void Dispose()
        {

        }

        public void Finish()
        {
            Rounder.Round(_numbers, Configuaration);
            if (_routeMode)
            {
                _nextReporter.Report(_numbers.ToArray(), 0, _numbers.Count);
                _nextReporter.Finish();
            }
            else
            {
                //Todo: write in format
                //Debug
                var writer = new StreamWriter(_outStream);
                for (var i = 0; i < _numbers.Count; i++)
                {
                    writer.WriteLine(_numbers[i].Reconstructed);
                }
               writer.Flush();
            }
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
