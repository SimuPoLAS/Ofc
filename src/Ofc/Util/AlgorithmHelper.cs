namespace Ofc.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Actions;
    using Algorithm.Blocky.Integration;
    using Algorithm.Integration;
    using Algorithm.Zetty;
    using Converters;
    using Core;
    using JetBrains.Annotations;

    // todo improve speed by caching the rerouting method infos / delegates

    internal static class AlgorithmHelper
    {
        private static readonly Dictionary<string, AlgorithmContainer> RegisteredAlgorithms = new Dictionary<string, AlgorithmContainer>
        {
            ["blocky"] = new AlgorithmContainer<OfcNumber>(e => new BlockyAlgorithm(), new CompressionDataConverter()),
            ["zetty"] = new AlgorithmContainer<string>(e => new ZettyAlgorithm(e), NoDataConverter.Instance)
        };

        internal const string DefaultAlgorithm = "zetty";

        private static AlgorithmContainer DefaultContainer => GetDataFromName(DefaultAlgorithm);

        internal static IEnumerable<string> RegisteredAlgorithmByName => RegisteredAlgorithms.Keys;


        public static bool IsValidAlgorithm(string algorithm)
        {
            return RegisteredAlgorithms.Keys.Contains(algorithm);
        }

        public static void AddCompressFileActionWithAlgorithm(this OfcActionManager manager, [CanBeNull] string algorithmName, IConfiguaration configuaration, string source, string destination)
        {
            var container = GetDataFromName(algorithmName);
            var algorithm = container.Creator(configuaration);
            var converter = container.Converter;

            var type = typeof(ActionUtils);
            var method = type.GetMethod(nameof(ActionUtils.AddCompressFileAction), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("Could not link to the internal method");
            var genericMethod = method.MakeGenericMethod(container.AlgorithmType);
            genericMethod.Invoke(null, new object[] {manager, algorithm, converter, configuaration, source, destination});
        }

        public static void AddCompressDirectoryActionWithAlgorithm(this OfcActionManager manager, [CanBeNull] string algorithmName, IConfiguaration configuaration, string baseInputDirectory, string baseOutputDirectory, bool recursive)
        {
            var container = GetDataFromName(algorithmName);
            var algorithm = container.Creator(configuaration);
            var converter = container.Converter;

            var type = typeof(ActionUtils);
            var method = type.GetMethod(nameof(ActionUtils.AddCompressDirectoryAction), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("Could not link to the internal method");
            var genericMethod = method.MakeGenericMethod(container.AlgorithmType);
            genericMethod.Invoke(null, new object[] { manager, algorithm, converter, configuaration, baseInputDirectory, baseOutputDirectory, recursive });
        }

        public static void AddDecompressFileActionWithAlgorithm(this OfcActionManager manager, [CanBeNull] string algorithmName, IConfiguaration configuaration, string metaSource, string dataSource, string destination)
        {
            var container = GetDataFromName(algorithmName);
            var algorithm = container.Creator(configuaration);
            var converter = container.Converter;

            var type = typeof(ActionUtils);
            var method = type.GetMethod(nameof(ActionUtils.AddDecompressFileAction), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("Could not link to the internal method");
            var genericMethod = method.MakeGenericMethod(container.AlgorithmType);
            genericMethod.Invoke(null, new object[] { manager, algorithm, converter, configuaration, metaSource, dataSource, destination });
        }

        public static void AddDecompressDirectoryActionWithAlgorithm(this OfcActionManager manager, [CanBeNull] string algorithmName, IConfiguaration configuaration, string baseInputDirectory, string baseOutputDirectory, bool recursive)
        {
            var container = GetDataFromName(algorithmName);
            var algorithm = container.Creator(configuaration);
            var converter = container.Converter;

            var type = typeof(ActionUtils);
            var method = type.GetMethod(nameof(ActionUtils.AddDecompressDirectoryAction), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("Could not link to the internal method");
            var genericMethod = method.MakeGenericMethod(container.AlgorithmType);
            genericMethod.Invoke(null, new object[] { manager, algorithm, converter, configuaration, baseInputDirectory, baseOutputDirectory, recursive });
        }

        private static AlgorithmContainer GetDataFromName(string name)
        {
            if (name == null) return DefaultContainer;
            AlgorithmContainer container;
            if (!RegisteredAlgorithms.TryGetValue(name, out container)) throw new NotSupportedException();
            return container;
        }


        private abstract class AlgorithmContainer
        {
            internal abstract Type AlgorithmType { get; }

            internal Func<IConfiguaration, IAlgorithm> Creator { get; }

            public IConverter Converter { get; }


            protected AlgorithmContainer(Func<IConfiguaration, IAlgorithm> creator, IConverter converter)
            {
                Creator = creator;
                Converter = converter;
            }
        }

        private class AlgorithmContainer<T> : AlgorithmContainer
        {
            internal override Type AlgorithmType => typeof(T);


            public AlgorithmContainer(Func<IConfiguaration, IAlgorithm<T>> creator, IConverter<T> converter) : base(creator, converter)
            {
            }
        }
    }
}