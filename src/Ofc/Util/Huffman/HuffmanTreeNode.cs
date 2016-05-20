using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class HuffmanTreeNode<T> : HuffmanNode<T> where T : IEquatable<T>
    {
        public override bool IsLeafNode => false;

        public HuffmanTreeNode(int occurance) : base(occurance)
        {
        }

        public HuffmanTreeNode(int occurance, IHuffmanNode<T> leftNode, IHuffmanNode<T> rightNode) : this(occurance)
        {
            LeftNode = leftNode;
            RightNode = rightNode;
        }

        public override HuffmanLeafNode<T> GetLeafNode(T value)
        {
            return LeftNode?.GetLeafNode(value) ?? RightNode?.GetLeafNode(value);
        }
    }
}
