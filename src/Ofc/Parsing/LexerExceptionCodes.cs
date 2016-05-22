namespace Ofc.Parsing
{
    /// <summary>
    ///     Represents all exception codes that can be thrown from the lexer.
    /// </summary>
    internal enum LexerExceptionCodes
    {
        /// <summary>
        ///     Represents the exception where the lexer could not provide more information about the exception.
        /// </summary>
        Unknown,

        /// <summary>
        ///     Represents the exception where the lexer encountered an invalid escape sequence.
        /// </summary>
        InvalidEscapeSequence,

        /// <summary>
        ///     Represents the exception where the lexer unexpectedly encountered the end of the stream.
        /// </summary>
        UnexpectedEndOfStream,

        /// <summary>
        ///     Represents the exception where the lexer unexpectedly encountered an in this context invalid character.
        /// </summary>
        UnexpectedSymbol,

        /// <summary>
        ///     Represents the exception where the lexer encountered an unterminated block comment.
        /// </summary>
        UnterminatedBlockComment,

        /// <summary>
        ///     Represents the exception where the lexer encountered an unterminated string literal.
        /// </summary>
        UnterminatedStringLiteral,

        /// <summary>
        ///     Represents the exception where the lexer encountered an unterminated string container.
        /// </summary>
        UnterminatedStringContainer
    }
}