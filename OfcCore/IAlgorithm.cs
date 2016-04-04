using JetBrains.Annotations;

namespace OfcCore
{
    using System;
    using System.IO;

    public interface IAlgorithm
    {
        string Id { get; }

        string Name { get; }

        Version Version { get; }

        bool SupportsDimension(int width, int height);
    }

    public interface IAlgorithm<T> : IAlgorithm
    {
        [MustUseReturnValue]
        IReporter<T> Compress(IFile target, IConfiguaration configuaration, Stream output, int width, int height);

        void Decompress(IFile target, IConfiguaration configuaration, Stream input, IReporter<T> reporter);
    }
}