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

        public ParserException(ParserExceptionCodes code) : this(code, null)
        {
        }

        public ParserException(ParserExceptionCodes code, [CanBeNull] string message) : base(message ?? ResolveExceptionMessage(code))
        {
            Code = code;
        }


        internal static string ResolveExceptionMessage(ParserExceptionCodes code)
        {
            return $"Unkown parser exception. [code: {code.ToString().ToLower()}]";
        }
    }
}