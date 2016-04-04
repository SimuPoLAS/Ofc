namespace Ofc.IO
{
    using OfcCore;

    internal interface IHandler<in T>
    {
        IConfiguaration Configuaration { get; }


        void End();

        void HandleEntry(T value);

        void HandleEntries(T[] values, int offset, int count);

        void Start();
    }
}