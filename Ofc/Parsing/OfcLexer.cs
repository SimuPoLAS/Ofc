#pragma warning disable CS0612

namespace Ofc.Parsing
{
    using System;
    using JetBrains.Annotations;
    using Ofc.IO;

    internal sealed class OfcLexer : IInputStream<OfcToken>
    {
        /** base members */
        private char[] _buffer;
        private bool _eos;
        private int _length;
        private int _position;
        private int _size;
        private IInputStream<char> _source;

        /** lexer base members */
        private bool _recordPosition;
        private uint _tabSize = 4;

        private char[] _textBuffer;
        private int _textPosition;
        private int _textLength;
        private int _textSize;

        private uint _currentLine = 1;
        private uint _currentColumn = 1;
        private uint _currentLength;

        private uint _tokenLine;
        private uint _tokenColumn;
        private uint _tokenLength;

        /** lexer specific members */
        private bool _symbolOnly;


        public OfcLexer([CanBeNull] IInputStream<char> input) : this(input, true)
        {
        }

        public OfcLexer([CanBeNull] IInputStream<char> input, bool recordPosition) : this(input, recordPosition, 4096)
        {
        }

        public OfcLexer([CanBeNull] IInputStream<char> input, bool recordPosition, int bufferSize)
        {
            if (bufferSize < 64) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            /** set base members */
            _buffer = new char[bufferSize];
            _eos = input == null;
            _length = 0;
            _position = 0;
            _size = bufferSize;
            _source = input;

            /** set base lexer members */
            _textBuffer = new char[64];
            _textPosition = 0;
            _textLength = 0;
            _textSize = 64;
            _recordPosition = recordPosition;
        }

        #region Base lexer methods

        private void DoubleBuffer()
        {
            _textSize *= 2;
            var newBuffer = new char[_textSize];
            Buffer.BlockCopy(_textBuffer, 0, newBuffer, 0, _textSize);
            _textBuffer = newBuffer;
        }

        private void Record(char c)
        {
            _currentLength++;
            if (c == '\n')
            {
                _currentLine++;
                _currentColumn = 1;
                return;
            }
            if (c == '\t')
            {
                _currentColumn += _tabSize;
                return;
            }
            _currentColumn++;
        }

        private void Record(uint count)
        {
            _currentLength += count;
            for (var i = 0; i < count; i++)
            {
                if (_buffer[_position + i] == '\n')
                {
                    _currentLine++;
                    _currentColumn = 1;
                    return;
                }
                if (_buffer[_position + i] == '\t')
                {
                    _currentColumn += _tabSize;
                    return;
                }
                _currentColumn++;
            }
        }


        private void Append(char value)
        {
            if (_textPosition == _textSize)
                DoubleBuffer();
            _textBuffer[_textPosition++] = value;
            _textLength++;
        }

        private void Clear() => _textLength = _textPosition = 0;

        private OfcToken CreateToken(OfcTokenType type) => CreateToken(type, null);

        private OfcToken CreateToken(OfcTokenType type, [CanBeNull] string data)
        {
            return new OfcToken(type, data, _tokenLine, _tokenColumn, _currentLength - _tokenLength);
        }

        private void StartToken()
        {
            if (!_recordPosition) return;
            _tokenLine = _currentLine;
            _tokenColumn = _currentColumn;
            _tokenLength = _currentLength;
        }

        private char Eat()
        {
            var character = _buffer[_position];
            _length--;
            _position++;
            if (_recordPosition) Record(character);
            return character;
        }

        private bool EatUntil(char[] values)
        {
            Clear();
            if (values.Length == 0) return true;
            int z = 0, l = values.Length;
            while (!(_eos && _length == 0))
            {
                if (_length == 0) Fill();
                for (var i = 0; i < _length; i++)
                {
                    var v = _buffer[_position + i];
                    if (v == values[z])
                    {
                        z++;
                        if (z == l)
                        {
                            var r = l - 1;
                            if (r < 0) r = 0;
                            _textLength -= r;
                            Skip(i + 1);
                            return true;
                        }
                    }
                    else z = 0;
                    Append(v);
                }
                Reset();
            }
            return false;
        }

