// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable StaticMemberInGenericType

namespace Ofc.CLI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Ofc.CLI.Validators;

    /// <summary>
    ///     Provides methods for parsing command line arguments.
    /// </summary>
    /// <typeparam name="T">Type which is used to identify a layer.</typeparam>
    /// <remarks>
    ///     Friendly reminder: optional option do not work
    /// </remarks>
    internal class ArgumentParser<T> : IArgumentParser<T>
    {
        private StringBuilder _builder = new StringBuilder();
        private List<Option> _generalOptions = new List<Option>();
        private Dictionary<T, Layer> _layers = new Dictionary<T, Layer>();
        private IValidatorContainer _container = new ValidatorContainer();


        public ArgumentParser()
        {
            _container.SetValidator<byte>(new FuncValidator((string v, ref object d) =>
            {
                byte value;
                if (!byte.TryParse(v, out value)) return false;
                d = value;
                return true;
            }));
        }


        /// <summary>
        ///     Description of the program which will be displayed in the help section.
        /// </summary>
        /// <remarks>
        ///     If the description is <c>null</c> or empty or contains only whitespaces it will not be shown in the help section.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        ///     Name of the exe. It will be displayed in the help section.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        ///     Generates a help string.
        /// </summary>
        /// <returns>The generated help.</returns>
        public string GenerateHelp()
        {
            // Clear the builder
            _builder.Length = 0;

            // If there is a program description add it with a line spacing
            if (!string.IsNullOrWhiteSpace(Description))
            {
                _builder.AppendLine(Description);
                _builder.AppendLine();
            }

            // If there are layers to describe generate a usage section
            if (_layers.Count > 0)
            {
                // Header
                _builder.AppendLine("Usage:");
                // Go through all layers
                foreach (var layer in _layers)
                {
                    // Write the layer description with a little indent
                    _builder.Append("  ");
                    if (Name != null) _builder.Append(Name + " ");
                    layer.Value.GenerateHelp(_builder);
                    _builder.AppendLine();
                }
                _builder.AppendLine();
            }

            // Methods
            // bug commands which have an argument between them will be displayed in a wrong ways (e.g. ofc.exe check command <test> if <something> - displays: check command if)
            var commands = _layers.Values.Select(e => e.Arguments.Where(f => f.Type == GroupType.Command).Cast<Command>().Where(f => f.CommandVisibility.HasFlag(ArgumentVisiblility.Description))).Where(e => e.Count() != 0);
            if (commands.Any())
            {
                // Write the header
                _builder.AppendLine("Methods:");

                // Get the max size of a command name
                var dist = commands.Max(e => e.Sum(f => f.CommandName.Length) + e.Count() - 1) + 2;

                // Write each command
                foreach (var command in commands)
                {
                    _builder.Append("  ");
                    _builder.Append(string.Join(" ", command.Select(e => e.CommandName)).PadRight(dist));
                    var description = command.LastOrDefault(e => e.CommandDescription != null)?.CommandDescription;
                    if (description != null) _builder.Append(description);
                    _builder.AppendLine();
                }
                _builder.AppendLine();
            }

            // Join long options and short options, only select the option class and make it distinct
            var options = _layers.Values.SelectMany(e => e.Options).Union(_generalOptions).Distinct().Where(e => e.OptionVisibility.HasFlag(ArgumentVisiblility.Description));
            if (options.Any())
            {
                // Select a pair of items instead of the hole option class
                var selected = options.Select(e =>
                {
                    // Some flags for the current element
                    var hasShort = e.OptionShortName != '\0';
                    var hasLong = e.OptionLongName != null;
                    var hasArg = e.ObjectArgument != null;

                    // Return the right formatting for the right flag combination
                    if (hasShort && !hasLong) return new Tuple<string, string>($"-{e.OptionShortName}", e.OptionDescription);
                    if (!hasShort && hasLong) return new Tuple<string, string>($"--{e.OptionLongName}{(hasArg ? $"=<{e.ObjectArgument.ArgumentName}>" : string.Empty)}", e.OptionDescription);
                    if (hasShort) return new Tuple<string, string>($"-{e.OptionShortName} --{e.OptionLongName}{(hasArg ? $"=<{e.ObjectArgument.ArgumentName}>" : string.Empty)}", e.OptionDescription);

                    // We do not support options with no short and no long version
                    throw new NotSupportedException();
                });

                // Formatting distance, with which all items are padded
                var dist = selected.Max(e => e.Item1.Length) + 2;

                // Write the header
                _builder.AppendLine("Options:");
                // Write all options and their description
                foreach (var option in selected.Where(e => !string.IsNullOrWhiteSpace(e.Item2)))
                {
                    _builder.Append("  ");
                    _builder.Append(option.Item1.PadRight(dist));
                    _builder.AppendLine(option.Item2);
                }
                _builder.AppendLine();
            }

            // Return the final string
            return _builder.ToString();
        }

        /// <summary>
        ///     Adds a new layer to the parser and returns it.
        /// </summary>
        /// <param name="id">Id of the layer. Must be unique.</param>
        /// <exception cref="ArgumentException">If a layer with the given <paramref name="id" /> is already registered.</exception>
        /// <returns>The generated layer.</returns>
        public IArgumentLayer NewLayer(T id)
        {
            // Check if the id is already registered.
            if (_layers.ContainsKey(id)) throw new ArgumentException("A layer with this key is already registered.");

            // Create a new Layer, add and return it
            var layer = new Layer(_container);
            _layers.Add(id, layer);
            return layer;
        }

        /// <summary>
        ///     Adds a new option to the parser and returns a builder for it.
        /// </summary>
        /// <returns>The generated option.</returns>
        public IOptionBuilder NewOption()
        {
            var opt = new Option(_container);
            _generalOptions.Add(opt);
            return opt;
        }

        /// <summary>
        ///     Parses the given input string with the registered layers and returns a parse result.
        /// </summary>
        /// <param name="input">A string, which is formatted as if it would be passed over with the command line.</param>
        /// <returns>A result containing all parsed elements as well as information about the parsing result (e.g. success).</returns>
        public IArgumentResult<T> Parse(string input)
        {
            // Clear the builder
            _builder.Length = 0;

            // Items which will be given to the other Parse method
            var args = new List<string>();
            // If the parser is in a literal
            var inLiteral = false;
            for (var i = 0; i < input.Length; i++)
            {
                // Save the current char
                var c = input[i];
                // Check if we are in a literal
                if (inLiteral)
                {
                    // If the current char is a quotation mark we need to handle a special case, if not simply append the char
                    if (c == '"')
                    {
                        // If the next char is a quotation too we need to write a quotation (escape sequence), if not we quit the literal
                        if (i != input.Length - 1 && input[i + 1] == '"') _builder.Append('"');
                        else inLiteral = false;
                    }
                    else _builder.Append(c);
                }
                else
                {
                    // If the char is a whitespace we need to stop the current item and add it to the args
                    if (char.IsWhiteSpace(c))
                    {
                        // We only need to add the item if it has at least one char (multiple whitespaces should not yield multiple empty items)
                        if (_builder.Length == 0) continue;
                        // Add the argument to args and clear the builder for the next item
                        args.Add(_builder.ToString());
                        _builder.Length = 0;
                    }
                    // Handle the case of a literal
                    else if (c == '"') inLiteral = true;
                    else _builder.Append(c);
                }
            }
            // If there is a current item add it to args
            if (_builder.Length != 0) args.Add(_builder.ToString());

            // Parse the arguments
            return Parse(args);
        }

        /// <summary>
        ///     Parses the given arguments with the registered layers and returns a parse result.
        /// </summary>
        /// <param name="arguments">Arguments which should be parsed.</param>
        /// <returns>A result containing all parsed elements as well as information about the parsing result (e.g. success).</returns>
        public IArgumentResult<T> Parse(IEnumerable<string> arguments)
        {
            // Convert the arguments to an array
            var args = arguments.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            var size = args.Length;

            // Select all canidates
            var targets = _layers.Where(e => e.Value.Min <= size && size <= e.Value.Max).ToDictionary(e => e.Key, e => e.Value);
            if (targets.Count == 0) return new ArgumentResult<T>();

            // Values
            var data = new List<object>();
            var names = new List<string>();
            var flags = new Dictionary<string, bool>();
            var options = new Dictionary<string, object>();

            // To store the result
            var found = false;
            var key = default(T);
            // Go through all canidates
            foreach (var canidate in targets)
            {
                // Save the current work item key
                key = canidate.Key;

                // Some helper variables
                var l = canidate.Value;
                var req = new Queue<IGroupable>(l.Arguments.TakeWhile(e => e.Type != GroupType.Argument || ((Argument) e).ArgumentRequired));
                var opt = new Queue<IGroupable>(l.Arguments.SkipWhile(e => e.Type != GroupType.Argument || ((Argument) e).ArgumentRequired));
                var o = l.Options.ToList();

                // Go through argument and commands
                for (var i = 0; i < args.Length; i++)
                {
                    var c = args[i];
                    var exp = ValidateExpandedOption(c);
                    var sim = ValidateSimpleOption(c);
                    if ((exp || sim) && req.Count != 0) break;
                    // Handle expanded options
                    if (exp)
                    {
                        var m = expandedOption.Match(c);
                        var name = m.Groups["name"].Value;
                        var value = m.Groups["value"].Success ? m.Groups["value"].Value : null;
                        var op = o.FirstOrDefault(e => e.OptionLongName == name);
                        o.Remove(op);
                        if (op == null || (op.ObjectArgument != null && op.ObjectArgument.ArgumentRequired && value == null) || (op.ObjectArgument == null && value != null)) break;
                        if (value == null)
                        {
                            if (op.ObjectArgument != null) options[name] = null;
                            if (op.OptionShortName != '\0') flags["" + op.OptionShortName] = true;
                            flags[name] = true;
                        }
                        else
                        {
                            if (op.ObjectArgument.Validator != null)
                            {
                                object ex = null;
                                if (!op.ObjectArgument.Validator.Validate(value, ref ex)) break;
                                options[name] = ex;
                            }
                            else options[name] = value;
                            if (op.OptionShortName != '\0') flags["" + op.OptionShortName] = true;
                            flags[name] = true;
                        }
                    }
                    // Handle simple options
                    else if (sim)
                    {
                        // Select the name of the switch
                        var name = c[1];
                        // Try to get the corresponding option class
                        var op = o.FirstOrDefault(e => e.OptionShortName == name);
                        // If there is no option class this layer is invalid
                        if (op == null) break;

                        // Remove the switch from the list and set the flags
                        o.Remove(op);
                        flags["" + name] = true;
                        if (op.OptionLongName != null) flags[op.OptionLongName] = true;
                    }
                    // Handle required parameters
                    else if (req.Count > 0)
                    {
                        var item = req.Peek();
                        if (item.Type == GroupType.Argument)
                        {
                            data.Add(c);
                            req.Dequeue();
                        }
                        else if (item.Type == GroupType.Command)
                        {
                            if (((Command) item).CommandName != c) break;
                            req.Dequeue();
                        }
                        else throw new NotSupportedException();
                    }
                    // Handle the rest
                    else if (opt.Count > 0)
                    {
                        var item = opt.Peek();
                        if (item.Type == GroupType.Argument)
                        {
                            data.Add(c);
                            opt.Dequeue();
                        }
                        else throw new NotSupportedException();
                    }
                    else break;
                    // Handle exit
                    if (i == args.Length - 1)
                    {
                        found = true;
                        names = canidate.Value.Arguments.Where(e => e.Type == GroupType.Argument).Select(e => ((Argument) e).ArgumentName).ToList();
                    }
                }
                if (found) break;
            }

            // Retun a result
            return found ? new ArgumentResult<T>(key, data, names, flags, options) : new ArgumentResult<T>();
        }

        public void Validator<TV>(IValidator validator)
        {
            _container.SetValidator<TV>(validator);
        }

        private static readonly Regex simpleOption = new Regex(@"^-[a-z]$", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Checks if a string can be a simple option.
        /// </summary>
        /// <param name="target">String to check.</param>
        /// <returns><c>true</c> if the string follows the simple option format.</returns>
        private static bool ValidateSimpleOption(string target) => simpleOption.IsMatch(target);

        private static readonly Regex expandedOption = new Regex(@"^--(?<name>[a-z_]+)(=(?<value>.*))?$", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Checks if a string can be a expanded option.
        /// </summary>
        /// <param name="target">String to check.</param>
        /// <returns><c>true</c> if the string follows the expanded option format.</returns>
        private static bool ValidateExpandedOption(string target) => expandedOption.IsMatch(target);

        /// <summary>
        ///     Checks if a name is valid.
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns><c>true</c> if the name only consists of letters or underscores.</returns>
        private static bool ValidateName([CanBeNull] string name) => name?.All(c => char.IsLetter(c) || c == '_') ?? true;

        /// <summary>
        ///     Checks if a short name is valid.
        /// </summary>
        /// <param name="symbol">Short name to check.</param>
        /// <returns><c>true</c> if the name is letter.</returns>
        private static bool ValidateShort(char symbol) => symbol >= 'A' && symbol <= 'Z' || symbol >= 'a' && symbol <= 'z';


        /// <summary>
        ///     Represents a mechanism to differ between argument and command.
        /// </summary>
        private interface IGroupable
        {
            /// <summary>
            ///     Type of the grouping.
            /// </summary>
            GroupType Type { get; }
        }

        /// <summary>
        ///     Used to identify a group type.
        /// </summary>
        private enum GroupType
        {
            /// <summary>
            ///     Type of <seealso cref="ArgumentParser{T}.Argument" />
            /// </summary>
            Argument,

            /// <summary>
            ///     Type of <seealso cref="ArgumentParser{T}.Command" />
            /// </summary>
            Command
        }

        /// <summary>
        ///     A container for an argument argument.
        /// </summary>
        private class Argument : IArgumentBuilder, IGroupable
        {
            private IValidatorContainer _container;

            public Argument(IValidatorContainer container)
            {
                _container = container;
            }


            /// <summary>
            ///     Description of the argument.
            /// </summary>
            [CanBeNull]
            internal string ArgumentDescription { get; private set; }

            /// <summary>
            ///     Name of the argument.
            /// </summary>
            internal string ArgumentName { get; private set; }

            /// <summary>
            ///     If the argument is a required argument
            /// </summary>
            internal bool ArgumentRequired { get; private set; } = true;

            /// <summary>
            ///     Visibility of the argument.
            /// </summary>
            internal ArgumentVisiblility ArgumentVisibility { get; private set; } = ArgumentVisiblility.All;

            /// <summary>
            /// Validator used for the argument.
            /// </summary>
            [CanBeNull]
            internal IValidator Validator { get; set; }


            /// <summary>
            ///     Checks if the argument is valid.
            /// </summary>
            /// <returns><c>true</c> if the argument is a valid argument.</returns>
            public bool IsValid() => ArgumentName != null && ValidateName(ArgumentName);


            /// <summary>
            ///     Sets the description of the argument.
            /// </summary>
            /// <param name="description">Argument description.</param>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Description([CanBeNull] string description)
            {
                ArgumentDescription = description;
                return this;
            }

            /// <summary>
            ///     Hides the argument from all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Hide()
            {
                ArgumentVisibility = ArgumentVisiblility.None;
                return this;
            }

            /// <summary>
            ///     Marks the current argument as optional.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Optional()
            {
                ArgumentRequired = false;
                return this;
            }

            /// <summary>
            ///     Marks the current argument as required.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Required()
            {
                ArgumentRequired = true;
                return this;
            }

            /// <summary>
            ///     Sets the name of the argument.
            /// </summary>
            /// <param name="name">Name of the argument.</param>
            /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
            /// <returns>Itself for method chaining.</returns>
            /// <remarks>
            ///     The <paramref name="name" /> may only contain letters.
            /// </remarks>
            public IArgumentBuilder SetName([CanBeNull] string name)
            {
                if (!ValidateName(name)) throw new ArgumentException("Invalid name.", nameof(name));
                ArgumentName = name;
                return this;
            }

            /// <summary>
            ///     Shows the argument in all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Show()
            {
                ArgumentVisibility = ArgumentVisiblility.All;
                return this;
            }

            /// <summary>
            ///     Sets the type of the argument for validation and values storage.
            /// </summary>
            /// <typeparam name="TV">Type of the argument.</typeparam>
            /// <returns>Itself for method chaining.</returns>
            IArgumentBuilder IArgumentBuilder.Type<TV>()
            {
                Validator = _container.GetValidator<TV>();
                return this;
            }

            /// <summary>
            ///     Sets the visibility of the argument.
            /// </summary>
            /// <param name="visiblility">Target visibility.</param>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentBuilder Visibility(ArgumentVisiblility visiblility)
            {
                ArgumentVisibility = visiblility;
                return this;
            }

            /// <summary>
            ///     Type of the grouping.
            /// </summary>
            public GroupType Type => GroupType.Argument;
        }

        /// <summary>
        ///     A container for an argument command.
        /// </summary>
        private class Command : ICommandBuilder, IGroupable
        {
            /// <summary>
            ///     Name of the command.
            /// </summary>
            internal string CommandName { get; private set; }

            /// <summary>
            ///     Description of the command.
            /// </summary>
            [CanBeNull]
            internal string CommandDescription { get; private set; }

            /// <summary>
            ///     Visibility of the command.
            /// </summary>
            internal ArgumentVisiblility CommandVisibility { get; private set; } = ArgumentVisiblility.All;


            /// <summary>
            ///     Checks if the command is valid.
            /// </summary>
            /// <returns><c>true</c> if the command is valid.</returns>
            /// <remarks>
            ///     A command is valid if:
            ///     - The name is no <c>null</c> and
            ///     - The name consist only of letters and underscores
            /// </remarks>
            internal bool IsValid() => CommandName != null && ValidateName(CommandName);

            /// <summary>
            ///     Hides the command from all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public ICommandBuilder Hide()
            {
                CommandVisibility = ArgumentVisiblility.None;
                return this;
            }

            /// <summary>
            ///     Sets the name of the command.
            /// </summary>
            /// <param name="name">Name of the command.</param>
            /// <returns>Itself for method chaining.</returns>
            /// <remarks>
            ///     The <paramref name="name" /> may only contain letters.
            /// </remarks>
            public ICommandBuilder Name([CanBeNull] string name)
            {
                if (!ValidateName(name)) throw new ArgumentException("Invalid name.", nameof(name));
                CommandName = name;
                return this;
            }

            /// <summary>
            ///     Shows the command in all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public ICommandBuilder Show()
            {
                CommandVisibility = ArgumentVisiblility.All;
                return this;
            }

            /// <summary>
            ///     Sets the visibility of the command.
            /// </summary>
            /// <param name="visiblility">Target visibility.</param>
            /// <returns>Itself for method chaining.</returns>
            public ICommandBuilder Visibility(ArgumentVisiblility visiblility)
            {
                CommandVisibility = visiblility;
                return this;
            }

            /// <summary>
            ///     Sets the description of the command.
            /// </summary>
            /// <param name="description">Command description.</param>
            /// <returns>Itself for method chaining.</returns>
            public ICommandBuilder Description([CanBeNull] string description)
            {
                CommandDescription = description;
                return this;
            }

            /// <summary>
            ///     Type of the grouping.
            /// </summary>
            public GroupType Type => GroupType.Command;
        }

        /// <summary>
        ///     A container for an argument layer.
        /// </summary>
        private class Layer : IArgumentLayer
        {
            private List<IGroupable> _arguments = new List<IGroupable>();
            private int _rsize;
            private int _size;
            private bool _optional;
            private Dictionary<string, Option> _longOptions = new Dictionary<string, Option>();
            private Dictionary<char, Option> _shortOptions = new Dictionary<char, Option>();
            private IValidatorContainer _container;

            public Layer(IValidatorContainer container)
            {
                _container = container;
            }

            /// <summary>
            ///     Registered arguments.
            /// </summary>
            internal List<IGroupable> Arguments => _arguments;

            /// <summary>
            ///     Minimum size of the layer.
            /// </summary>
            internal int Min => _rsize;

            /// <summary>
            ///     Maximum size of the layer.
            /// </summary>
            internal int Max => _size;

            /// <summary>
            ///     All registered options
            /// </summary>
            internal IEnumerable<Option> Options => _shortOptions.Values.Concat(_longOptions.Values).Distinct();


            /// <summary>
            ///     Adds an argument to the current layer, which is set up by the given builder.
            /// </summary>
            /// <param name="builder">Builds the given argument.</param>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentLayer AddArgument(Action<IArgumentBuilder> builder)
            {
                // Check arguments
                if (builder == null) throw new ArgumentNullException(nameof(builder));

                // Create a option supply and supply it to the 
                var argument = new Argument(_container);
                builder(argument);

                // Check if the argument is valid and if not add it to the options
                if (!argument.IsValid()) throw new ArgumentException("The built option is invalid.");
                if (argument.ArgumentRequired && _optional) throw new ArgumentException("Can not add a required argument after an optional argument.");
                if (!argument.ArgumentRequired) _optional = true;
                _arguments.Add(argument);

                if (!_optional) _rsize++;
                _size++;

                // Return myself for chaining
                return this;
            }

            /// <summary>
            ///     Adds a command to the current layer, which is set up by the given builder.
            /// </summary>
            /// <param name="builder">Builds the given command.</param>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentLayer AddCommand(Action<ICommandBuilder> builder)
            {
                // Check arguments
                if (builder == null) throw new ArgumentNullException(nameof(builder));

                // Create a command supply and supply it to the 
                var command = new Command();
                builder(command);

                // Check if the command is valid and if not add it to the arguments
                if (!command.IsValid()) throw new ArgumentException("The built option is invalid.");
                if (_optional) throw new ArgumentException("Can not add a command after an optional argument.");
                _arguments.Add(command);

                _rsize++;
                _size++;

                // Return myself for chaining
                return this;
            }

            /// <summary>
            ///     Adds a option to the current layer, which is set up by the given builder.
            /// </summary>
            /// <param name="builder">Builds the given option.</param>
            /// <returns>Itself for method chaining.</returns>
            public IArgumentLayer AddOption(Action<IOptionBuilder> builder)
            {
                // Check arguments
                if (builder == null) throw new ArgumentNullException(nameof(builder));

                // Create a option supply and supply it to the 
                var option = new Option(_container);
                builder(option);

                // Check if the option is valid and if not add it to the options
                if (!option.IsValid()) throw new ArgumentException("The built option is invalid.");

                // If the option has a short name add it
                var shortName = option.OptionShortName;
                if (shortName != '\0')
                {
                    if (_shortOptions.ContainsKey(shortName)) throw new ArgumentException("A option with the given short name is already registered.");
                    _shortOptions.Add(shortName, option);
                }

                // If the option has a long name add it
                var longName = option.OptionLongName;
                if (longName != null)
                {
                    if (_longOptions.ContainsKey(longName)) throw new ArgumentException("A option with the given long name is already registered.");
                    _longOptions.Add(longName, option);
                }

                // We don't support empty options
                if (shortName == '\0' && longName == null) throw new NotSupportedException();
                if (option.OptionRequired) _rsize++;
                _size++;

                // Return myself for chaining
                return this;
            }

            /// <summary>
            ///     Generates a help description for the current line and writes it into the given string builder.
            /// </summary>
            /// <param name="builder">Target string builder</param>
            internal void GenerateHelp(StringBuilder builder)
            {
                // Add arguments
                for (var i = 0; i < _arguments.Count; i++)
                {
                    var c = _arguments[i];

                    if (c.Type == GroupType.Command)
                    {
                        var o = (Command) c;
                        if (!o.CommandVisibility.HasFlag(ArgumentVisiblility.Usage)) continue;
                        builder.Append(o.CommandName);
                    }
                    else if (c.Type == GroupType.Argument)
                    {
                        var o = (Argument) c;
                        if (!o.ArgumentVisibility.HasFlag(ArgumentVisiblility.Usage)) continue;
                        builder.Append(o.ArgumentRequired ? '<' + o.ArgumentName + '>' : '[' + o.ArgumentName + ']');
                    }
                    else throw new NotSupportedException();
                    if (i != _arguments.Count - 1) builder.Append(' ');
                }

                // Add options
                var space = _arguments.Count > 0;
                foreach (var option in _shortOptions.Values.Concat(_longOptions.Values).Distinct().OrderBy(e => !e.OptionRequired).Where(e => e.OptionVisibility.HasFlag(ArgumentVisiblility.Usage)))
                {
                    if (space) builder.Append(" ");
                    space = true;
                    var hasShort = option.OptionShortName != '\0';
                    var hasLong = option.OptionLongName != null;
                    var hasArg = option.ObjectArgument != null;
                    if (!option.OptionRequired) builder.Append('[');
                    if (hasShort && !hasLong) builder.Append("-" + option.OptionShortName + "");
                    else if (!hasShort && hasLong) builder.Append("--" + option.OptionLongName + (hasArg ? "=<" + option.ObjectArgument.ArgumentName + ">" : string.Empty));
                    else if (hasShort) builder.Append("-" + option.OptionShortName + "|--" + option.OptionLongName + "");
                    if (!option.OptionRequired) builder.Append(']');
                }
            }
        }

        /// <summary>
        ///     A container for an argument option.
        /// </summary>
        private class Option : IOptionBuilder
        {
            private IValidatorContainer _container;

            public Option(IValidatorContainer container)
            {
                _container = container;
            }


            /// <summary>
            ///     Argument/Value of the option or <c>null</c> if there is none.
            /// </summary>
            [CanBeNull]
            internal Argument ObjectArgument { get; private set; }

            /// <summary>
            ///     Option description or <c>null</c> if there is none.
            /// </summary>
            [CanBeNull]
            internal string OptionDescription { get; private set; }

            /// <summary>
            ///     <c>true</c> if the option is required.
            /// </summary>
            internal bool OptionRequired { get; private set; }

            /// <summary>
            ///     The long name of the option if there is one else <c>null</c>.
            /// </summary>
            [CanBeNull]
            internal string OptionLongName { get; private set; }

            /// <summary>
            ///     The shor name of the option is there is one else '\0'.
            /// </summary>
            internal char OptionShortName { get; private set; } = '\0';

            /// <summary>
            ///     Visibility of the option.
            /// </summary>
            internal ArgumentVisiblility OptionVisibility { get; private set; } = ArgumentVisiblility.All;

            /// <summary>
            /// Validator used for the option.
            /// </summary>
            [CanBeNull]
            internal IValidator Validator { get; set; }


            /// <summary>
            ///     Adds an argument to the option.
            /// </summary>
            /// <param name="builder">The builder for the option argument.</param>
            /// <returns>Itself for method chaining.</returns>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public IOptionBuilder Argument(Action<IArgumentBuilder> builder)
            {
                // Check parameter
                if (builder == null) throw new ArgumentNullException(nameof(builder));

                // Create a argument and build it
                var argument = new Argument(_container);
                builder(argument);

                // Check if the argument is valid and set it
                if (!argument.IsValid()) throw new ArgumentException("The built argument is not valid");
                ObjectArgument = argument;

                return this;
            }

            /// <summary>
            ///     Sets the description of the option.
            /// </summary>
            /// <param name="description">The option description.</param>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Description([CanBeNull] string description)
            {
                OptionDescription = description;
                return this;
            }

            /// <summary>
            ///     Hides the option from all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Hide()
            {
                OptionVisibility = ArgumentVisiblility.None;
                return this;
            }

            /// <summary>
            ///     Marks the option as optional.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Optional()
            {
                OptionRequired = false;
                return this;
            }

            /// <summary>
            ///     Marks the option as required.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Required()
            {
                OptionRequired = true;
                return this;
            }

            /// <summary>
            ///     Checks if the option is a valid.
            /// </summary>
            /// <returns>If the option is valid.</returns>
            /// <remarks>
            ///     In order for an option to be valid the following must be true:
            ///     - There must be a short or a long name supplied (or both)
            ///     - If there is a long name it must not be empty
            ///     - If the argument is not <c>null</c> it must be valid
            /// </remarks>
            internal bool IsValid() => (OptionShortName != '\0' || OptionLongName != null) && (OptionShortName == '\0' || ValidateShort(OptionShortName)) && (OptionLongName == null || ValidateName(OptionLongName)) && (ObjectArgument == null || OptionShortName == '\0');

            /// <summary>
            ///     Sets the long name of the option (e.g. --help).
            /// </summary>
            /// <param name="longName">Long name of the option</param>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder SetLongName([CanBeNull] string longName)
            {
                if (!ValidateName(longName)) throw new ArgumentException("Invalid name.", longName);
                OptionLongName = string.IsNullOrWhiteSpace(longName) ? null : longName;
                return this;
            }

            /// <summary>
            ///     Sets the short name of the option (e.g. -h).
            /// </summary>
            /// <param name="shortName">Short name of the option.</param>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder SetShortName(char shortName)
            {
                OptionShortName = shortName;
                return this;
            }

            /// <summary>
            ///     Shows the option in all sections.
            /// </summary>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Show()
            {
                OptionVisibility = ArgumentVisiblility.All;
                return this;
            }

            /// <summary>
            ///     Sets the type of the option for validation and values storage.
            /// </summary>
            /// <typeparam name="TV">Type of the option.</typeparam>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Type<TV>()
            {
                Validator = _container.GetValidator<TV>();
                return this;
            }

            /// <summary>
            ///     Sets the visibility of the option.
            /// </summary>
            /// <param name="visiblility">Target visibility.</param>
            /// <returns>Itself for method chaining.</returns>
            public IOptionBuilder Visibility(ArgumentVisiblility visiblility)
            {
                OptionVisibility = visiblility;
                return this;
            }
        }
    }
}