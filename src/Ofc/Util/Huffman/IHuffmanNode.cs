namespace Ofc.Util.Huffman
{
    using System;
    using System.Collections.Generic;

    public interface IHuffmanNode<T> : IComparable<IHuffmanNode<T>> where T : IEquatable<T>
    {
        IHuffmanNode<T> LeftNode { get; set; }
        IHuffmanNode<T> RightNode { get; set; }
        IHuffmanNode<T> Parent { get; set; }
        int Occurance { get; }

        bool IsLeafNode { get; }

        bool[] GetBooleanEncoding();
        HuffmanLeafNode<T> GetLeafNode(T value);
        IEnumerable<IHuffmanNode<T>> Enumerate();
    }
}
