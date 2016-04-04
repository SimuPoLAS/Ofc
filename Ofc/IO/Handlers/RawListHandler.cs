namespace Ofc.IO.Handlers
{
    using System;
    using JetBrains.Annotations;

    internal class RawListHandler : ListHandler
    {
        protected override bool SupportsStacking => false;


        public RawListHandler([NotNull] IDataWriter writer) : base(writer)
        {
        }


        public override void HandleEntry(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Writer.WriteDouble(double.Parse(value));
        }

        public override void HandleEntries(string[] values, int offset, int count)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (offset < 0 || offset >= values.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + count >= values.Length) throw new ArgumentOutOfRangeException(nameof(count));
            for (var i = offset; i < offset + count; i++)
                Writer.WriteDouble(double.Parse(values[i]));
        }
    }
}