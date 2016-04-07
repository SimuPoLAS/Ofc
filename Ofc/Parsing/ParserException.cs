namespace Ofc.Parsing
{
    using System;

    /// <summary>
    ///     Represents an exception thrown by the parser.
    /// </summary>
    internal class ParserException : Exception
    {
        /// <summary>
        ///     The internal code of the exception.
        /// </summary>
        internal ParserExceptionCodes Code { get; set; }


        [Obsolete]
        public ParserException() : this(ParserExceptionCodes.Unknown)
        {
        }

        /// <summary>
        ///     Sets up the exception with the specified exception code.
        /// </summary>
        /// <param name="code">Internal code of the exception.</param>
        public ParserException(ParserExceptionCodes code) : this(code, new object[0])
        {
        }

        /// <summary>
        ///     Sets up the exception with the specified exception code and the specified formatting arguments.
        /// </summary>
        /// <param name="code">Internal code of the exception.</param>
        /// <param name="args">Formatting arguments used when converting the exception into a string.</param>
        public ParserException(ParserExceptionCodes code, params object[] args) : base(string.Format(ResolveExceptionMessage(code), args))
        {
            Code = code;
        }


        /// <summary>
        ///     Converts the specified exception code into a formatted string.
        /// </summary>
        /// <param name="code">Internal exception code.</param>
        /// <returns>Textual representation of the specified exception code.</returns>
        internal static string ResolveExceptionMessage(ParserExceptionCodes code)
        {
            return $"Unkown parser exception. [code: {code.ToString().ToLower()}]";
        }
    }
}