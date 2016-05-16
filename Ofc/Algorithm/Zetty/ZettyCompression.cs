namespace Ofc.Algorithm.Zetty
{
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Algorithm.Integration;
    using Ofc.Core;

    public class ZettyCompression : IReporter<OfcNumber>
    {
        public IConfiguaration Configuaration { get; }
        private List<OfcNumber> _values = new List<OfcNumber>();
        private readonly BinaryWriter _writer;

        public ZettyCompression(Stream outStream)
        {
            _writer = new BinaryWriter(outStream);

        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

        public void Finish()
        {
            if (_values.Count == 0) return;

            //for (var i = 0; i < _values.Count; i++)
            //{
            //    _writer.Write(_values[i].Reconstructed);
            //}

            //var meta = BlockyMetadata.FromData(_values);

            //meta.Write(_writer);
            //for (var i = 0; i < _values.Count; i++)
            //{
            //    if (!meta.IsAbsolute)
            //        _writer.WriteByte(_values[i].IsNegative ? (byte)1 : (byte)0, 1);
            //    _writer.Write((ulong)_values[i].Number, meta.MaxNeededBitsNumber);
            //}

            //for (var i = 0; i < _values.Count; i++)
            //{
            //    if (!meta.IsAbsolute)
            //        _writer.WriteByte(_values[i].Exponent < 0 ? (byte)1 : (byte)0, 1);
            //    _writer.Write((ulong)Math.Abs(_values[i].Exponent), meta.MaxNeededBitsNumber);
            //}
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Report(OfcNumber value)
        {
            _values.Add(value);
        }

        public void Report(OfcNumber[] values, int offset, int amount)
        {
            for (var i = offset; i < offset + amount; i++)
            {
                Report(values[i]);
            }
        }
    }
}
