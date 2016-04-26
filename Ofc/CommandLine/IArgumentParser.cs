namespace Ofc.CommandLine
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides methods for a argument parser.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IArgumentParser<T>
    {
        /// <summary>
        ///     Description of the program.
        /// </summary>
        [CanBeNull]
        string Description { get; set; }

        /// <summary>
        ///     Name of the program.
        /// </summary>
        [CanBeNull]
        string Name { get; set; }


        /// <summary>
        ///     Generates a help string from all registered layers.
        /// </summary>
        /// <returns>The generated help.</returns>
        string GenerateHelp();

        /// <summary>
        ///     Adds a new layer to the parser and returns it.
        /// </summary>
        /// <param name="id">Id of the layer. Must be unique.</param>
        /// <returns>The generated layer.</returns>
        IArgumentLayer NewLayer(T id);

        /// <summary>
        ///     Adds a new option to the parser and returns a builder for it.
        /// </summary>
        /// <returns>The generated option.</returns>
        IOptionBuilder NewOption();

        /// <summary>
        ///     Parses the given input string with the registered layers and returns a parse result.
        /// </summary>
        /// <param name="input">A string, which is formatted as if it would be passed over with the command line.</param>
        /// <returns>A result containing all parsed elements as well as information about the parsing result (e.g. success).</returns>
        IArgumentResult<T> Parse(string input);

        /// <summary>
        ///     Parses the given arguments with the registered layers and returns a parse result.
        /// </summary>
        /// <param name="arguments">Arguments which should be parsed.</param>
        /// <returns>A result containing all parsed elements as well as information about the parsing result (e.g. success).</returns>
        IArgumentResult<T> Parse(IEnumerable<string> arguments);
    }
}