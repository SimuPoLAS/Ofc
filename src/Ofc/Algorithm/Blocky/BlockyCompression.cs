// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InvertIf

using System.Security.Cryptography.X509Certificates;

namespace Ofc.Algorithm.Blocky
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Algorithm.Blocky.Method.PatternPingPong;
    using Ofc.Algorithm.Huffman;
    using Ofc.Algorithm.Integration;
    using Ofc.Core;
    using Ofc.Util;

    class BlockyCompression : IReporter<OfcNumber>
    {
        public IConfiguaration Configuaration { get; }

        public int Layers => 0;
        public bool SupportsLayer => false;

        private readonly StreamBitWriter _writer;
        public List<OfcNumber> Values;
        public List<Block> Blocks;
        public BlockyMetadata Metadata;
        private HuffmanCreator _huffman;
        private Blockfinding.Blockfinding _blockfinding;
        private int _totalPostCompressionOptimisationBlockValues;

        public BlockyCompression(int capacity, Stream writer, IConfiguaration config)
        {
            Configuaration = config;
            _writer = new StreamBitWriter(writer);
            Values = capacity > 0 ? new List<OfcNumber>(capacity) : new List<OfcNumber>();
            Blocks = new List<Block>();
        }

        #region SupportsLayers is set to false ... (Todo)
        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void PushLayer(int capacity)
        {
            throw new NotImplementedException();
        }

        public void PopLayer()
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Adds a number to the list of values that will be compressed. It will also analyze the number and change some global headers based on the result, so only add numbers through here!
        /// </summary>
        /// <param name="ofcNumber"></param>
        public void Report(OfcNumber ofcNumber)
        {
            Values.Add(ofcNumber);
        }

        public void Report(OfcNumber[] numbers, int offset, int count)
        {
            for (var index = offset; index < offset + count; index++)
            {
                var num = numbers[index];
                Report(num);
            }
        }

        /// <summary>
        /// Will calculate everything and make it ready for writing
        /// </summary>
        public void Finish()
        {
                if (Values.Count == 0) return;

                Metadata = BlockyMetadata.FromData(Values);
                Metadata.MaxNeededBitsNeededBitsNumber = Utility.GetNeededBits(Metadata.MaxNeededBitsNumber);

                _blockfinding = new Blockfinding.Blockfinding(Values, Metadata);
                Blocks = _blockfinding.FindAllBlocks();

                PostCompressionOptimisation(); //Todo: make optional

                Write();
        }

        private void PostCompressionOptimisation()
        {
            // creating huffman data now so we can leave out values that are in a block ;)
            // results in better speed + more accuracy
            #region calculating huffman data 
            var currentBlockIndex = 0;
            var valueCount = Values.Count;
            var blockCount = Blocks.Count;
            var nextStop = blockCount == 0 ? valueCount : Blocks[currentBlockIndex].Index;
            var currentBlockLength = blockCount == 0 ? 0 : Blocks[currentBlockIndex].Length;
            var totalBlockValues = currentBlockLength;

            //if (!Metadata.NoExponent)
            //{
            //    _huffman = new HuffmanCreator((ushort)Math.Pow(2, 10));

            //    var huffIndex = 0;
            //    while (huffIndex < valueCount)
            //    {
            //        for (; huffIndex < nextStop; huffIndex++)
            //        {
            //            _huffman.AddOccurence(Values[huffIndex].Exponent);
            //        }

            //        if (++currentBlockIndex >= Blocks.Count)
            //            nextStop = valueCount;
            //        else
            //        {
            //            huffIndex += currentBlockLength;
            //            nextStop = Blocks[currentBlockIndex].Index;
            //            currentBlockLength = Blocks[currentBlockIndex].Length;
            //            totalBlockValues += currentBlockLength;
            //        }
            //    }
            //}
            _totalPostCompressionOptimisationBlockValues = totalBlockValues;
            #endregion

            #region putting multiple "pattern same" blocks to one pingpong block

            if (Blocks.Count == 0)
                return;
            var ppp = (PatternPingPongCompression)_blockfinding.GetInitializedMethod(Blockfinding.Blockfinding.Methods.PatternPingPong);
            var recentBlock = Blocks[0];
            byte lastppLength = 0;
            for (var i = 1; i < Blocks.Count; i++)
            {
                var currentBlock = Blocks[i];

                if (currentBlock.HasPattern && currentBlock.Pattern == Block.PatternType.Same && recentBlock.HasPattern)
                {
                    if (recentBlock.Pattern == Block.PatternType.Pingpong && lastppLength * recentBlock.Length + recentBlock.Index == currentBlock.Index)
                    {
                        var initValue = Values[currentBlock.Index];
                        var recentBlockRealLength = lastppLength * recentBlock.Length;
                        if (currentBlock.Length != lastppLength || initValue.Number == Values[recentBlock.Index + recentBlockRealLength - 1].Number || initValue.Number != Values[recentBlock.Index + recentBlockRealLength - 1 - lastppLength].Number) continue;
                        recentBlock.Length++;
                        Blocks[i - 1] = recentBlock;
                        Blocks.RemoveAt(i--);
                    }
                    else if (recentBlock.Pattern == Block.PatternType.Same && recentBlock.Index + recentBlock.Length == currentBlock.Index && currentBlock.Length == recentBlock.Length)
                    {
                        recentBlock.Pattern = Block.PatternType.Pingpong;
                        recentBlock.Method = ppp;
                        ppp.PingPongPatternLengths.Add(new PatternPingPongCompression.PatternPingPongMetadata(recentBlock.Length, recentBlock.Index));
                        lastppLength = recentBlock.Length;
                        recentBlock.Length = 2;
                        Blocks[i - 1] = recentBlock;
                        Blocks.RemoveAt(i--);
                    }
                    else
                        recentBlock = currentBlock;
                }
            }
            #endregion
        }

        /// <summary>
        /// Will write everything to the given Stream/Writer. Call "Compress" first!
        /// </summary>
        public void Write() //Bug: Actually, finding all blocks and then writing all blocks is pretty stupid. By doing this we first have a CPU / RAM bottleneck, and here we get a Harddrive bottleneck. Doesn't matter if master weed does good parallel stuff though
        {
            var currentBlockIndex = 0;

            var valueCount = Values.Count;
            var blockCount = Blocks.Count;
            var nextStop = blockCount == 0 ? valueCount : Blocks[currentBlockIndex].Index;

            var hasExponent = !Metadata.NoExponent;
            var huffmanWriter = _huffman?.CreateWriter(Metadata.MaxNeededBitsExponent, _writer);
            var shouldUseHuffman = hasExponent && huffmanWriter != null && huffmanWriter.IsBetterThanDefault(valueCount - _totalPostCompressionOptimisationBlockValues, _huffman);

            // Writing global header
            Metadata.Write(_writer);

            shouldUseHuffman = false; //debug

            _writer.WriteByte(shouldUseHuffman ? (byte)1 : (byte)0, 1);

            if (shouldUseHuffman)
                huffmanWriter.WriteDictionary();

            for (var i = 0; i < valueCount;)
            {
                for (; nextStop > i; i++) // writing values until the next block is here
                {
                    var value = Values[i];

                    //Todo: is the loop below really nessecary with the new changes?
                    while (value.NeededBitsNumber > Metadata.MaxNeededBitsNumber) //Todo: check how often this case appears. (This is a "bug" created by the blockfinding, which "corrects" the value to fit with the exp of a block that might get created ...
                    {
                        value.Number /= 10;
                        value.Exponent++;
                        value.NeededBitsNumber = value.Number.GetNeededBits();
                    }

                    _writer.WriteByte(0, 1);
                   
                    if (!Metadata.IsAbsolute)
                        _writer.WriteByte((byte)(value.IsNegative ? 1 : 0), 1);
                    _writer.Write((ulong)value.Number, Metadata.MaxNeededBitsNumber);
                    if (hasExponent)
                    {
                        _writer.WriteByte((byte)(value.Exponent < 0 ? 1 : 0), 1);
                        if (shouldUseHuffman)
                            huffmanWriter.Write((ushort)Math.Abs(value.Exponent));
                        else
                            _writer.Write((ulong)Math.Abs(value.Exponent), Metadata.MaxNeededBitsExponent);
                    }

                }

                if (++currentBlockIndex < blockCount)
                {
                //    Console.WriteLine(Blocks[currentBlockIndex - 1]);
                    Blocks[currentBlockIndex - 1].Method.Write(_writer, Blocks[currentBlockIndex - 1], ref i);
                    nextStop = Blocks[currentBlockIndex].Index;
                }
                else
                {
                    nextStop = valueCount;
                }
            }

            _writer.Flush(); // This is nessecary. Will write the last buffered byte may only be partially complete!
        }


        //private void ViewDebug(int index, Calculation[] calcs, ReplacingCalculation[] reps, Block lastBlock)
        //{
        //    var enablePress = false;
        //    if (index == 0)
        //    {
        //        enablePress = true;
        //        index = Values.Count - 1;
        //    }
        //    for (var i = Math.Min(0, Math.Max(0, index - 50)); i <= index; i++)
        //    {
        //        if (Blocks.Any(block => block.Index == i && block.Length != 0))
        //            Console.WriteLine("\t[" + Blocks.First(block => block.Index <= i && block.Index + block.Length > i) + "]");

        //        if (calcs.Any(block => block.VirtualBlock.Index == i && block.VirtualBlock.Length != 0))
        //        {
        //            Console.ForegroundColor = ConsoleColor.DarkYellow;
        //            var cc = calcs.First(block => block.VirtualBlock.Index == i && block.VirtualBlock.Index + block.VirtualBlock.Length > i);
        //            Console.WriteLine("\t[" + cc.VirtualBlock + "] " + cc.SavedBits);
        //            Console.ForegroundColor = ConsoleColor.Gray;

        //        }

        //        if (reps.Any(block => block.VirtualBlock.Index == i && block.VirtualBlock.Length != 0))
        //        {
        //            Console.ForegroundColor = ConsoleColor.DarkBlue;
        //            var cc = reps.First(block => block.VirtualBlock.Index <= i && block.VirtualBlock.Index + block.VirtualBlock.Length > i);
        //            Console.WriteLine("\t[" + cc.VirtualBlock + "] " + cc.SavedBits);
        //            Console.ForegroundColor = ConsoleColor.Gray;

        //        }

        //        if (Blocks.Any(block => block.Index <= i && block.Index + block.Length > i))
        //            Console.Write("\t");

        //        if (i == index)
        //            Console.ForegroundColor = ConsoleColor.DarkMagenta;

        //        Console.WriteLine(Values[i].Number + " e" + Values[i].Exponent + "       /" + i + (_copy[i].Number != Values[i].Number || Values[i].Exponent != _copy[i].Exponent ? _copy[i].Number + " e" + _copy[i].Exponent : "!"));

        //        if (i == index)
        //            Console.ForegroundColor = ConsoleColor.Gray;

        //        if (enablePress && i % 20 == 0 && i > 0)
        //            if (Console.ReadKey().KeyChar == 'c')
        //            {
        //                Console.Clear();
        //                return;
        //            }
        //    }
        //    Console.WriteLine("Appending block: [" + lastBlock + "]");

        //    Console.ReadKey();
        //}

        //private bool ProcessValue(ref Block block, ref OfcNumber value, int index, ref int bitDiff) // positive bitDiff = good
        //{
        //    if (block.Length == byte.MaxValue && (!block.HasPattern || block.Pattern != Block.PatternType.Pingpong))
        //    {

        //        return false;
        //    }

        //    if (!block.HasExponent)
        //    {
        //        if (value.Exponent != 0)
        //            return false; //Todo: Is the block really impossible in this case?

        //        if (block.HasPattern && block.Length > 0)
        //        {
        //            var patternDiff = (value.Number - Values[index - 1].Number);
        //            if (block.Pattern == Block.PatternType.Same && patternDiff != 0)
        //            {
        //                if (block.Length == 1 && patternDiff == 1)
        //                    block.Pattern = Block.PatternType.Increasing;
        //                else
        //                {
        //                    block.Pattern = Block.PatternType.Pingpong;
        //                    block.PatternProperties = new Block.PingPongPatternProperties(Values[index - 1].Number, block.Length, value.Number, 0);
        //                }
        //            }
        //            else if (block.Pattern == Block.PatternType.Increasing && block.Length == 1 && patternDiff != 1) // Todo: could we just delete this case cause it will hardly ever happen?
        //            {
        //                block.Pattern = Block.PatternType.Pingpong;
        //                block.PatternProperties = new Block.PingPongPatternProperties(value.Number, 1, Values[index - 1], 1);
        //            }
        //            else if (block.Pattern == Block.PatternType.Pingpong)
        //            {
        //                if (patternDiff != 0 && value.Number == block.PatternProperties.PatternNum1)
        //                {
        //                    if (block.PatternProperties.RepeatCount == byte.MaxValue)
        //                        return false;
        //                    block.PatternProperties.RepeatCount++;
        //                }

        //                if (((patternDiff != 0 && (block.Length % block.PatternProperties.Length != 0 || value.Number != block.PatternProperties.PatternNum1 && value.Number != block.PatternProperties.PatternNum2))
        //                || patternDiff == 0 && block.Length % block.PatternProperties.Length == 0)
        //                || (block.Pattern == Block.PatternType.Increasing && patternDiff != 1))
        //                {
        //                    if (index - block.Index >= byte.MaxValue) return false;
        //                    var nb = block.NeededBits;
        //                    block.NeededBits = 0; // the nb per value if we still had the pattern
        //                    bitDiff += block.DifferenceWithNb(this, ref nb) - 2; // -2 because now we have no "Pattern" option
        //                    block.HasPattern = false;
        //                }

        //            }
        //        }
        //    }
        //    else if (block.Exponent < value.Exponent) // The exp is bigger, which means we will set the exp equal and add stuff to the value to balance things
        //    {
        //        var expDiff = value.Exponent - block.Exponent;

        //        if (expDiff > 18) return false; // long can't hold 10^19 or higher
        //        var multiplier = (long)Math.Pow(10, expDiff);
        //        var newNum = value.Number;
        //        if (value.Number != 0)
        //        {
        //            if (long.MaxValue / value.Number < multiplier) return false; // overflow check

        //            newNum = value.Number * multiplier; // This balances the expDiff we subtraced from Exponent
        //        }

        //        var newNb = newNum.GetNeededBits(); // Recalculate the needed bits for the number, because that may have changed
        //        if (value.NeededBitsNumber > MaxNeededBitsNumber) return false; //Todo: you can't write with more than max bits (even in blocks when the nb are overridden), because of "nbnbmaxnumber" ... maybe we can work our way around that? Currently we just throw the block away ...

        //        // We don't actually apply the changes before checking if we can even write the new value ...
        //        value.Exponent -= (short)expDiff;
        //        value.NeededBitsNumber = newNb;
        //        value.Number = newNum;

        //        if (value.NeededBitsNumber > block.NeededBits) // Value cannot be added to block, we have to check what would change if we change the NeededBits in the block header!
        //        {
        //            var nbNewNumber = value.NeededBitsNumber;
        //            bitDiff += block.DifferenceWithNb(this, ref nbNewNumber); // Adds the difference in bits if we would change the block header. May change nbNewNumber to the global-header-max! (can be worth, because no header then ...)
        //            block.NeededBits = nbNewNumber; // Need to set this after the call for future calculations to be exact
        //        }
        //        Values[index] = value;
        //    }
        //    else if (block.Exponent > value.Exponent) // The exp is smaller, which means we will recalculate the whole block to fit this value
        //    {
        //        var expDiff = (block.Exponent - value.Exponent);

        //        if (expDiff > 18) return false; // long can't hold 10^19 or higher
        //        var multiplier = (long)Math.Pow(10, expDiff);


        //        if (block.BiggestNumber > 0 && long.MaxValue / block.BiggestNumber < multiplier) return false; // overflow check
        //        var newNum = block.BiggestNumber * multiplier; // Recalculate the biggest number of the blog

        //        block.BiggestNumber = Math.Max(value.Number, newNum);// value.Number > newNum ? value.Number : newNum; // Recalculate the biggest number of the blog
        //        block.Exponent = value.Exponent; // Setting the new exponent for the whole block

        //        var bigNumNb = Math.Max(block.BiggestNumber.GetNeededBits(), value.NeededBitsNumber); // Math.Max because the new value could be bigger than the oldest biggest value, which would give a wrong result
        //        if (bigNumNb > MaxNeededBitsNumber) return false; //Todo: you can't write with more than max bits (even in blocks when the nb are overridden), because of "nbnbmaxnumber" ... maybe we can work our way around that? Currently we just throw the block away ...


        //        if (bigNumNb > block.NeededBits) // The change in exp made the numbers bigger -> we need more Nb to store the numbers
        //        {
        //            var diff = block.DifferenceWithNb(this, ref bigNumNb); // Adds the difference in bits that comes with changing the block header. May change bigNumNb to the global-header-max! (can be worth, because no header then ...)
        //            bitDiff += diff;
        //            block.NeededBits = bigNumNb; // Need to set this after the call for future calculations to be exact
        //        }
        //    }

        //    if (block.AbsoluteSign && block.IsSignNegative != value.IsNegative) // Check if the new value works with the current "absolute sign" block header
        //    {
        //        block.AbsoluteSign = false;
        //        bitDiff -= block.Length - 1; // We loose 1 bit per value, because we need to write down the sign now ... but we save 1 because less block header stuffs 
        //    }

        //    block.Length++;

        //    if (value.Number > block.BiggestNumber) // If the biggest value in the block is smaller than the new one, we need to set it for future calculations to be correct
        //    {
        //        block.BiggestNumber = value.Number;
        //        if (block.NeededBits < value.NeededBitsNumber) // If the new number needs more bits than specified in the block header, we need to adjust that
        //        {
        //            var nbNewBiggest = value.NeededBitsNumber;
        //            if (!block.HasPattern)
        //                bitDiff += block.DifferenceWithNb(this, ref nbNewBiggest); // Adds the difference in bits that comes with changing the block header. May change bigNumNb to the global-header-max! (can be worth, because no header then ...)
        //            block.NeededBits = nbNewBiggest; // Need to set this after the call for future calculations to be exact
        //        }
        //    }


        //    bitDiff++;
        //    if (block.AbsoluteSign && !IsAbsolute)
        //        bitDiff++;
        //    if (!NoExponent)
        //        bitDiff += MaxNeededBitsExponent;
        //    if (block.HasPattern)
        //        bitDiff += MaxNeededBitsNumber;
        //    else if (block.OverrideGlobalNb)
        //        bitDiff += MaxNeededBitsNumber - block.NeededBits;



        //    //bitDiff += 1 + MaxNeededBitsExponent + value.NeededBitsNumber - (block.OverrideGlobalNb ? block.NeededBits : MaxNeededBitsNumber) + (block.AbsoluteSign && !IsAbsolute ? 1 : 0);
        //    return true;
        //}

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
