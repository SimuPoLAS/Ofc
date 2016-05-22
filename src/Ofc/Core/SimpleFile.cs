namespace Ofc.Core
{
    using System.IO;

    /// <summary>
    ///     A simple implementation of the IFile interface.
    /// </summary>
    public class SimpleFile : IFile
    {
        private FileInfo _info;


        /// <summary>
        ///     Returns <c>true</c> if the file exists.
        /// </summary>
        public bool Exists => _info.Exists;

        /// <summary>
        ///     Returns the extention of the file.
        /// </summary>
        /// <remarks>
        ///     The extention contains the dot (.).
        /// </remarks>
        /// <example>
        ///     test.py -> .py
        ///     test ->
        /// </example>
        public string Extention => _info.Extension;

        /// <summary>
        ///     Returns the FileInfo object of the file.
        /// </summary>
        internal FileInfo Info => _info;

        /// <summary>
        ///     Returns the Name of the object.
        /// </summary>
        /// <remarks>
        ///     The name contains the extention.
        /// </remarks>
        public string Name => _info.Name;

        /// <summary>
        ///     Returns the full path of the file.
        /// </summary>
        public string Path => _info.FullName;


        /// <summary>
        ///     Sets up the class with the specified path.
        /// </summary>
        /// <param name="path">Path to the target file.</param>
        public SimpleFile(string path)
        {
            _info = new FileInfo(path);
        }
    }
}