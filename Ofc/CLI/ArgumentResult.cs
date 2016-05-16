namespace Ofc.CLI
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    internal class ArgumentResult<T> : IArgumentResult<T>
    {
        [ItemNotNull] private List<string> _arguments;
        [ItemNotNull] private List<string> _argumentNames;
        private Dictionary<string, bool> _flags;
        private Dictionary<string, string> _options;


        /// <summary>
        ///     If the argument parsing was a success.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        ///     Id of the layer, which was detected by the parser.
        /// </summary>
        /// <remarks>
        ///     This value is only representing when <seealso cref="Success" /> is <c>true</c>.
        /// </remarks>
        public T LayerId { get; }


        public ArgumentResult()
        {
            Success = false;
            LayerId = default(T);
        }

        public ArgumentResult(T layerId, [ItemNotNull] List<string> arguments, List<string> argumentNames, Dictionary<string, bool> flags, Dictionary<string, string> options)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (argumentNames == null) throw new ArgumentNullException(nameof(argumentNames));
            if (flags == null) throw new ArgumentNullException(nameof(flags));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (arguments.Count > argumentNames.Count) throw new ArgumentException("The argument count can not be greater than the count of the argument names.");
            Success = true;
            LayerId = layerId;
            _arguments = arguments;
            _argumentNames = argumentNames;
            _flags = flags;
            _options = options;
        }


        /// <summary>
        ///     Returns a specific argument.
        /// </summary>
        /// <param name="name">Name of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /></returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     This method will throw an exception if the argument is not registered or is not set.
        /// </remarks>
        public string GetArgument(string name)
        {
            var index = _argumentNames.IndexOf(name);
            if (index == -1) throw new ArgumentException("Could not find the target argument.");
            return GetArgument(index);
        }

        /// <summary>
        ///     Returns an argument at a specific position.
        /// </summary>
        /// <param name="position">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" />.</returns>
        /// <exception cref="ArgumentException">There is no argument with the specified position.</exception>
        public string GetArgument(int position)
        {
            if (position < 0 || position >= _arguments.Count) throw new ArgumentException("Could not find the target argument.");
            return _arguments[position];
        }

        /// <summary>
        ///     Returns a specifc argument or <c>null</c> if the argument is not set.
        /// </summary>
        /// <param name="name">Name of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /> or <c>null</c> if the argument is not set.</returns>
        /// <remarks>
        ///     This method will throw an exception if the argument is not registered.
        /// </remarks>
        [CanBeNull]
        public string GetArgumentOrNull(string name)
        {
            var index = _argumentNames.IndexOf(name);
            return index == -1 ? null : GetArgumentOrNull(index);
        }

        /// <summary>
        ///     Returns an argument at a specific position or <c>null</c> if the argument could not be found.
        /// </summary>
        /// <param name="index">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /> or <c>null</c> if the argument could not be found.</returns>
        [CanBeNull]
        public string GetArgumentOrNull(int index)
        {
            if (index < 0 || index >= _arguments.Count) return null;
            return _arguments[index];
        }

        /// <summary>
        ///     Returns a specific flag.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <returns>The flag.</returns>
        /// <remarks>
        ///     This method will throw an exception if the flag is not registered.
        /// </remarks>
        public bool GetFlag(string name)
        {
            bool item;
            return _flags.TryGetValue(name, out item) && item;
        }

        /// <summary>
        ///     Returns a specific option.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <returns>The option as <seealso cref="string" />.</returns>
        /// <remarks>
        ///     This method will throw an exception if the option is not registered or set.
        /// </remarks>
        public string GetOption(string name)
        {
            string item;
            if (!_options.TryGetValue(name, out item) || item == null) throw new ArgumentException("Could not find the target argument.");
            return item;
        }

        /// <summary>
        ///     Returns a specifc option or <c>null</c> if the option is not set.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <returns>The option as <seealso cref="string" /> or <c>null</c> if the option is not set.</returns>
        /// <remarks>
        ///     This method will throw an exception if the option is not registered.
        /// </remarks>
        public string GetOptionOrNull(string name)
        {
            string item;
            return !_options.TryGetValue(name, out item) ? null : item;
        }


        /// <summary>
        ///     Returns an argument at a specific position.
        /// </summary>
        /// <param name="index">Position of the argument.</param>
        /// <returns>Argument value as <seealso cref="string" /> or <c>null</c> if the argument could not be found.</returns>
        [CanBeNull]
        public string this[int index] => GetArgumentOrNull(index);

        /// <summary>
        ///     Returns an argument or an option depending on the name.
        /// </summary>
        /// <param name="name">Name of the argument/option.</param>
        /// <returns>Argument/option value as <seealso cref="string" /> or <c>null</c> if the argument/option could not be found.</returns>
        [CanBeNull]
        public string this[string name]
        {
            get
            {
                if (name.StartsWith("--")) return GetOptionOrNull(name.Substring(2));
                return name.StartsWith("-") ? GetOptionOrNull(name.Substring(1)) : GetArgumentOrNull(name);
            }
        }

        /// <summary>
        ///     Returns <c>true</c> if the specified flags is set.
        /// </summary>
        /// <param name="flag">Name of the flag.</param>
        /// <returns><c>true</c> if the specified flag is set <c>false</c> otherwise.</returns>
        public bool this[char flag] => GetFlag(flag.ToString());
    }
}