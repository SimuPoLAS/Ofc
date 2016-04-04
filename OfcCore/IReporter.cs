namespace OfcCore
{
    using System;

    public interface IReporter<in T> : IDisposable
    {
        void Finish();

        void Flush();

        void Report(T[] values, int offset, int amount);
    }
}