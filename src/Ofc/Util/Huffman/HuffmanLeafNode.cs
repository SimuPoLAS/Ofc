namespace Ofc.Util.Huffman
{
    using System;

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
