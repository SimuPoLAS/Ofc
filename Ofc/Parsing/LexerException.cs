namespace Ofc.Parsing
{
    using System;

    internal class LexerException : Exception
    {
        internal LexerExceptionCodes Code { get; set; }


        [Obsolete]
        public LexerException() : this(LexerExceptionCodes.Unknown)
        {
        }

        public LexerException(LexerExceptionCodes code) : this(code, new object[0])
        {
        }

        public LexerException(LexerExceptionCodes code, params object[] args) : base(string.Format(ResolveExceptionMessage(code), args))
        {
            Code = code;
        }


        internal static string ResolveExceptionMessage(LexerExceptionCodes code)
        {
            return $"Unkown lexer exception. [code: {code.ToString().ToLower()}]";
        }
    }
}