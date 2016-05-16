namespace Ofc.Algorithm.Blocky.Blockfinding
{
    using Ofc.Algorithm.Integration;

    /// <summary>
    /// Represents a potentual block (A block that doesn't exist right now, but could be created in the future)
    /// </summary>
    struct BlockCalculation
    {
        /// <summary>
        /// The difference in bits if the block would be created now. > 0 would mean that we would save bits with this
        /// </summary>
        public int SavedBits;
        /// <summary>
        /// THe potentual block
        /// </summary>
        public Block VirtualBlock;

        public BlockCalculation(int savedBits, Block virtualBlock)
        {
            SavedBits = savedBits;
            VirtualBlock = virtualBlock;
        }

        /// <summary>
        /// Adds the given value to the potentual block, or destroys it if it can't be added
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ProcessValue(OfcNumber value, int index)
        {
            return VirtualBlock.Method.ProcessValue(ref VirtualBlock, value, index, ref SavedBits);
        }

        /// <summary>
        /// Creates a normal potentual block from a potentual block that would replace the current block
        /// </summary>
        /// <param name="calc"></param>
        /// <returns></returns>
        public static BlockCalculation FromReplacingCalculation(BlockReplacingCalculation calc)
        {
            return new BlockCalculation(0, calc.VirtualBlock);
        }
    }
}
