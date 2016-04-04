using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OfcAlgorithm.Blocky.Method;
using OfcAlgorithm.Blocky.Method.FloatSimmilar;
using OfcAlgorithm.Blocky.Method.NumbersNoExp;
using OfcAlgorithm.Blocky.Method.PatternOffset;
using OfcAlgorithm.Blocky.Method.PatternPingPong;
using OfcAlgorithm.Blocky.Method.PatternSame;
using OfcAlgorithm.Integration;

// ReSharper disable InvertIf
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace OfcAlgorithm.Blocky.Blockfinding
{
    class Blockfinding
    {
        public readonly List<OfcNumber> Values;
        public readonly int ValueCount;
        public readonly BlockyMetadata Metadata;
        public readonly HeaderSizes Headers;
        public bool IsDone => _index >= ValueCount;

        private int _index;
        private readonly List<Block> _blocks = new List<Block>();
        private BlockCalculation _appendingCalculation;
        private int _appendingCalculationSavingGrade;
        private Block _lastStableBlock;
        private bool _isAppendingCalculationValid;
        private readonly List<BlockCalculation> _calculations = new List<BlockCalculation>();
        private readonly BlockReplacingCalculation[] _replacingCalculations = new BlockReplacingCalculation[Enum.GetValues(typeof(Block.SavingGrade)).Length];
        private readonly PatternPredictor _patternPredictor;
        private bool _hasRunningPatternCalculation;
        private readonly CompressionMethod[] _initializedCompressionMethods = new CompressionMethod[(int)Methods.Count];

#if DEBUG
        private bool _debugEnabled = true;
        private int _debugJump;
        private static int _debugSkip;
        private static int _debugCounter;
        private bool _debugShowStable;
#endif



        public Blockfinding(List<OfcNumber> values, BlockyMetadata metadata)
        {
            Values = values;
            ValueCount = Values.Count;
            Metadata = metadata;

            Headers = new HeaderSizes(Metadata);
            _patternPredictor = new PatternPredictor(Values);
#if DEBUG
            if (_debugSkip > 0)
            {
                _debugSkip--;
                _debugEnabled = false;
            }
            _debugCounter++;
#endif

            _initializedCompressionMethods[(int)Methods.PatternSame] = new PatternSameCompression(this);
            _initializedCompressionMethods[(int)Methods.PatternPingPong] = new PatternPingPongCompression(this);
            _initializedCompressionMethods[(int)Methods.FloatSimmilar] = new FloatSimmilarCompression(this);
            _initializedCompressionMethods[(int)Methods.NumbersNoExp] = new NumbersNoExpCompression(this);
            _initializedCompressionMethods[(int)Methods.PatternOffset] = new PatternOffsetCompression(this);
        }



#if DEBUG
        public static void SetDebugEnabled(bool enabled)
        {
            _debugSkip = enabled ? 0 : int.MaxValue;
        }
#endif



        public List<Block> FindAllBlocks()
        {
            while (ProcessNextValue())
            {
#if DEBUG
                if (_index >= _debugJump)
                    ViewStateDebug();
#endif
            }
            return _blocks;
        }

#if DEBUG
        public void ViewStateDebug(int viewCount = 35)
        {
            if (!_debugEnabled) return;

#if DNX451
            Console.Clear();
#endif
            // BUG: needs a clear
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Blocks that are really there");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Blocks that may be here in the future (calculations)");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Blocks that may be here in the future (calculations) that alter an existing block");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Last block (still appending)");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Last block (dead)");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            for (var i = Math.Max(_index - viewCount, 0); i < _index; i++)
            {
                PrintBlockAtIndexDebug(i, ConsoleColor.DarkYellow, _blocks.Where(block => block.Index != _appendingCalculation.VirtualBlock.Index));
                PrintCalculationAtIndexDebug(i, ConsoleColor.Blue, _calculations);
                PrintBlockAtIndexDebug(i, ConsoleColor.Magenta, _replacingCalculations.Where(calc => calc.IsValid).Select(calc => calc.VirtualBlock));

                var displayedAppendingBlock = _debugShowStable ? _lastStableBlock : _appendingCalculation.VirtualBlock;

                if (displayedAppendingBlock.IsValid && displayedAppendingBlock.Index == i)
                {
                    if (_isAppendingCalculationValid)
                        PrintBlockDebug(displayedAppendingBlock, ConsoleColor.Green);
                    else
                        PrintBlockDebug(displayedAppendingBlock, ConsoleColor.DarkGreen);
                }

                if (IsValueInBlock(i, _blocks))
                {
                    Console.Write("\t");
                }
                Console.WriteLine(Values[i].Reconstructed);
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_index < ValueCount)
                Console.WriteLine("next: " + Values[_index].Reconstructed + " (" + _index + ".)");

            Console.Write(_debugCounter + "# ");
            Console.ForegroundColor = ConsoleColor.Gray;

            var command = Console.ReadLine().ToLower();
            switch (command)
            {
                case "break":
                    Debugger.Break();
                    break;
                case "mode":
                    _debugShowStable = !_debugShowStable;
                    Console.WriteLine("Showing Stable block: " + _debugShowStable + ", press any key!");
                    Console.ReadKey(true);
                    break;
                case "skip":
                    _debugEnabled = false;
                    break;
                case "skip all":
                    _debugEnabled = false;
                    _debugSkip = int.MaxValue;
                    break;
                default:
                    int value;
                    if (int.TryParse(command, out value))
                    {
                        _debugJump = value;
                    }
                    else if (command?.Contains(' ') ?? false)
                    {
                        var split = command.Split(' ');
                        if (split[0] == "skip" && int.TryParse(split[1], out value))
                        {
                            _debugSkip = --value;
                            _debugEnabled = false;
                        }
                    }
                    break;
            }
        }

        private void PrintBlockAtIndexDebug(int index, ConsoleColor color, IEnumerable<Block> potentialBlocks)
        {
            var block = GetBlockAtIndex(index, potentialBlocks);
            PrintBlockDebug(block, color);
        }

        private void PrintCalculationAtIndexDebug(int index, ConsoleColor color, IEnumerable<BlockCalculation> calculation)
        {
            var calc = GetCalcAtIndex(index, calculation);
            if (calc.VirtualBlock.Index != index) return;
            Console.ForegroundColor = color;
            Console.Write("\t" + calc.VirtualBlock + " / " + calc.SavedBits + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintBlockDebug(Block block, ConsoleColor color)
        {
            if (block.IsValid)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("\t" + block);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /// <summary>
        /// Only use this for debug purposes!
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        private Block GetBlockForValue(int index, IEnumerable<Block> blocks)
        {
            return blocks.FirstOrDefault(block => block.Index <= index && block.Index + block.Length > index);
        }

        /// <summary>
        /// Only use this for debug purposes!
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        private bool IsValueInBlock(int index, IEnumerable<Block> blocks)
        {
            return blocks.Any(block => block.Index <= index && block.Index + block.Length > index);
        }

        private BlockCalculation GetCalcAtIndex(int index, IEnumerable<BlockCalculation> blocks)
        {
            return blocks.FirstOrDefault(block => block.VirtualBlock.Index == index);
        }

        /// <summary>
        /// Only use this for debug purposes!
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        private Block GetBlockAtIndex(int index, IEnumerable<Block> blocks)
        {
            return blocks.FirstOrDefault(block => block.Index == index);
        }
#endif


        public enum Methods
        {
            PatternSame,
            FloatSimmilar,
            NumbersNoExp,
            PatternOffset,
            PatternPingPong,
            Count
        }

        public CompressionMethod GetInitializedMethod(Methods method)
        {
            return _initializedCompressionMethods[(int)method];
        }

        private void AddNewBlock(BlockCalculation calc)
        {
            _lastStableBlock = calc.VirtualBlock;
            _appendingCalculation = calc;
            _blocks.Add(calc.VirtualBlock);
            _isAppendingCalculationValid = true;
            _appendingCalculationSavingGrade = (int)calc.VirtualBlock.GetSavingGrade();

            for (var i = 1; i < _replacingCalculations.Length; i++)
            {
                _replacingCalculations[i].IsValid = false;
            }
        }

        private void ReplaceNewestBlock(BlockCalculation with)
        {
            _blocks[_blocks.Count - 1] = with.VirtualBlock;
            _lastStableBlock = with.VirtualBlock;
            _appendingCalculation = with;
            _appendingCalculation.SavedBits = 0;
            _appendingCalculationSavingGrade = (int)with.VirtualBlock.GetSavingGrade();
        }

        private void TransformCalcsToReplaceCalsOrDelete(Block oldConcurrent, int oldConcurrentSavedBits, int exclude = -1)
        {
            //Bug: we shouldn't mindlessly remove all calculations, because if there is a calc will less nb that would have been created soon, it might be better than this one in the long run
            for (var k = 0; k < _calculations.Count; k++)
            {
                if (exclude == k) continue;
                var savingGrade = (int)_calculations[k].VirtualBlock.GetSavingGrade();
                if (savingGrade > _appendingCalculationSavingGrade && (!_replacingCalculations[savingGrade].IsValid || _replacingCalculations[savingGrade].SavedBits < _calculations[k].SavedBits))
                {
                    _replacingCalculations[savingGrade].Initialize(_calculations[k].SavedBits - oldConcurrentSavedBits, _calculations[k].VirtualBlock, oldConcurrent, (Block.SavingGrade)_appendingCalculationSavingGrade);
                }

                if (_calculations[k].VirtualBlock.HasPattern)
                {
                    _hasRunningPatternCalculation = false;
                }
                // _calculations.RemoveAt(k--);
            }
            _calculations.Clear();
        }

        private void UpdateReplacingCalculations(ref OfcNumber value, int bitDiffDiff = 0)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = _appendingCalculationSavingGrade + 1; j < _replacingCalculations.Length; j++)
            {
                if (!_replacingCalculations[j].IsValid) continue;

                var replacingCalculation = _replacingCalculations[j];
                var savingGrade = replacingCalculation.VirtualBlock.GetSavingGrade();
                if (!replacingCalculation.ProcessValue(value, _index))
                {
                    _replacingCalculations[j].IsValid = false;
                    continue;
                }

                if ((replacingCalculation.SavedBits += bitDiffDiff) > 0)
                {
                    _replacingCalculations[j].IsValid = false;

                    _calculations.Clear(); //Todo: not jsut clear, but make a replacingCalc on this replacing calc ...
                    _hasRunningPatternCalculation = false;

                    if (!replacingCalculation.OldConcurrentBlock.IsValid) //Todo: check what this was
                    {
                        ReplaceNewestBlock(BlockCalculation.FromReplacingCalculation(replacingCalculation));
                    }
                    else
                    {
                        _blocks[_blocks.Count - 1] = replacingCalculation.OldConcurrentBlock; // set last block to old, non-overlapping state
                        AddNewBlock(BlockCalculation.FromReplacingCalculation(replacingCalculation));
                    }

                    break;
                }

                var newSavingGrade = replacingCalculation.VirtualBlock.GetSavingGrade();

                if (savingGrade != newSavingGrade)
                {
                    _replacingCalculations[(int)savingGrade].IsValid = false;
                    var overriddenCalc = _replacingCalculations[(int)newSavingGrade];
                    if (overriddenCalc.IsValid && overriddenCalc.SavedBits > replacingCalculation.SavedBits)
                        continue;
                }

                if (newSavingGrade <= replacingCalculation.OldConcurrentSavingGrade)
                    replacingCalculation.IsValid = false;

                // replacingCalculations[j].IsValid = false;
                _replacingCalculations[(int)newSavingGrade] = replacingCalculation;
            }
        }

        public bool ProcessNextValue()
        {
            var value = Values[_index];
            var patternPred = _patternPredictor.PredictNext(value);

            #region adding value to last block and updating replacing calculations


            if (_isAppendingCalculationValid)
            {

                var lastBitDiff = _appendingCalculation.SavedBits;
                var lastAppendingBlock = _appendingCalculation.VirtualBlock;

                if (_appendingCalculation.ProcessValue(value, _index))
                {
                    var bifDiffDiff = lastBitDiff - _appendingCalculation.SavedBits;
                    if (_appendingCalculation.SavedBits > 0)
                    {
                        if (_appendingCalculation.VirtualBlock.Length - _lastStableBlock.Length > 1) // we made a jump, maybe there were some calcs started in the meantime?
                        {
                            TransformCalcsToReplaceCalsOrDelete(lastAppendingBlock, _appendingCalculation.SavedBits);
                        }

                        ReplaceNewestBlock(_appendingCalculation);
                    }

                    _appendingCalculationSavingGrade = (int)_appendingCalculation.VirtualBlock.GetSavingGrade();

                    UpdateReplacingCalculations(ref value, bifDiffDiff);

                    if (patternPred && _appendingCalculationSavingGrade < (int)Block.SavingGrade.Pattern && !_replacingCalculations[(int)Block.SavingGrade.Pattern].IsValid)
                    {
                        var block = new Block(_index, value.Exponent, value.NeededBitsNumber, value.IsNegative, value.Number, this, Methods.PatternSame, true);
                        var calc = _replacingCalculations[(int)Block.SavingGrade.Pattern];
                        calc.Initialize(-Headers.StandardBlockPatternSame, block, lastAppendingBlock, (Block.SavingGrade)_appendingCalculationSavingGrade);
                        if (calc.ProcessValue(value, _index))
                            _replacingCalculations[(int)Block.SavingGrade.Pattern] = calc;
                    }
                }
                else
                {
                    UpdateReplacingCalculations(ref value);
                    _isAppendingCalculationValid = false;
                }
            }
            else
            {
                UpdateReplacingCalculations(ref value);
            }

            #endregion

            #region updating current calculations if nessecary
            var isLastBlockUp2Date = _lastStableBlock.Index + _lastStableBlock.Length - 1 == _index;

            for (var j = 0; j < _calculations.Count; j++)
            {
                var calc = _calculations[j];

                // Todo: we shouldn't only remove calculations that are super far away, we should also remove calculations that have a very negative calc.SavedBits

                var hasPattern = calc.VirtualBlock.HasPattern;
                if (!calc.ProcessValue(value, _index))
                {
                    if (hasPattern)
                    {
                        _hasRunningPatternCalculation = false;
                    }
                    _calculations.RemoveAt(j--); // There was some error / the block is impossible, e.g. the exps are different and you can't even correct the values because of overflows ...
                    continue;
                }

                if (hasPattern && !calc.VirtualBlock.HasPattern)
                {
                    //throw new Exception("c#er is wrong");
                    _hasRunningPatternCalculation = false;
                }

                if (calc.SavedBits > 0)
                {
                    if (calc.VirtualBlock.HasPattern)
                        _hasRunningPatternCalculation = false;

                    if (_lastStableBlock.Index + _lastStableBlock.Length - 1 >= calc.VirtualBlock.Index)
                    {
#if debug
                        Console.WriteLine(_debugCounter);
                        _debugSkip = 0;
                        _debugEnabled = true;
#endif
                        _calculations.RemoveAt(j--);

                        continue;
                        throw new InvalidOperationException("Blocks sharing values! (ノ° ͜ʖ ͡°)ノ︵┻┻ ");
                    }

                    AddNewBlock(calc);

                    isLastBlockUp2Date = true;

                    for (var k = j + 1; k < _calculations.Count; k++) // we need to update the rest here or they jump one update tick if they get transformed to ReplaceCalcs ...
                    {
                        var calc2 = _calculations[k];
                        if (!calc2.ProcessValue(value, _index))
                        {
                            _calculations.RemoveAt(k--);
                            continue;
                        }
                        _calculations[k] = calc2;
                    }

                    TransformCalcsToReplaceCalsOrDelete(Block.InvalidBlock, calc.SavedBits, j); // we give an invalid block because the just created block and the already running calc could never co-exist. The just created block will get deleted if a replacing calc with an invalid oldConcurrentBLock gets founded

                    break; // Bug: actually, breaking here is pretty bad. There could be multiple calculations that want to create a block, and maybe this is not the best one. Saves some performance though
                }

                _calculations[j] = calc;
            }

            #endregion

            #region starting new calculations if nessecary

            //if (isLastBlockUp2Date)
            //{
            //    if (patternPred && _appendingCalculationSavingGrade < (int)Block.SavingGrade.Pattern && !_replacingCalculations[(int)Block.SavingGrade.Pattern].IsValid)
            //    {
            //        var block = new Block(_index, value.Exponent, value.NeededBitsNumber, value.IsNegative, value.Number, this, Methods.PatternSame, true);
            //        var calc = _replacingCalculations[(int)Block.SavingGrade.Pattern];
            //        calc.Initialize(-Headers.StandardBlockPatternSame, block, _lastStableBlock, (Block.SavingGrade)_appendingCalculationSavingGrade);
            //        if (calc.ProcessValue(ref value, _index))
            //            _replacingCalculations[(int)Block.SavingGrade.Pattern] = calc;
            //    }
            //}
            //else 
            if (!_hasRunningPatternCalculation && !isLastBlockUp2Date)
            {
                if (patternPred)
                {
                    var calc = new BlockCalculation(-Headers.StandardBlockPatternSame, new Block(_index, value.Exponent, 0, false, value.Number, this, Methods.PatternSame, true));
                    if (calc.ProcessValue(value, _index))
                    {
                        _calculations.Add(calc);
                        _hasRunningPatternCalculation = true;
                    }
                }
                else
                {
                    var hasExp = false;
                    foreach (var runningCalculation in _calculations) //Todo: pre calculate that (bool array?)
                    {
                        if (runningCalculation.VirtualBlock.Exponent == value.Exponent)
                        {
                            hasExp = true;
                            break;
                        }
                    }
                    if (!hasExp)
                    {
                        BlockCalculation calc;

                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression - You can't see **** if you do a ?:
                        if (value.Exponent == 0)
                            calc = new BlockCalculation(-Headers.StandardBlockNumbersNoExp, new Block(_index, 0, value.NeededBitsNumber, value.IsNegative, value.Number, this, Methods.NumbersNoExp, false));
                        else
                            calc = new BlockCalculation(-Headers.StandardBlockFloatSimmilar, new Block(_index, value.Exponent, value.NeededBitsNumber, value.IsNegative, value.Number, this, Methods.FloatSimmilar, false));

                        if (calc.ProcessValue(value, _index))
                            _calculations.Add(calc);
                        else
                        {
                            throw new Exception("Nice blockfinding Kappa");
                        }
                    }
                }
            }
            #endregion

            return ++_index < ValueCount;
        }


    }
}
