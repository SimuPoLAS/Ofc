using System.Collections.Generic;
using OfcAlgorithm.Integration;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace OfcAlgorithm.Blocky.Blockfinding
{
    /// <summary>
    /// Predicts special patterns in values
    /// </summary>
    class PatternPredictor
    {
        /// <summary>
        /// The index of the current value
        /// </summary>
        public int Index;

        private readonly List<OfcNumber> _values;
        private readonly int _limit;

        /// <summary>
        /// Is able to say if there's going to be a pattern in the future. You need to call PredictNext every value, or set Index to the current index before calling.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="index">´the current value index</param>
        public PatternPredictor(List<OfcNumber> values, int index = 0)
        {
            _values = values;
            _limit = values.Count - 2;
            Index = index;
        }

        /// <summary>
        /// Is able to say if there's could be a pattern in the near future
        /// </summary>
        /// <param name="current"></param>
        /// <returns>If you should start a pattern calc now</returns>
        public bool PredictNext(OfcNumber current)
        {
            Index++;
            if (Index >= _limit) return false;
            var next = _values[Index];
            if (next.Number == current.Number && next.Exponent == current.Exponent) //same pattern
                return true;
            var ahead = _values[Index + 1];
            if (next.Number + (next.Number - current.Number) == ahead.Number && next.Exponent + (next.Exponent - current.Exponent) == ahead.Exponent) // offset pattern
                return true;
            return false;
        }
    }
}
