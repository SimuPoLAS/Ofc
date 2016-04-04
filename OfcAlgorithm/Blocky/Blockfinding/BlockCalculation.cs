using OfcAlgorithm.Integration;

namespace OfcAlgorithm.Blocky.Blockfinding
{
    struct BlockCalculation
    {
        public int SavedBits;
        public Block VirtualBlock;

        public BlockCalculation(int savedBits, Block virtualBlock)
        {
            SavedBits = savedBits;
            VirtualBlock = virtualBlock;
        }

        public bool ProcessValue(OfcNumber value, int index)
        {
            return VirtualBlock.Method.ProcessValue(ref VirtualBlock, value, index, ref SavedBits);
        }

        /// <summary>
        /// Also sets the SavedBits to 0
        /// </summary>
        /// <param name="calc"></param>
        /// <returns></returns>
        public static BlockCalculation FromReplacingCalculation(BlockReplacingCalculation calc)
        {
            return new BlockCalculation(0, calc.VirtualBlock);
        }
    }
}
