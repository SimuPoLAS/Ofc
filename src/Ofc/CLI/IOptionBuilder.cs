namespace Ofc.CLI
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides methods for building an option.
    /// </summary>
    internal interface IOptionBuilder
    {
        /// <summary>
        ///     Adds an argument to the option.
        /// </summary>
        /// <param name="builder">The builder for the option argument.</param>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Argument(Action<IArgumentBuilder> builder);

        /// <summary>
        ///     Sets the description of the option.
        /// </summary>
        /// <param name="description">The option description.</param>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Description([CanBeNull] string description);

        /// <summary>
        ///     Hides the option from all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Hide();

        /// <summary>
        ///     Marks the option as optional.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Optional();

        /// <summary>
        ///     Marks the option as required.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Required();

        /// <summary>
        ///     Sets the long name of the option (e.g. --help).
        /// </summary>
        /// <param name="longName">Long name of the option</param>
        /// <returns>Itself for method chaining.</returns>
        /// <remarks>
        ///     The <paramref name="longName" /> may only consist of letters.
        /// </remarks>
        IOptionBuilder SetLongName([CanBeNull] string longName);

        /// <summary>
        ///     Sets the short name of the option (e.g. -h).
        /// </summary>
        /// <param name="shortName">Short name of the option.</param>
        /// <returns>Itself for method chaining.</returns>
        /// <remarks>
        /// The <paramref name="shortName"/> may only consist of letters.
        /// </remarks>
        IOptionBuilder SetShortName(char shortName);

        /// <summary>
        ///     Shows the option in all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Show();

        /// <summary>
        ///     Sets the visibility of the option.
        /// </summary>
        /// <param name="visiblility">Target visibility.</param>
        /// <returns>Itself for method chaining.</returns>
        IOptionBuilder Visibility(ArgumentVisiblility visiblility);
    }
}