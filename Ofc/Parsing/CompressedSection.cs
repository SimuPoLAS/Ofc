namespace Ofc.Parsing
{
    internal struct CompressedSection
    {
        internal uint Start;
        internal uint End;
        internal byte Size;


        public CompressedSection(uint start, uint end, byte size)
        {
            Start = start;
            End = end;
            Size = size;
        }
    }
}