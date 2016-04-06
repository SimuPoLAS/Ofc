using OfcAlgorithm.Integration;

namespace OfcAlgorithm.Blocky.Blockfinding
{
    /// <summary>
    /// Represents a potentual block that would alter the current block if it was created
    /// </summary>
    struct BlockReplacingCalculation
    {
        /// <summary>
        /// The difference in bits if the block would be created now. > 0 would mean that we would save bits with this
        /// </summary>
        public int SavedBits;

        /// <summary>
        /// THe potentual block
        /// </summary>
        public Block VirtualBlock;
        
        /// <summary>
        /// The state of the current block when this potentual block was created
        /// </summary>
        public Block OldConcurrentBlock;
        public Block.SavingGrade OldConcurrentSavingGrade;

        /// <summary>
        /// If this is still running
        /// </summary>
        public bool IsValid;

        public void Initialize(int savedBits, Block virtualBlock, Block oldConcurrentBlock, Block.SavingGrade oldConcurrentBlockSavingGrade)
        {
            SavedBits = savedBits;
            VirtualBlock = virtualBlock;
            OldConcurrentBlock = oldConcurrentBlock;
            OldConcurrentSavingGrade = oldConcurrentBlockSavingGrade;
            IsValid = true;
        }

        /// <summary>
        /// Adds the given value to the potentual block, or sets IsValid to false if it can't be added
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ProcessValue(OfcNumber value, int index)
        {
            return VirtualBlock.Method.ProcessValue(ref VirtualBlock, value, index, ref SavedBits);
        }
    }
}