        private void Fill()
        {
            if (_eos) throw new LexerException();
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

        private void Reset()
        {
            if (_recordPosition) Record((uint) _length);
            _length = 0;
        }

        private void Skip()
        {
            if (_recordPosition) Record(_buffer[_position]);
            _position++;
            _length--;
        }

        private void Skip(int amount)
        {
            if (_recordPosition) Record((uint) amount);
            _position += amount;
            _length -= amount;
        }

        private void Relocate()
        {
            if (_length == 0) _position = 0;
            if (_position == 0) return;
            Buffer.BlockCopy(_buffer, _position, _buffer, 0, _length*2);
            _position = 0;
        }

        private void SkipUntil(char value)
        {
            while (!(_eos && _length == 0))
            {
                if (_length == 0) Fill();
                for (var i = 0; i < _length; i++)
                {
                    if (_buffer[_position + i] != value) continue;
                    Skip(i + 1);
                    return;
                }
                Reset();
            }
        }

        private bool SkipUntil(char[] values)
        {
            if (values.Length == 0) return true;
            int z = 0, l = values.Length;
            while (!(_eos && _length == 0))
            {
                if (_length == 0) Fill();
                for (var i = 0; i < _length; i++)
                {
                    if (_buffer[_position + i] == values[z])
                    {
                        z++;
                        if (z != l) continue;
                        Skip(i + 1);
                        return true;
                    }
                    z = 0;
                }
                Reset();
            }
            return false;
        }

        #endregion

        #region Lexer methods

        private static OfcTokenType IsSingleCharToken(char c)
        {
            switch (c)
            {
                case '{':
                    return OfcTokenType.BRACES_OPEN;
                case '}':
                    return OfcTokenType.BRACES_CLOSE;
                case '[':
                    return OfcTokenType.BRACKETS_OPEN;
                case ']':
                    return OfcTokenType.BRACKETS_CLOSE;
                case '(':
                    return OfcTokenType.PARENTHESE_OPEN;
                case ')':
                    return OfcTokenType.PARENTHESE_CLOSE;
                case '<':
                    return OfcTokenType.CHEVRONS_OPEN;
                case '>':
                    return OfcTokenType.CHEVRONS_CLOSE;
                case ';':
                    return OfcTokenType.SEMICOLON;
                case '#':
                    return OfcTokenType.HASHTAG;
                default:
                    return OfcTokenType.NONE;
            }
        }

        private void EatKeyword()
        {
            // while there are still chars available
            while (!(_eos && _length == 0))
            {
                // if there are no more chars in the buffer read new
                if (_length == 0)
                {
                    Fill();
                    continue;
                }
                // go through all items in the buffer, check if the follow the rule
                for (ushort i = 0; i < _length; i++)
                {
                    var c = _buffer[_position + i];
                    if (char.IsWhiteSpace(c) || IsSingleCharToken(c) != OfcTokenType.NONE)
                    {
                        Skip(i);
                        return;
                    }
                    Append(c);
                }
                Reset(); // all the chars in the buffer are valid -> skip them
            }
        }

        private int EatNumber()
        {
            // while there are still characters availble
            var amount = 0;
            while (!(_eos && _length == 0))
            {
                // fill the buffer if it is empty
                if (_length == 0)
                {
                    Fill();
                    continue;
                }
                // loop though all available characters in the buffer
                char c;
                for (ushort i = 0; i < _length; i++, amount++)
                    if ((c = _buffer[_position + i]) >= '0' && c <= '9')
                        Append(c);
                    else
                    {
                        Skip(i); // eat all read chars
                        return amount; // return the amount of read chars
                    }
                Reset(); // eat all chars in the buffer
            }
            return amount; // return the amount of read chars
        }

        private int EatString()
        {
            // while there are still characters availble
            var amount = 0;
            var escape = false;
            while (!(_eos && _length == 0))
            {
                // fill the buffer if it is empty
                if (_length == 0)
                {
                    Fill();
                    continue;
                }
                // loop though all available characters in the buffer
                for (ushort i = 0; i < _length; i++, amount++)
                {
                    var c = _buffer[_position + i];
                    if (escape)
                    {
                        escape = false;
                        switch (c)
                        {
                            case '"':
                                Append('"');
                                break;
                            case '\\':
                                Append('\\');
                                break;
                            case '/':
                                Append('/');
                                break;
                            case 'b':
                                Append('\b');
                                break;
                            case 'f':
                                Append('\f');
                                break;
                            case 'n':
                                Append('\n');
                                break;
                            case 'r':
                                Append('\r');
                                break;
                            case 't':
                                Append('\t');
                                break;
                            default:
                                throw new LexerException();
                        }
                    }
                    else if (c == '\\') escape = true;
                    else if (c == '\n') throw new LexerException();
                    else if (c != '"') Append(c); // if the current char matches the Regex add it to the StringBuilder
                    else
                    {
                        Skip(i); // eat all read chars
                        return amount; // return the amount of read chars
                    }
                }
                Reset(); // eat all chars in the buffer (we have read them)
            }
            return -1; // return the amount of read chars
        }

        internal OfcToken NextToken()
        {
            //Position++;
            while (Needs(1))
            {
                // Save the next character store symbol only and set it to false
                var c = _buffer[_position];
                var onlySymbols = _symbolOnly;
                _symbolOnly = false;

                // If the current character is a whitespace skip it
                if (char.IsWhiteSpace(c))
                {
                    Skip();
                    continue;
                }

                // Check if the current char is the start of a comment
                if (c == '/' && Needs(2))
                {
                    c = _buffer[_position + 1];
                    if (c == '/') // Handling single line comment
                    {
                        SkipUntil('\n');
                        continue;
                    }
                    if (c == '*') // Handling multi line comment
                    {
                        if (!SkipUntil(new[] {'*', '/'})) throw new LexerException();
                        continue;
                    }
                }

                // Save the current position as starting position for the token
                StartToken();

                // Lexing string containers #{.*?#}
                if (!onlySymbols && c == '#' && Needs(2) && _buffer[_position + 1] == '{')
                {
                    Skip(2);
                    if (!EatUntil(new[] {'#', '}'})) throw new LexerException();
                    _symbolOnly = true;
                    return CreateToken(OfcTokenType.STRING, new string(_textBuffer, 0, _textLength));
                }

                // Check if the current char is a token
                var singleTokenType = IsSingleCharToken(c);
                if (singleTokenType != OfcTokenType.NONE)
                {
                    Skip(1);
                    return CreateToken(singleTokenType);
                }

                // If the last character was not a symbol, whitespace or comment and this one isn't too -> BAIL!!!
                if (onlySymbols) throw new LexerException();
                _symbolOnly = true;

                // Lexing Numbers reg. -?(0|[1-9][0-9]*)(\.[0-9]+)?([eE][+-]?[0-9]+)? - for visualization -> json.org
                if (char.IsNumber(c) || c == '-')
                {
                    Clear();
                    if (c == '-') Append(Eat());
                    if (!Needs(1)) throw new LexerException();
                    c = _buffer[_position];
                    if (c == '0') Append(Eat());
                    else if (c >= '1' && c <= '9')
                    {
                        Append(Eat());
                        EatNumber();
                    }
                    else throw new LexerException();
                    if (!Needs(1)) return CreateToken(OfcTokenType.NUMBER, new string(_textBuffer, 0, _textLength));
                    c = _buffer[_position];
                    if (c == '.')
                    {
                        Append(Eat());
                        if (EatNumber() == 0) throw new LexerException();
                        if (!Needs(1)) return CreateToken(OfcTokenType.NUMBER, new string(_textBuffer, 0, _textLength));
                        c = _buffer[_position];
                    }
                    if (c == 'e' || c == 'E')
                    {
                        Append(Eat());
                        if (!Needs(1)) throw new LexerException();
                        c = _buffer[_position];
                        if (c == '+' || c == '-') Append(Eat());
                        if (EatNumber() == 0) throw new LexerException();
                    }

                    return CreateToken(OfcTokenType.NUMBER, new string(_textBuffer, 0, _textLength));
                }

                // Lexing strings reg. ".*?"
                if (c == '"')
                {
                    Clear();
                    Skip(1);
                    if (EatString() == -1) throw new LexerException();
                    Skip(1);
                    return CreateToken(OfcTokenType.STRING, new string(_textBuffer, 0, _textLength));
                }

                // Lexing Keywords reg. [a-zA-Z_$]([a-zA-Z0-9_]|'(')*
                Clear();
                Append(Eat());
                EatKeyword();
                return CreateToken(OfcTokenType.KEYWORD, new string(_textBuffer, 0, _textLength));
            }
            return CreateToken(OfcTokenType.END_OF_STREAM);
        }

        public int Read(OfcToken[] buf, int offset, int count)
        {
            // do some argument checks
            if (buf == null) throw new ArgumentNullException(nameof(buf));
            if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException(offset < 0 ? nameof(offset) : nameof(count));
            if (buf.Length - offset < count) throw new ArgumentException();

            // Loop while we need to read chars
            var t = 0;
            for (var i = 0; i < count; i++, t++)
            {
                var tkn = NextToken(); // read the next token
                if (tkn.Type == OfcTokenType.END_OF_STREAM) // if it is null there has been an error or EOS -> stop reading
                    break;
                buf[i + offset] = tkn; // set the token in the buffer to the read one
            }
            return t; // return the amount of read bytes
        }

        #endregion
    }
}