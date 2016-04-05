namespace OfcAlgorithm.Herbert
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OfcAlgorithm.Integration;
    using OfcCore;
    using OfcCore.Configurations;
    using OfcCore.Utility;

    internal class HerbertCompression : IReporter<OfcNumber>
    {
        public IConfiguaration Configuaration { get; } = new SimpleConfiguration();

        private Stream _writer;
        private List<long> Values;
        public int Layers { get; }
        public bool SupportsLayer => true;
        private int[] _occurences = new int[64];
        private IFile _file;

        public HerbertCompression(int elements, Stream writer)
        {
            Values = elements < 4 ? new List<long>() : new List<long>(elements);
            _writer = writer;
        }

        public HerbertCompression(IFile file, int elements, Stream writer) : this(elements, writer)
        {
            _file = file;
        }

        private int BruteForceSolution(int max, int count, byte index = 1)
        {
            var aggrCount = 0;
            var maxSaved = 0;
            for (var i = index; i < max; i++)
            {
                aggrCount += _occurences[i];
                var save = aggrCount*(max - i - 1) - (count - aggrCount);
                if (save <= 0) continue;

                save += BruteForceSolution(max, count - aggrCount, (byte) (i + 1));
                if (save > maxSaved)
                {
                    maxSaved = save;
                }
            }
            return maxSaved;
        }

        private List<int> CalculateSolution(int max, int count, ref int save)
        {
            var aggrCount = 0;
            save = 0;
            var points = new List<int>();
            for (var i = 0; i < max; i++)
            {
                aggrCount += _occurences[i];
                var tempSave = aggrCount*(max - i - 1) - (count - aggrCount);
                if (tempSave <= 0) continue;

                aggrCount = 0;
                save += tempSave;
                points.Add(i);
            }
            return points;
        }

        public void Finish()
        {
            // throw new NotImplementedException("This is not ready for use at all");

            var max = 0;
            var count = 0;
            for (var i = 0; i < _occurences.Length; i++)
            {
                if (_occurences[i] > 0)
                {
                    max = i;
                    count += _occurences[i];
                }
            }

            var bitcount = max*count;


            var x = BruteForceSolution(max, count);
            var y = 0;
            var points = CalculateSolution(max, count, ref y);
            Console.WriteLine(x/(float) bitcount*100f + " % @ " + y/(float) bitcount*100f + " count: " + count); // + _file?.Name);


            if (x != 0)
            {
            }
            //var points = new List<byte>();

            //for (var i = points.Count - 1; i > 0; i--)
            //{
            //    points[i] -= points[i - 1];
            //}

            var stream = new StreamBitWriter(_writer);


            if (points.Count == 0)
            {
                for (var i = 0; i < Values.Count; i++)
                {
                    stream.WriteByte(Values[i] < 0 ? (byte) 1 : (byte) 0, 1);
                    stream.Write((ulong) Values[i], (byte) max);
                }
                return;
            }

            for (var i = 0; i < Values.Count; i++)
            {
                stream.WriteByte(Values[i] < 0 ? (byte) 1 : (byte) 0, 1);
                var data = (ulong) Values[i];
                var pOffset = 0;
                var initialSectionBits = points[pOffset++];
                stream.Write(data, (byte) initialSectionBits);
                data = data >> initialSectionBits;
                while (data > 0)
                {
                    if (pOffset == points.Count)
                    {
                        stream.Write(data, (byte) (max - points[pOffset - 1]));
                        break;
                    }
                    var extendedSectionBits = points[pOffset++];
                    var d = (data << 1) | 1;
                    stream.Write(d, (byte) (extendedSectionBits + 1));
                    data = data >> extendedSectionBits;
                }
            }
            //stream.Write(data, initialSectionBits);
            //data = data >> initialSectionBits;
            //while (data > 0)
            //{
            //    var d = (data << 1) | 1;
            //    stream.Write(d, (byte)(extendedSectionBits + 1));
            //    data = data >> extendedSectionBits;
            //}
            //stream.WriteByte(0, 1);
        }


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

        private long lastNumber;

        public void Report(OfcNumber number)
        {
            Values.Add(number.Number - lastNumber);
            lastNumber = number.Number;
            //  Values.Add(number.Exponent);

            _occurences[lastNumber.GetNeededBits()]++;
        }

        public void Report(OfcNumber[] numbers, int offset, int count)
        {
            for (var i = offset; i < offset + count; i++)
            {
                Report(numbers[i]);
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}