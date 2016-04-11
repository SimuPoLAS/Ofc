namespace Ofc.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ofc.Util;
    using OfcCore;
    using OfcCore.Configurations;

    public class BinaryInputReader<T> : IDisposable
    {
        /* Needed vars */
        private Stream _stream;
        private IDataReader _input;
        private TextWriter _output;
        private IAlgorithm<T> _algorithm;
        private IConverter<T> _converter;
        private Reporter _reporter;

        public BinaryInputReader(Stream input, TextWriter output, IAlgorithm<T> algorithm, IConverter<T> converter)
        {
            _stream = input;
            _input = new BinaryDataReader(input);
            _output = output;
            _algorithm = algorithm;
            _converter = converter;
            _reporter = new Reporter(this);
        }


        internal void Read()
        {
            byte id;
            while ((id = _input.ReadByte()) != 0)
                ReadAny(id);
        }

        private void ReadDictionary()
        {
            var name = _input.ReadString();
            _output.WriteLine($"{name} {{");
            byte id;
            while ((id = _input.ReadByte()) != 0)
                ReadAny(id);
            _output.WriteLine("}");
        }

        private void ReadCodeStreamDictionary()
        {
            var name = _input.ReadString();
            _output.WriteLine($"{name} #codeStream {{");
            byte id;
            while ((id = _input.ReadByte()) != 0)
                ReadAny(id);
            _output.WriteLine("}");
        }

        private void ReadEntry()
        {
            var name = _input.ReadString();
            _output.Write($"{name} ");
            byte id;
            while ((id = _input.ReadByte()) != 0)
                ReadAny(id);
            _output.WriteLine(";");
        }

        private void ReadAny(byte id = 0)
        {
            if (id == 0) id = _input.ReadByte();
            if (id == 0) return;
            switch (id)
            {
                case 1:
                    ReadDictionary();
                    break;
                case 2:
                    ReadCodeStreamDictionary();
                    break;
                case 3:
                    ReadEntry();
                    break;
                case 4:
                    _reporter.Width = 1;
                    _output.WriteLine($"List<scalar> (");
                    _algorithm.Decompress(null, _reporter.Configuaration, _stream, _reporter);
                    _output.WriteLine(");");
                    break;
                case 5:
                    _reporter.Width = 3;
                    _output.WriteLine($"List<vector> (");
                    _algorithm.Decompress(null, _reporter.Configuaration, _stream, _reporter);
                    _output.WriteLine(");");
                    break;
                case 6:
                    _reporter.Width = 9;
                    _output.WriteLine($"List<tensor> (");
                    _algorithm.Decompress(null, _reporter.Configuaration, _stream, _reporter);
                    _output.WriteLine(");");
                    break;
                case 7:
                    throw new NotImplementedException();
                    _algorithm.Decompress(null, _reporter.Configuaration, _stream, _reporter);
                    break;
                case 8:
                {
                    var data = _input.ReadString();
                    _output.WriteLine($"#include \"{data}\"");
                    break;
                }
                case 9:
                {
                    var data = _input.ReadString();
                    _output.WriteLine($"#inputMode {data}");
                    break;
                }
                case 10:
                {
                    var data = _input.ReadString();
                    _output.WriteLine($"#remove {data}");
                    break;
                }
                case 11:
                    _output.Write(" [");
                    for (var i = 0; i < 7; i++)
                        _output.Write($"{_input.ReadDouble()}{(i == 6 ? "]" : " ")}");
                    break;
                case 12:
                {
                    var data = _input.ReadDouble();
                    _output.Write($" {data}");
                    break;
                }
                case 13:
                {
                    var data = _input.ReadString();
                    _output.Write($" {data}");
                    break;
                }
                case 14:
                {
                    var data = _input.ReadString();
                    _output.Write($" \"{data}\"");
                    break;
                }
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }


        private class Reporter : IReporter<T>
        {
            public IConfiguaration Configuaration { get; }

            internal int Width { get; set; } = 1;


            private BinaryInputReader<T> _obj;

            private Stack<string> _stack = new Stack<string>(); 


            public Reporter(BinaryInputReader<T> obj)
            {
                _obj = obj;
                Configuaration = new SimpleConfiguration();
            }

            public void Finish()
            {
                FlushIfNeeded();
            }

            public void Flush()
            {
                FlushIfNeeded();
                _obj._output.Flush();
            }

            public void Report(T value)
            {
                _stack.Push(_obj._converter.ToString(value));
            }

            public void Report(T[] values, int offset, int amount)
            {
                for (var i = 0; i < amount; i++)
                    _stack.Push(_obj._converter.ToString(values[offset + i]));
            }

            public void Dispose()
            {
                Finish();
            }


            private void FlushIfNeeded()
            {
                if (Width <= 0) throw new NotSupportedException();
                if (Width == 1)
                {
                    while (_stack.Count > 0)
                        _obj._output.Write($" {_stack.Pop()}");
                }
                else
                {
                    if (_stack.Count < Width) return;
                    var data = new string[Width];
                    while (_stack.Count >= Width)
                    {
                        for (var i = 0; i < Width; i++)
                            data[i] = _stack.Pop();
                        _obj._output.WriteLine($" ({string.Join(" ", data)})");
                    }
                }
            }
        }
    }
}