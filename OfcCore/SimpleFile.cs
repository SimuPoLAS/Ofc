namespace OfcCore
{
    using System.IO;

    internal class SimpleFile : IFile
    {
        private FileInfo _info;


        public bool Exists => _info.Exists;

        public string Extention => _info.Extension;

        internal FileInfo Info => _info;

        public string Name => _info.Name;

        public string Path => _info.FullName;


        public SimpleFile(string path)
        {
            _info = new FileInfo(path);
        }
    }
}