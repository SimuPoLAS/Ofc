namespace Ofc.Core
{
    using System;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    ///     Describes an algorithm with an unique Id, a name and a version.
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        ///     Unique Id of the algorithm.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Friendly name of the algorithm.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Version of the algorithm.
        /// </summary>
        Version Version { get; }


        /// <summary>
        ///     Checks if the algorithm supports the specified width and height.
        /// </summary>
        /// <param name="width">The desired list width.</param>
        /// <param name="height">The desired list height (length).</param>
        /// <returns>Returns <c>true</c> if the algorithm supports the specified width and height combination.</returns>
        bool SupportsDimension(int width, int height);
    }

    /// <summary>
    ///     Provides a generic extention to the IAlgorithm interface and definies the Compress/Decompress methods.
    /// </summary>
    /// <typeparam name="T">Type used by the algorithm to represent a number.</typeparam>
    public interface IAlgorithm<T> : IAlgorithm
    {
        /// <summary>
        ///     Starts compression with the specified parameters.
        /// </summary>
        /// <param name="target">File information about the file which is being read.</param>
        /// <param name="configuaration">The configuration used to compress the data.</param>
        /// <param name="output">The output stream where all the compressed data is written to.</param>
        /// <param name="width">The amount of horizontal elements.</param>
        /// <param name="height">The amount of vertical elements (estimate).</param>
        /// <returns>Returns a reporter which is used to supply data to the algorithm.</returns>
        [MustUseReturnValue]
        IReporter<T> Compress([CanBeNull]IFile target, [NotNull]IConfiguaration configuaration, [NotNull]Stream output, int width, int height);

        /// <summary>
        ///     Starts the decompression with the specified parameters.
        /// </summary>
        /// <param name="target">File information about the file which is being read.</param>
        /// <param name="configuaration">The configuration used to read the compressed data.</param>
        /// <param name="input">The input stream where all the compressed data is read from.</param>
        /// <param name="reporter">The reporter which will be used by the algorithm to return the read values.</param>
        /// <param name="width">The width of the list.</param>
        void Decompress([CanBeNull] IFile target, [NotNull] IConfiguaration configuaration, [NotNull] Stream input, [NotNull] IReporter<T> reporter, int width);
    }
}