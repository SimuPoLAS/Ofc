using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public abstract class HuffmanNode<T> : IHuffmanNode<T> where T : IEquatable<T>
    {
        public abstract bool IsLeafNode { get; }

        public IHuffmanNode<T> LeftNode
        {
            get { return _leftNode; }
            set
            {
                if (value == null)
                    _leftNode.Parent = null;
                else
                    value.Parent = this;
                _leftNode = value;
                if (_rightNode == _leftNode)
                    _rightNode = null;
            }
        }

        public IHuffmanNode<T> RightNode
        {
            get { return _rightNode; }
            set
            {
                if (value == null)
                    _rightNode.Parent = null;
                else
                    value.Parent = this;
                _rightNode = value;
                if (_leftNode == _rightNode)
                    _leftNode = null;
            }
        }

        public IHuffmanNode<T> Parent { get; set; }
        public int Occurance { get; }

        private IHuffmanNode<T> _leftNode;
        private IHuffmanNode<T> _rightNode;

        protected HuffmanNode(int occurance)
        {
            Occurance = occurance;
        }

        public int GetSubLevel()
        {
            var level = 0;
            var node = (IHuffmanNode<T>)this;
            while (node.Parent != null)
            {
                level++;
                node = node.Parent;
            }
            return level;
        }

        public bool[] GetBooleanEncoding()
        {
            var result = new bool[GetSubLevel()];
            var node = (IHuffmanNode<T>)this;
            for (var i = 0; i < result.Length; i++)
            {
                result[result.Length - (i + 1)] = node.Parent.RightNode == node;
                node = node.Parent;
            }
            return result;
        }


        public abstract HuffmanLeafNode<T> GetLeafNode(T value);

        public IEnumerable<IHuffmanNode<T>> Enumerate()
        {
            yield return this;
            var leftNodes = _leftNode?.Enumerate();
            if (leftNodes != null)
                foreach (var huffmanTreeNode in leftNodes)
                {
                    yield return huffmanTreeNode;
                }
            var rightNodes = _rightNode?.Enumerate();
            if (rightNodes == null) yield break;
            foreach (var huffmanTreeNode in rightNodes)
            {
                yield return huffmanTreeNode;
            }
        }

        public int CompareTo(IHuffmanNode<T> other)
        {
            return Occurance.CompareTo(other.Occurance);
        }
    }
}
