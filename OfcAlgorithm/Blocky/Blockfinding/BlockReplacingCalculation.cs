using OfcAlgorithm.Integration;

namespace OfcAlgorithm.Blocky.Blockfinding
{
    struct BlockReplacingCalculation
    {
        public int SavedBits;
        public Block VirtualBlock;
        public Block OldConcurrentBlock;
        public Block.SavingGrade OldConcurrentSavingGrade;
        public bool IsValid;

        public void Initialize(int savedBits, Block virtualBlock, Block oldConcurrentBlock, Block.SavingGrade oldConcurrentBlockSavingGrade)
        {
            SavedBits = savedBits;
            VirtualBlock = virtualBlock;
            OldConcurrentBlock = oldConcurrentBlock;
            OldConcurrentSavingGrade = oldConcurrentBlockSavingGrade;
            IsValid = true;
        }

        public bool ProcessValue(OfcNumber value, int index)
        {
            return VirtualBlock.Method.ProcessValue(ref VirtualBlock, value, index, ref SavedBits);
        }
    }
}
