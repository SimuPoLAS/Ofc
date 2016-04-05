namespace Ofc.Parsing
{
    internal enum LexerExceptionCodes
    {
        InvalidEscapeSequence,
        Unknown,
        UnexpectedEndOfStream,
        UnexpectedSymbol,
        UnterminatedBlockComment,
        UnterminatedStringLiteral,
        UnterminatedStringContainer,
    }
}