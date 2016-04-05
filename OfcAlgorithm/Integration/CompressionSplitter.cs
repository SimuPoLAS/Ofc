using JetBrains.Annotations;

namespace OfcAlgorithm.Integration
{
    using System;
    using OfcCore;
    using OfcCore.Configurations;

    internal class CompressionSplitter : IReporter<OfcNumber>
    {
        public IConfiguaration Configuaration { get; } = new SimpleConfiguration();

        private readonly IReporter<OfcNumber>[] _blockyCompressions;
        private int _numberIndex;
        private readonly int _count;

        public CompressionSplitter([NotNull]IReporter<OfcNumber>[] blockyCompressions)
        {
            _blockyCompressions = blockyCompressions;
            _count = blockyCompressions.Length;
        }

        public int Layers { get; }
        public bool SupportsLayer { get; }

        public void Finish()
        {
            foreach (var compression in _blockyCompressions)
            {
                compression.Finish();
            }
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void PushLayer(int capacity)
        {
            throw new NotImplementedException();
        }

        public void PopLayer()
        {
            throw new NotImplementedException();
        }

        public void Report(OfcNumber number)
        {
            _blockyCompressions[_numberIndex].Report(number);
            _numberIndex = (_numberIndex + 1) % _count;
        }

        public void Report(OfcNumber[] numbers, int offset, int count) //Todo: make performant
        {
            for (var i = offset; i < offset + count; i++)
            {
                Report(numbers[i]);
            }
        }

        public void Dispose()
        {
            foreach (var t in _blockyCompressions)
            {
                t?.Dispose();
            }
        }
    }
}