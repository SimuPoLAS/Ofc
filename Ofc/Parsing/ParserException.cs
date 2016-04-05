namespace Ofc.Parsing
{
    using System;
    using JetBrains.Annotations;

    internal class ParserException : Exception
    {
        internal ParserExceptionCodes Code { get; set; }


        [Obsolete]
        public ParserException() : this(ParserExceptionCodes.Unknown)
        {
        }

        public ParserException(ParserExceptionCodes code) : this(code, new object[0])
        {
        }

        public ParserException(ParserExceptionCodes code, params object[] args) : base(string.Format(ResolveExceptionMessage(code), args))
        {
            Code = code;
        }


        internal static string ResolveExceptionMessage(ParserExceptionCodes code)
        {
            return $"Unkown parser exception. [code: {code.ToString().ToLower()}]";
        }
    }
}