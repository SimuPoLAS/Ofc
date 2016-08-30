namespace Ofc.Core
{
    using System.IO;
    using JetBrains.Annotations;

    public interface IConverter
    {
    }

    /// <summary>
    ///   Provides methods to convert between string and the specified type as well as writing to a stream.
    /// </summary>
    /// <typeparam name="T">Type between which will be converted.</typeparam>
    public interface IConverter<T> : IConverter
    {
        /// <summary>
        ///   Writes the specified value into the specified stream.
        /// </summary>
        /// <param name="output">The stream where the value will be written into.</param>
        /// <param name="value">The value which will be written into the stream.</param>
        void Write(Stream output, [CanBeNull] T value);

        /// <summary>
        ///   Reads a value from the specified stream.
        /// </summary>
        /// <param name="input">The stream where the value will be read from.</param>
        /// <returns>The read value.</returns>
        T Read(Stream input);

        /// <summary>
        ///   Converts the specified string into a value of type
        ///   <typeparam name="T"></typeparam>
        ///   .
        /// </summary>
        /// <param name="target">String value which will be converted.</param>
        /// <returns>The converted object of type
        ///   <typeparam name="T">.</typeparam>
        /// </returns>
        T FromString(string target);

        /// <summary>
        ///   Converts the specified object into a string.
        /// </summary>
        /// <param name="value">The value which should be converted into a string.</param>
        /// <returns>The converted string value.</returns>
        string ToString([CanBeNull] T value);
    }
}