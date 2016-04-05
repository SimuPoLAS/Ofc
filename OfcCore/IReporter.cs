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

        /// <summary>
        /// Reports all values at once, you may not call Report() after this!
        /// </summary>
        /// <param name="values"></param>
        void ReportAll(T[] values);
    }
}