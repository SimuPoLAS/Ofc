namespace Ofc.LZMA
{
    using System;

    /// <summary>
    /// The exception that is thrown when the value of an argument is outside the allowable range.
    /// </summary>
    internal class InvalidParamException : Exception
    {
        public InvalidParamException(): base("Invalid Parameter") { }
    }
}