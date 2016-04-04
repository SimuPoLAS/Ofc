namespace Ofc.Parsing
{
    using System;
    using JetBrains.Annotations;

    internal class LexerException : Exception
    {
        internal LexerExceptionCodes Code { get; set; }


        [Obsolete]
        public LexerException() : this(LexerExceptionCodes.Unknown)
        {
        }

        public LexerException(LexerExceptionCodes code) : this(code, null)
        {
        }

        public LexerException(LexerExceptionCodes code, [CanBeNull] string message) : base(message ?? ResolveExceptionMessage(code))
        {
            Code = code;
        }


        internal static string ResolveExceptionMessage(LexerExceptionCodes code)
        {
            return $"Unkown lexer exception. [code: {code.ToString().ToLower()}]";
        }
    }
}