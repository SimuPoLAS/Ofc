// ReSharper disable StaticMemberInGenericType

// ReSharper disable ForCanBeConvertedToForeach
namespace Ofc.Parsing.Hooks
{
    using System;
    using System.IO;
    using System.Text;
    using JetBrains.Annotations;
    using Ofc.IO;
    using Ofc.Util;
    using OfcCore;

    internal class BinaryOutputHook<T> : IParserHook<string>
    {
        internal const byte Version = 1;

        private static readonly byte[] signature = {98, 79, 70, 67, 70, 126};
        private const double EPSILON = 0.000001;

        private int G_GENERAL = 0, G_KEYWORD = 2, G_STRING = 3, G_LIST = 4, G_DIRECTIVE = 6, G_DIMENSION = 7;

        private IDataWriter _writer;
        private IAlgorithm<T> _algorithm;
        private IConverter<T> _converter;
        private Encoding _encoding;

        public BinaryOutputHook(string output, IAlgorithm<T> algorithm, IConverter<T> converter)
        {
            _algorithm = algorithm;
            _converter = converter;
            _encoding = new UTF8Encoding();
            OpenFile(output);
        }


        private void OpenFile(string target)
        {
            if (!File.Exists(target)) throw new FileNotFoundException("Could not find the target file.", target);
            FileStream stream = null;
            try
            {
                stream = new FileStream(target, FileMode.Create);
                _writer = new BinaryDataWriter(stream);
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        public void WriteSignature()
        {
            _writer.Write(signature, 0, signature.Length);
            _writer.WriteByte(Version);
            _writer.WriteInt(0);
            _writer.WriteUShort(0);
        }


        public void EnterDictionary(string name)
        {
            _writer.WriteByte(2);
        }

        public void LeaveDictionary()
        {
            _writer.WriteByte(0);
        }

        public void EnterCodeStreamDictionary(string name)
        {
            _writer.WriteByte(3);
        }

        public void LeaveCodeStreamDictionary()
        {
            _writer.WriteByte(0);
        }

        public void EnterEntry(string name)
        {
            _writer.WriteByte(4);
        }

        public void LeaveEntry()
        {
            _writer.WriteByte(0);
        }

        public void EnterList(OfcListType type, int capacity)
        {
            throw new NotImplementedException();
        }

        public void HandleListEntry(string value)
        {
            throw new NotImplementedException();
        }

        public void HandleListEntries(string[] values)
        {
            throw new NotImplementedException();
        }

        public void LeaveList()
        {
            throw new NotImplementedException();
        }

        public void HandleMacro(OfcMacroType macro, [CanBeNull] string data)
        {
            throw new NotImplementedException();
        }

        public void HandleDimension(string[] values) // todo same
        {
            if (values == null || values.Length != 7) throw new ArgumentException();
            for (var i = 0; i < values.Length; i++)
            {
                double value;
                if (!double.TryParse(values[i], out value)) throw new ArgumentException();
                _writer.WriteDouble(value);
            }
        }

        public void HandleScalar(string value)
        {
            double number;
            if (!double.TryParse(value, out number)) throw new ArgumentException();
            var isInt = (number%1) < EPSILON;
            if (!isInt)
            {
                _writer.WriteId(G_GENERAL, 7);
                _writer.WriteDouble(number);
            }
            else
            {
                var intValue = (long) number;
                var neg = intValue < 0;
                _writer.WriteId(G_GENERAL, neg ? 9 : 8);
                _writer.WriteVarLong(neg ? ~intValue : intValue);
            }
        }

        public void HandleKeyword(string value)
        {
            var bytes = _encoding.GetBytes(value);
            var l = bytes.Length;

            // special case empty keyword
            if (l == 0)
            {
                _writer.WriteId(G_KEYWORD, 0);
                return;
            }

            // test for same bytes over and over
            var t = bytes[0];
            var same = true;
            for (var i = 1; i < l; i++)
                if (t != bytes[i])
                {
                    same = false;
                    break;
                }

            // special case small keyword
            if (l <= 12)
            {
                _writer.WriteId(G_KEYWORD, l + 3);
                if (same) _writer.WriteByte(t);
                else _writer.WriteBytes(bytes);
                return;
            }

            // any other case: length index up front
            var needed = NumberHelper.NeededBytes(l);
            if (needed <= 0) throw new NotSupportedException();
            if (needed > 2) throw new ArgumentException("The specified keyword is too big.");
            _writer.WriteId(G_KEYWORD, needed + 1);
            _writer.Write(BitConverter.GetBytes(needed), 0, needed);
            if (same) _writer.WriteByte(t);
            else _writer.WriteBytes(bytes);
        }

        public void HandleString(string data)
        {
            var bytes = _encoding.GetBytes(data);
            var l = bytes.Length;

            // special case empty string
            if (l == 0)
            {
                _writer.WriteId(G_STRING, 0);
                return;
            }

            // test for same bytes over and over
            var t = bytes[0];
            var same = true;
            for (var i = 1; i < l; i++)
                if (t != bytes[i])
                {
                    same = false;
                    break;
                }

            // special case small string
            if (l <= 12)
            {
                _writer.WriteId(G_STRING, l + 3);
                if (same) _writer.WriteByte(t);
                else _writer.WriteBytes(bytes);
                return;
            }

            // any other case: length index up front
            var needed = NumberHelper.NeededBytes(l);
            if (needed <= 0) throw new NotSupportedException();
            if (needed > 2) throw new ArgumentException("The specified keyword is too big.");
            _writer.WriteId(G_STRING, needed + 1);
            _writer.Write(BitConverter.GetBytes(needed), 0, needed);
            if (same) _writer.WriteByte(t);
            else _writer.WriteBytes(bytes);
        }

        public void Flush()
        {
            _writer.Flush();
        }
    }
}