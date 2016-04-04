namespace OfcCore
{
    using System.IO;
    using JetBrains.Annotations;

    public interface IConverter<T>
    {
        void Write(Stream output, [CanBeNull] T value);

        T Read(Stream input);

        T FromString(string target);

        string ToString([CanBeNull] T value);
    }
}