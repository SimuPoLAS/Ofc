namespace Ofc.Util.Converters
{
    using System;
    using System.IO;
    using Ofc.Core;

    internal class NoDataConverter : IConverter<string>
    {
        /// <summary>
        ///     Writes the specified value into the specified stream.
        /// </summary>
        /// <param name="output">The stream where the value will be written into.</param>
        /// <param name="value">The value which will be written into the stream.</param>
        public void Write(Stream output, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Reads a value from the specified stream.
        /// </summary>
        /// <param name="input">The stream where the value will be read from.</param>
        /// <returns>The read value.</returns>
        public string Read(Stream input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Converts the specified string into a value of type
        ///     <typeparam name="T"></typeparam>
        ///     .
        /// </summary>
        /// <param name="target">String value which will be converted.</param>
        /// <returns>The converted object of type
        ///     <typeparam name="T">.</typeparam>
        /// </returns>
        public string FromString(string target) => target;

        /// <summary>
        ///     Converts the specified object into a string.
        /// </summary>
        /// <param name="value">The value which should be converted into a string.</param>
        /// <returns>The converted string value.</returns>
        public string ToString(string value) => value;
    }
}