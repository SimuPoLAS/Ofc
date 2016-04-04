namespace OfcCore
{
    public interface IFile
    {
        bool Exists { get; }

        string Extention { get; }

        string Name { get; }

        string Path { get; }
    }
}