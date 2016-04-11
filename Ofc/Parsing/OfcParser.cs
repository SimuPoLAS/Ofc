// ReSharper disable LoopCanBeConvertedToQuery


#pragma warning disable CS0612

namespace Ofc.Parsing
{
    using System;
    using JetBrains.Annotations;
    using Ofc.IO;
    using Ofc.Parsing.Hooks;

    internal class OfcParser : IPositionProvider
    {
        /** base members */
        private OfcToken[] _buffer;
        private bool _eos;
        private int _length;
        private int _position;
        private int _size;
        private IInputStream<OfcToken> _source;

        /** parser specific members */
        private readonly IParserHook<string> _hook;


        public OfcParser([CanBeNull] IInputStream<OfcToken> input) : this(input, null)
        {
        }

        public OfcParser([CanBeNull] IInputStream<OfcToken> input, [CanBeNull] IParserHook<string> hook) : this(input, hook, 4096)
        {
        }

        public OfcParser([CanBeNull] IInputStream<OfcToken> input, [CanBeNull] IParserHook<string> hook, int bufferSize)
        {
            if (bufferSize < 64) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            /** set base members */
            _buffer = new OfcToken[bufferSize];
            _eos = input == null;
            _length = 0;
            _position = 0;
            _size = bufferSize;
            _source = input;

            /** set parser members */
            _hook = hook ?? EmptyHook<string>.Instance;
        }

        #region Base parser methods

        private void Ensure(int amount)
        {
            if (amount <= _length) return;
            if (_eos) throw new ParserException();
            Fill();
            if (amount <= _length) return;
            throw new ParserException();
        }

        private void Fill()
        {
            if (_eos) throw new ParserException();
            if (_length == 0) _position = 0;
            else if (_position != 0) Relocate();
            var difference = _size - _length;
            var read = _source.Read(_buffer, _length, difference);
            if (read != difference) _eos = true;
            _length += read;
        }

        private bool Needs(int amount)
        {
            if (amount <= _length) return true;
            if (_eos) return false;
            Fill();
            return amount <= _length;
        }

        private void Skip()
        {
            _position++;
            _length--;
        }

        private void Skip(int amount)
        {
            _position += amount;
            _length -= amount;
        }

        private void Relocate()
        {
            if (_length == 0) _position = 0;
            if (_position == 0) return;
            for (var i = 0; i < _length; i++)
                _buffer[i] = _buffer[_position + i];
            _position = 0;
        }

        #endregion

        #region Parser methods

        private bool Expect(OfcTokenType type)
        {
            Ensure(1);
            return _buffer[_position].Type == type;
        }

        private bool Expect(OfcTokenType[] types)
        {
            if (types.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(types));
            Ensure(types.Length);
            for (var i = 0; i < types.Length; i++)
                if (_buffer[_position + i].Type != types[i])
                    return false;
            return true;
        }


        internal void Parse()
        {
            while (Needs(1))
            {
                var tkn = _buffer[_position];

                if (tkn.Type == OfcTokenType.KEYWORD || tkn.Type == OfcTokenType.STRING)
                {
                    Skip(1);
                    ParseEntryOrObject(tkn);
                    continue;
                }
                if (tkn.Type == OfcTokenType.NUMBER && Needs(2) && _buffer[_position + 1].Type == OfcTokenType.PARENTHESE_OPEN)
                {
                    Skip(2);
                    double a;
                    if (!double.TryParse(tkn.Payload, out a)) ParseAnonymousList();
                    else ParseAnonymousList((int) a);
                    continue;
                }
                if (tkn.Type == OfcTokenType.PARENTHESE_OPEN)
                {
                    Skip(1);
                    ParseAnonymousList();
                    continue;
                }
                if (tkn.Type == OfcTokenType.HASHTAG)
                {
                    ParseDirective();
                    continue;
                }

                throw new ParserException();
            }
        }

        private void ParseEntryOrObject(OfcToken me)
        {
            if (!Needs(1)) throw new ParserException();
            var c = _buffer[_position];
            if (c.Type == OfcTokenType.BRACES_OPEN)
            {
                Skip(1);
                ParseObject(me);
            }
            else if (c.Type == OfcTokenType.HASHTAG)
            {
                Skip(1);
                ParseCodeStreamObject(me);
            }
            else ParseEntry(me);
        }

