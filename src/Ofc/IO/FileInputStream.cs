namespace Ofc.IO
{
    using System;
    using System.IO;

    internal class FileInputStream : IInputStream<char>, IDisposable
    {
        private TextReader _reader;

        public FileInputStream(string target)
        {
            if (File.Exists(target))
            {
                FileStream stream = null;
                try
                {
                    stream = new FileStream(target, FileMode.Open);
                    _reader = new StreamReader(stream);
                }
                catch (Exception)
                {
                    stream?.Dispose();
                    _reader = null;
                }
            }
            else _reader = null;
        }

        public FileInputStream(TextReader reader)
        {
            _reader = reader;
        }

        public int Read(char[] buffer, int offset, int count)
        {
            return _reader?.Read(buffer, offset, count) ?? 0;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}