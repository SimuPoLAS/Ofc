namespace Ofc.IO.Handlers
{
    using System;
    using JetBrains.Annotations;

    internal class VectorListHandler : ListHandler
    {
        protected override bool SupportsStacking { get; }


        public VectorListHandler([NotNull] IDataWriter writer) : base(writer)
        {
        }


        public override void HandleEntry(string value)
        {
            throw new NotImplementedException();
        }

        public override void HandleEntries(string[] values, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}