namespace OfcCore
{
    using System.IO;

    internal class File : IFile
    {
        private FileInfo _info;


        public bool Exists => _info.Exists;

        public string Extention => _info.Extension;

        internal FileInfo Info => _info;

        public string Name => _info.Name;

        public string Path => _info.FullName;


        public File(string path)
        {
            _info = new FileInfo(path);
        }
    }
}