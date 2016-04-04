namespace Ofc.CommandLine
{
    using System;

    /// <summary>
    ///     Provides methods for adding arguments, command or option to the layer.
    /// </summary>
    internal interface IArgumentLayer
    {
        /// <summary>
        ///     Adds an argument to the current layer, which is set up by the given builder.
        /// </summary>
        /// <param name="builder">Builds the given argument.</param>
        /// <returns>Itself for method chaining.</returns>
        IArgumentLayer AddArgument(Action<IArgumentBuilder> builder);

        /// <summary>
        ///     Adds a command to the current layer, which is set up by the given builder.
        /// </summary>
        /// <param name="builder">Builds the given command.</param>
        /// <returns>Itself for method chaining.</returns>
        IArgumentLayer AddCommand(Action<ICommandBuilder> builder);

        /// <summary>
        ///     Adds a option to the current layer, which is set up by the given builder.
        /// </summary>
        /// <param name="builder">Builds the given option.</param>
        /// <returns>Itself for method chaining.</returns>
        IArgumentLayer AddOption(Action<IOptionBuilder> builder);
    }
}