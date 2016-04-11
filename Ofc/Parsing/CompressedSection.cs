namespace Ofc.Parsing
{
    internal struct CompressedSection
    {
        internal uint Start;
        internal uint End;


        public CompressedSection(uint start, uint end)
        {
            Start = start;
            End = end;
        }
    }
}