        private void ParseObject(OfcToken me)
        {
            _hook.EnterDictionary(me.Payload ?? string.Empty);
            while (!(_eos && _length == 0))
            {
                if (!Needs(1)) throw new ParserException();
                var tkn = _buffer[_position];
                if (tkn.Type == OfcTokenType.KEYWORD || tkn.Type == OfcTokenType.STRING)
                {
                    Skip(1);
                    ParseEntryOrObject(tkn);
                }
                else if (tkn.Type == OfcTokenType.NUMBER || tkn.Type == OfcTokenType.PARENTHESE_OPEN)
                {
                    ParseAnonymousList();
                }
                else if (tkn.Type == OfcTokenType.SEMICOLON)
                {
                    Skip(1);
                }
                else if (tkn.Type == OfcTokenType.BRACES_CLOSE)
                {
                    _hook.LeaveDictionary();
                    Skip(1);
                    return;
                }
                else if (tkn.Type == OfcTokenType.HASHTAG)
                {
                    ParseDirective();
                }
                else throw new ParserException();
            }
            throw new ParserException();
        }

        private void ParseCodeStreamObject(OfcToken me)
        {
            if (!Expect(OfcTokenType.KEYWORD)) throw new ParserException();
            var text = _buffer[_position].Payload;
            if (text != "codeStream") throw new ParserException();
            Skip(1);
            if (!Expect(OfcTokenType.BRACES_OPEN)) throw new ParserException();
            Skip(1);
            _hook.EnterCodeStreamDictionary(me.Payload ?? string.Empty);
            while (!(_eos && _length == 0))
            {
                if (!Needs(1)) throw new ParserException();
                var tkn = _buffer[_position];
                if (tkn.Type == OfcTokenType.KEYWORD || tkn.Type == OfcTokenType.STRING)
                {
                    Skip(1);
                    ParseEntryOrObject(tkn);
                }
                else if (tkn.Type == OfcTokenType.NUMBER || tkn.Type == OfcTokenType.PARENTHESE_OPEN)
                {
                    ParseAnonymousList();
                }
                else if (tkn.Type == OfcTokenType.SEMICOLON)
                {
                    Skip(1);
                }
                else if (tkn.Type == OfcTokenType.BRACES_CLOSE)
                {
                    _hook.LeaveCodeStreamDictionary();
                    Skip(1);
                    return;
                }
                else if (tkn.Type == OfcTokenType.HASHTAG)
                {
                    ParseDirective();
                }
                else throw new ParserException();
            }
            throw new ParserException();
        }

        private void ParseEntry(OfcToken me)
        {
            _hook.EnterEntry(me.Payload ?? string.Empty);
            while (!(_eos && _length == 0))
            {
                if (!Needs(1)) throw new ParserException();
                var tkn = _buffer[_position];
                if (tkn.Type == OfcTokenType.SEMICOLON)
                {
                    _hook.LeaveEntry();
                    Skip(1);
                    return;
                }
                ParseValue(tkn);
            }
            throw new ParserException();
        }

        private void ParseDirective()
        {
            if (_buffer[_position].Type != OfcTokenType.HASHTAG) throw new ParserException();
            Skip();
            Expect(OfcTokenType.STRING);
            var macro = _buffer[_position].Payload;
            OfcDirectiveType type;
            switch (macro)
            {
                case "include":
                    type = OfcDirectiveType.Include;
                    break;
                case "inputMode":
                    type = OfcDirectiveType.InputMode;
                    break;
                case "remove":
                    type = OfcDirectiveType.Remove;
                    break;
                default:
                    throw new ParserException();
            }
            Skip();
            Ensure(1);
            if (_buffer[_position].Type != OfcTokenType.KEYWORD && _buffer[_position].Type != OfcTokenType.STRING) throw new ParserException();
            _hook.HandleMacro(type, _buffer[_position].Payload);
            Skip();
        }

