namespace Ofc.CLI
{
    using System;
    using JetBrains.Annotations;

    internal static class ArgumentExtentions
    {
        [UsedImplicitly]
        internal static IArgumentLayer Argument(this IArgumentLayer target, string name) => target.AddArgument(e => e.SetName(name));

        [UsedImplicitly]
        internal static IArgumentLayer Argument(this IArgumentLayer target, string name, string description) => target.AddArgument(e => e.SetName(name).Description(description));

        [UsedImplicitly]
        internal static IArgumentLayer Argument(this IArgumentLayer target, string name, bool optional) => optional ? target.AddArgument(e => e.SetName(name).Optional()) : target.Argument(name);

        [UsedImplicitly]
        internal static IArgumentLayer Argument(this IArgumentLayer target, string name, string description, bool optional) => optional ? target.AddArgument(e => e.SetName(name).Description(description).Optional()) : target.Argument(name, description);

        [UsedImplicitly]
        internal static IArgumentLayer Command(this IArgumentLayer target, string name) => target.AddCommand(e => e.Name(name));

        [UsedImplicitly]
        internal static IArgumentLayer Command(this IArgumentLayer target, string name, string description) => target.AddCommand(e => e.Name(name).Description(description));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, char shortName) => target.AddOption(e => e.SetShortName(shortName));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, string longName) => target.AddOption(e => e.SetLongName(longName));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, char shortName, string longName) => target.AddOption(e => e.SetShortName(shortName).SetLongName(longName));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, char shortName, Action<IArgumentBuilder> builder) => target.AddOption(e => e.SetShortName(shortName).Argument(builder));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, string longName, Action<IArgumentBuilder> builder) => target.AddOption(e => e.SetLongName(longName).Argument(builder));

        [UsedImplicitly]
        internal static IArgumentLayer Option(this IArgumentLayer target, char shortName, string longName, Action<IArgumentBuilder> builder) => target.AddOption(e => e.SetShortName(shortName).SetLongName(longName).Argument(builder));
    }
}