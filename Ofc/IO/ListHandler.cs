namespace Ofc.IO
{
    using System;
    using OfcCore;
    using OfcCore.Configurations;

    internal abstract class ListHandler : IHandler<string>
    {
        public IConfiguaration Configuaration { get; } = new SimpleConfiguration();

        internal bool Open { get; private set; }

        protected abstract bool SupportsStacking { get; }


        protected IDataWriter Writer;


        protected ListHandler(IDataWriter writer)
        {
            Open = false;
            Writer = writer;
        }


        public virtual void End()
        {
            if (!Open) throw new InvalidOperationException("The list handler is not opened yet.");
            if (!SupportsStacking) Open = false;
        }

        public abstract void HandleEntry(string value);

        public abstract void HandleEntries(string[] values, int offset, int count);

        public virtual void Start()
        {
            if (Open && !SupportsStacking) throw new NotSupportedException("Stacking is not supported.");
        }
    }
}