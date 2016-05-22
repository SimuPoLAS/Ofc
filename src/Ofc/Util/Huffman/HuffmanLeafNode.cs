using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class HuffmanLeafNode<T> : HuffmanNode<T> where T : IEquatable<T>
    {
        public readonly T Value;
        public override bool IsLeafNode => false;

        public HuffmanLeafNode(int occurance, T value) : base(occurance)
        {
            Value = value;
        }

        public override HuffmanLeafNode<T> GetLeafNode(T value)
        {
            return Value.Equals(value) ? this : null;
        }
    }
}
