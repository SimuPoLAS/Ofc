using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfcAlgorithm.Integration;

namespace OfcAlgorithm.Blocky.Method
{
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
