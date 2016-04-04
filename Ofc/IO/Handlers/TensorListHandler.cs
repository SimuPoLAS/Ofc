namespace Ofc.IO.Handlers
{
    using JetBrains.Annotations;

    internal class TensorListHandler : ListHandler
    {
        protected override bool SupportsStacking { get; }


        public TensorListHandler([NotNull] IDataWriter writer) : base(writer)
        {
        }


        public override void HandleEntry(string value)
        {
            throw new System.NotImplementedException();
        }

        public override void HandleEntries(string[] values, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}