namespace Ofc.IO.Handlers
{
    using JetBrains.Annotations;

    internal class ScalarListHandler : ListHandler
    {
        protected override bool SupportsStacking { get; }


        public ScalarListHandler([NotNull] IDataWriter writer) : base(writer)
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