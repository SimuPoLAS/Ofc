using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
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
