namespace OfcAlgorithm.Integration.Dummy
{
    using System;
    using System.Globalization;
    using System.IO;
    using OfcCore;
    using OfcCore.Configurations;

    public class DummyReporter : IReporter<OfcNumber>
    {
        public IConfiguaration Configuaration { get; } = new SimpleConfiguration();

        public readonly StreamWriter FileStream;
        public int Layers => 0;
        public bool SupportsLayer => false;

        public DummyReporter(string targetDecompressed)
        {
            FileStream = new StreamWriter(new FileStream(targetDecompressed, FileMode.OpenOrCreate, FileAccess.Write));
        }

        public void Finish()
        {
            FileStream.Dispose();
        }

        public void Flush()
        {
            FileStream.Flush();
        }

        public void PushLayer(int capacity)
        {
            throw new NotSupportedException();
        }

        public void PopLayer()
        {
            throw new NotSupportedException();
        }

        public void Report(OfcNumber number)
        {
            FileStream.WriteLine(number.Reconstructed.ToString(CultureInfo.InvariantCulture) + ",");
        }

        public void Report(OfcNumber[] numbers, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(numbers[i]);
            }
        }

        public void ReportAll(OfcNumber[] values)
        {
            Report(values, 0, values.Length);
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}