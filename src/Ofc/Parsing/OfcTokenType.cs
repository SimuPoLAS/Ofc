namespace Ofc.Parsing
{
    /// <summary>
    ///     Represents a Ofc lexer token.
    /// </summary>
    internal enum OfcTokenType
    {
        /// <summary>
        ///     Represents the end of the stream/file.
        /// </summary>
        END_OF_STREAM = -2,

        /// <summary>
        ///     Represents the missingness of a token.
        /// </summary>
        NONE = -1,

        /// <summary>
        ///     Represents an opening brace {.
        /// </summary>
        BRACES_OPEN = 0,

        /// <summary>
        ///     Represents a closing brace }.
        /// </summary>
        BRACES_CLOSE = 1,

        /// <summary>
        ///     Represents an opening bracket [.
        /// </summary>
        BRACKETS_OPEN = 2,

        /// <summary>
        ///     Represents a closing bracket ].
        /// </summary>
        BRACKETS_CLOSE = 3,

        /// <summary>
        ///     Represents an opening parenthesis (.
        /// </summary>
        PARENTHESE_OPEN = 4,

        /// <summary>
        ///     Represents a closing parenthesis ).
        /// </summary>
        PARENTHESE_CLOSE = 5,

        /// <summary>
        ///     Represents an opening chevron &lt;.
        /// </summary>
        CHEVRONS_OPEN = 6,

        /// <summary>
        ///     Represents a closing chevron &gt;.
        /// </summary>
        CHEVRONS_CLOSE = 7,

        /// <summary>
        ///     Represents a semicolon ;.
        /// </summary>
        SEMICOLON = 8,

        /// <summary>
        ///     Represents a hashtag #.
        /// </summary>
        HASHTAG = 9,

        /// <summary>
        ///     Represnts a keyword.
        /// </summary>
        KEYWORD = 10,

        /// <summary>
        ///     Represents a number
        /// </summary>
        NUMBER = 11,

        /// <summary>
        ///     Represents a string.
        /// </summary>
        STRING = 12
    }
}