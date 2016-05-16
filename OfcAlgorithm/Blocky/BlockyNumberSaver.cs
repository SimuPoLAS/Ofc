namespace OfcAlgorithm.Blocky
{
    using OfcAlgorithm.Integration;

    public class BlockyNumberSaver : IOfcNumberWriter
    {
        private int _index;
        public OfcNumber[] Values;

        public void Initialize(int numCount)
        {
            Values = new OfcNumber[numCount];
        }

        public void Write(OfcNumber value)
        {
            Values[_index++] = value;
        }
    }
}
