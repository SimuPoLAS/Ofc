// ReSharper disable StaticMemberInGenericType
// ReSharper disable ForCanBeConvertedToForeach

namespace Ofc.Parsing.Hooks
{
    using System;
    using System.IO;
    using System.Text;
    using JetBrains.Annotations;
    using Ofc.IO;
    using Ofc.IO.Handlers;
    using Ofc.Util;
    using OfcCore;
    using OfcCore.Configurations;

    internal class BinaryOutputHook<T> : IParserHook<string>
    {
        private const double EPSILON = 0.000001;

        private const int G_GENERAL = 0, G_LIST = 1, G_KEYWORD = 2, G_STRING = 3, G_DIRECTIVE = 6, G_DIMENSION = 7;

        private IDataWriter _writer;
        private IAlgorithm<T> _algorithm;
        private bool _hasAlgorithm;
        private IConverter<T> _converter;
        private Encoding _encoding;

        private IFile _target = null;
        private IConfiguaration _configuaration = EmptyConfiguration.Instance;

        private IHandler<string> _listHandler;
        private bool _inList;


        public BinaryOutputHook(string output, [CanBeNull] IAlgorithm<T> algorithm, [CanBeNull] IConverter<T> converter)
        {
            _algorithm = algorithm;
            _hasAlgorithm = algorithm != null;
            if (_hasAlgorithm && _converter == null) throw new ArgumentNullException(nameof(converter));
            _converter = converter;
            _encoding = new UTF8Encoding();
            OpenFile(output);
        }


        private void OpenFile(string target)
        {
            if (!File.Exists(target)) throw new FileNotFoundException("Could not find the specified file.", target);
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


        public void EnterDictionary(string name)
        {
            _writer.WriteId(G_GENERAL, 2);
        }

        public void LeaveDictionary()
        {
            _writer.WriteId(G_GENERAL, 0);
        }

        public void EnterCodeStreamDictionary(string name)
        {
            _writer.WriteId(G_GENERAL, 3);
        }

        public void LeaveCodeStreamDictionary()
        {
            _writer.WriteId(G_GENERAL, 0);
        }

        public void EnterEntry(string name)
        {
            _writer.WriteId(G_GENERAL, 4);
        }

        public void LeaveEntry()
        {
            _writer.WriteId(G_GENERAL, 0);
        }

        public void EnterList(OfcListType type, int capacity)
        {
            if (_inList) throw new NotSupportedException("Stacking not supported.");
            _inList = true;
            if (!_hasAlgorithm) _listHandler = new RawListHandler(_writer);
            else
            {
                switch (type)
                {
                    case OfcListType.Scalar:

                        break;
                    case OfcListType.Vector:

                        break;
                    case OfcListType.Tensor:

                        break;
                    case OfcListType.Anonymous:
                        throw new NotSupportedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            
        }

        public void HandleListEntry(string value)
        {
            if (!_inList) throw new NotSupportedException();
            _listHandler.HandleEntry(value);
        }

        public void HandleListEntries(string[] values)
        {
            if (!_inList) throw new NotSupportedException();
            _listHandler.HandleEntries(values, 0, values.Length);
        }

        public void LeaveList()
        {
            if (!_inList) throw new InvalidOperationException("No list is opened.");
            _listHandler.End();
        }

        public void HandleMacro(OfcMacroType macro, [CanBeNull] string data)
        {
            switch (macro)
            {
                case OfcMacroType.Include:
                    _writer.WriteId(G_DIRECTIVE, 0);
                    break;
                case OfcMacroType.InputMode:
                    _writer.WriteId(G_DIRECTIVE, 1);
                    break;
                case OfcMacroType.Remove:
                    _writer.WriteId(G_DIRECTIVE, 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(macro), macro, null);
            }
        }

        public void HandleDimension(string[] values)
        {
            if (values == null || values.Length != 7) throw new ArgumentException();
            var t = values[0];
            var same = true;
            for (var i = 1; i < values.Length; i++)
                if (t != values[i])
                {
                    same = false;
                    break;
                }
            if (same)
            {
                _writer.WriteDouble(double.Parse(t));
                return;
            }
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
            var isInt = number%1 < EPSILON;
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