namespace Ofc.Parsing
{
    using System;

    internal class LexerException : Exception
    {
        internal LexerExceptionCodes Code { get; set; }

        internal uint ExceptionLine { get; set; }

        internal uint ExceptionColumn { get; set; }

        internal bool HasMoreInformation { get; set; }

        internal uint ReferenceLine { get; set; }

        internal uint ReferenceColumn { get; set; }


        [Obsolete]
        public LexerException() : this(LexerExceptionCodes.Unknown)
        {
        }

        public LexerException(LexerExceptionCodes code) : this(code, new object[0])
        {
        }

        public LexerException(LexerExceptionCodes code, params object[] args) : base(string.Format(ResolveExceptionMessage(code, args.Length), args))
        {
            Code = code;
        }


        internal static string ResolveExceptionMessage(LexerExceptionCodes code, int length)
        {
            switch (code)
            {
                case LexerExceptionCodes.InvalidEscapeSequence:
                    return "Invalid escape sequence '{0}'";
                case LexerExceptionCodes.Unknown:
                    return "Unknown lexer error.";
                case LexerExceptionCodes.UnexpectedEndOfStream:
                    return "Unexpected end of stream.";
                case LexerExceptionCodes.UnexpectedSymbol:
                    return length == 0 ? "Unexpected symbol." : "Unexpected symbol '{0}'.";
                case LexerExceptionCodes.UnterminatedBlockComment:
                    return "Unterminated block comment.";
                case LexerExceptionCodes.UnterminatedStringLiteral:
                    return "Unterminated string literal.";
                case LexerExceptionCodes.UnterminatedStringContainer:
                    return "Unterminated string container.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }


        public override string ToString() => $"code: {Code} {(HasMoreInformation ? $"@ line: {ExceptionLine}; column: {ExceptionColumn}; {(ExceptionLine != ReferenceLine && ExceptionColumn != ReferenceColumn ? $"ref line: {ReferenceLine}; ref column: {ReferenceColumn};" : string.Empty)}" : string.Empty)}\n{base.ToString()}";
    }
}