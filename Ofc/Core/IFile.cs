namespace Ofc.Core
{
    /// <summary>
    ///     Represents a file.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        ///     Returns <c>true</c> if the file exists.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        ///     Returns the extention of the file.
        /// </summary>
        string Extention { get; }

        /// <summary>
        ///     Returns the name of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Returns the full path of the file.
        /// </summary>
        string Path { get; }
    }
}