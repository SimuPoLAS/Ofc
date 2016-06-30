namespace Ofc.CLI
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides methods and properties to represent a result which is returned by an argument parser.
    /// </summary>
    /// <typeparam name="T">Type which will be used to represent a layer identification.</typeparam>
    internal interface IArgumentResult<out T>
    {
        /// <summary>
        ///     If the argument parsing was a success.
        /// </summary>
        bool Success { get; }

        /// <summary>
        ///     Id of the layer, which was detected by the parser.
        /// </summary>
        /// <remarks>
        ///     This value is only representing when <seealso cref="Success" /> is <c>true</c>.
        /// </remarks>
        T LayerId { get; }


        /// <summary>
        ///     Returns a specific argument.
        /// </summary>
        /// <param name="name">Name of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /></returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     This method will throw an exception if the argument is not registered or is not set.
        /// </remarks>
        object GetArgument(string name);

        TV GetArgument<TV>(string name);

        /// <summary>
        /// Returns an argument at a specific position. 
        /// </summary>
        /// <param name="position">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string"/>.</returns>
        /// <exception cref="ArgumentException">There is no argument with the specified position.</exception>
        object GetArgument(int position);

        TV GetArgument<TV>(int position);

        /// <summary>
        ///     Returns a specifc argument or <c>null</c> if the argument is not set.
        /// </summary>
        /// <param name="name">Name of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /> or <c>null</c> if the argument is not set.</returns>
        /// <remarks>
        ///     This method will throw an exception if the argument is not registered.
        /// </remarks>
        [CanBeNull]
        object GetArgumentOrNull(string name);

        TV GetArgumentOrNull<TV>(string name) where TV : class;

        /// <summary>
        /// Returns an argument at a specific position or <c>null</c> if the argument could not be found.
        /// </summary>
        /// <param name="index">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string"/> or <c>null</c> if the argument could not be found.</returns>
        [CanBeNull]
        object GetArgumentOrNull(int index);

        TV GetArgumentOrNull<TV>(int position) where TV : class;

        /// <summary>
        ///     Returns a specific flag.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <returns>The flag.</returns>
        /// <remarks>
        ///     This method will throw an exception if the flag is not registered.
        /// </remarks>
        bool GetFlag(string name);

        /// <summary>
        ///     Returns a specific option.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <returns>The option as <seealso cref="string" />.</returns>
        /// <remarks>
        ///     This method will throw an exception if the option is not registered or set.
        /// </remarks>
        object GetOption(string name);

        TV GetOption<TV>(string name);

        /// <summary>
        ///     Returns a specifc option or <c>null</c> if the option is not set.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <returns>The option as <seealso cref="string" /> or <c>null</c> if the option is not set.</returns>
        /// <remarks>
        ///     This method will throw an exception if the option is not registered.
        /// </remarks>
        [CanBeNull]
        object GetOptionOrNull(string name);

        TV GetOptionOrNull<TV>(string name) where TV : class;

        /// <summary>
        /// Returns an argument at a specific position.
        /// </summary>
        /// <param name="index">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /> or <c>null</c> if the argument could not be found.</returns>
        string this[int index] { get; }

        /// <summary>
        /// Returns an argument or an option depending on the name.
        /// </summary>
        /// <param name="name">Name of the argument/option.</param>
        /// <returns>Argument/option value as <seealso cref="string" /> or <c>null</c> if the argument/option could not be found.</returns>
        string this[string name] { get; }

        /// <summary>
        /// Returns <c>true</c> if the specified flags is set.
        /// </summary>
        /// <param name="flag">Name of the flag.</param>
        /// <returns><c>true</c> if the specified flag is set <c>false</c> otherwise.</returns>
        bool this[char flag] { get; }
    }
}