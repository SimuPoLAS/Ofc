namespace Ofc.CommandLine
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides methods for building a command.
    /// </summary>
    internal interface ICommandBuilder
    {
        /// <summary>
        ///     Sets the description of the command.
        /// </summary>
        /// <param name="description">Command description.</param>
        /// <returns>Itself for method chaining.</returns>
        ICommandBuilder Description([CanBeNull] string description);

        /// <summary>
        ///     Hides the command from all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        ICommandBuilder Hide();

        /// <summary>
        ///     Sets the name of the command.
        /// </summary>
        /// <param name="name">Name of the command.</param>
        /// <returns>Itself for method chaining.</returns>
        /// <remarks>
        ///     The <paramref name="name" /> may only contain letters.
        /// </remarks>
        ICommandBuilder Name([CanBeNull] string name);

        /// <summary>
        ///     Shows the command in all sections.
        /// </summary>
        /// <returns>Itself for method chaining.</returns>
        ICommandBuilder Show();

        /// <summary>
        ///     Sets the visibility of the command.
        /// </summary>
        /// <param name="visiblility">Target visibility.</param>
        /// <returns>Itself for method chaining.</returns>
        ICommandBuilder Visibility(ArgumentVisiblility visiblility);
    }
}