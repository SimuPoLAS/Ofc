namespace Ofc.CommandLine
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides methods for building a argument.
    /// </summary>
    internal interface IArgumentBuilder
    {
        /// <summary>
        ///     Sets the description of the argument.
        /// </summary>
        /// <param name="description">Argument description.</param>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Description([CanBeNull] string description);

        /// <summary>
        ///     Hides the argument from all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Hide();

        /// <summary>
        ///     Marks the current argument as optional.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Optional();

        /// <summary>
        ///     Marks the current argument as required.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Required();

        /// <summary>
        ///     Sets the name of the argument.
        /// </summary>
        /// <param name="name">Name of the argument.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <returns>Itself for method chaining.</returns>
        /// <remarks>
        ///     The <paramref name="name" /> may only contain letters.
        /// </remarks>
        IArgumentBuilder SetName([CanBeNull] string name);

        /// <summary>
        ///     Shows the argument in all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Show();

        /// <summary>
        ///     Sets the visibility of the argument.
        /// </summary>
        /// <param name="visiblility">Target visibility.</param>
        /// <returns>Itself for method chaining.</returns>
        IArgumentBuilder Visibility(ArgumentVisiblility visiblility);
    }
}