        private void ParseValue(OfcToken me)
        {
            switch (me.Type)
            {
                case OfcTokenType.KEYWORD:
                    if (me.Payload == "List")
                    {
                        Skip(1);
                        if (!Expect(new[] {OfcTokenType.CHEVRONS_OPEN, OfcTokenType.KEYWORD, OfcTokenType.CHEVRONS_CLOSE})) throw new ParserException();
                        var ltype = _buffer[_position + 1].Payload;
                        OfcListType type;
                        switch (ltype)
                        {
                            case "scalar":
                                type = OfcListType.Scalar;
                                break;
                            case "vector":
                                type = OfcListType.Vector;
                                break;
                            case "tensor":
                                type = OfcListType.Tensor;
                                break;
                            default:
                                throw new ParserException();
                        }
                        Skip(3);
                        if (!Needs(1)) throw new ParserException();
                        var amount = -1;
                        var c = _buffer[_position];
                        if (c.Type == OfcTokenType.NUMBER)
                        {
                            amount = (int) double.Parse(c.Payload);
                            Skip(1);
                        }
                        if (!Expect(OfcTokenType.PARENTHESE_OPEN)) throw new ParserException();
                        Skip(1);
                        _hook.EnterList(type, amount);
                        var done = false;
                        var d = false;
                        do
                        {
                            if (!Needs(1)) throw new ParserException();
                            c = _buffer[_position];
                            if (c.Type == OfcTokenType.PARENTHESE_CLOSE)
                            {
                                d = true;
                                _hook.LeaveList();
                                Skip(1);
                                done = true;
                                break;
                            }
                            switch (type)
                            {
                                case OfcListType.Scalar:
                                    ParseScalar();
                                    break;
                                case OfcListType.Vector:
                                    ParseVector();
                                    break;
                                case OfcListType.Tensor:
                                    ParseTensor();
                                    break;
                                default:
                                    throw new Exception("Not supported list type: " + type);
                            }
                        } while (!(_eos && _length == 0));
                        if (!d) _hook.LeaveList();
                        if (!done) throw new ParserException();
                        return;
                    }

                    Skip(1);
                    _hook.HandleKeyword(me.Payload ?? string.Empty);
                    return;
                case OfcTokenType.NUMBER:
                    if (Needs(2) && _buffer[_position + 1].Type == OfcTokenType.PARENTHESE_OPEN)
                    {
                        Skip(2);
                        double a;
                        if (!double.TryParse(me.Payload, out a)) ParseAnonymousList();
                        else ParseAnonymousList((int) a);
                        return;
                    }

                    Skip(1);
                    _hook.HandleScalar(me.Payload);
                    return;
                case OfcTokenType.STRING:
                    Skip(1);
                    _hook.HandleString(me.Payload ?? string.Empty);
                    return;
                case OfcTokenType.BRACKETS_OPEN:
                    Skip(1);
                    if (!Expect(new[] {OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.BRACKETS_CLOSE})) throw new ParserException();
                    var values = new string[7];
                    for (var i = 0; i < 7; i++)
                        values[i] = _buffer[_position + i].Payload;
                    Skip(8);
                    _hook.HandleDimension(values);
                    return;
                case OfcTokenType.PARENTHESE_OPEN:
                    Skip(1);
                    ParseAnonymousList();
                    return;
                default:
                    throw new ParserException();
            }
        }

        private void ParseAnonymousList(int number = -1)
        {
            if (!Needs(1)) throw new ParserException();
            var c = _buffer[_position];
            _hook.EnterList(OfcListType.Anonymous, number);
            for (; c.Type != OfcTokenType.PARENTHESE_CLOSE; c = _buffer[_position])
            {
                if (c.Type == OfcTokenType.KEYWORD && Needs(2) && _buffer[_position + 1].Type == OfcTokenType.BRACES_OPEN)
                {
                    Skip(2);
                    ParseObject(c);
                }
                else ParseValue(c);
                if (!Needs(1)) throw new ParserException();
            }
            _hook.LeaveList();
            Skip(1);
        }

        private void ParseScalar()
        {
            if (!Expect(OfcTokenType.NUMBER)) throw new ParserException();
            _hook.HandleListEntry(_buffer[_position].Payload ?? string.Empty);
            Skip(1);
        }

        private void ParseVector()
        {
            if (!Expect(new[] {OfcTokenType.PARENTHESE_OPEN, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.PARENTHESE_CLOSE})) throw new ParserException();
            var values = new string[3];
            for (var i = 0; i < 3; i++)
                values[i] = _buffer[_position + i + 1].Payload ?? string.Empty;
            _hook.HandleListEntries(values);
            Skip(5);
        }

        private void ParseTensor()
        {
            if (!Expect(new[] {OfcTokenType.PARENTHESE_OPEN, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.NUMBER, OfcTokenType.PARENTHESE_CLOSE})) throw new ParserException();
            var values = new string[9];
            for (var i = 0; i < 9; i++)
                values[i] = _buffer[_position + i + 1].Payload ?? string.Empty;
            _hook.HandleListEntries(values);
            Skip(11);
        }

        #endregion

        public uint Position => _buffer[_position].Position;
    }
}