namespace Ofc.LZMA
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error in input stream occurs during decoding.
    /// </summary>
    internal class DataErrorException : Exception
    {
        public DataErrorException(): base("Data Error") { }
    }
}