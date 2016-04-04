namespace OfcCore
{
    using System;

    public interface IReporter<in T> : IDisposable
    {
        IConfiguaration Configuaration { get; }


        void Finish();

        void Flush();

        void Report(T value);

        void Report(T[] values, int offset, int amount);
    }
}