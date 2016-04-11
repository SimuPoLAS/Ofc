namespace Ofc.Parsing
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents a token emmited by the lexer and passed to the parser.
    /// </summary>
    internal struct OfcToken
    {
        /// <summary>
        ///     The type of the token.
        /// </summary>
        internal OfcTokenType Type;

        /// <summary>
        ///     The textual payload of the token.
        /// </summary>
        [CanBeNull] internal string Payload;


        /// <summary>
        ///     Position in the source file at which the token starts.
        /// </summary>
        internal uint Position;

        /// <summary>
        ///     Column at which the token starts.
        /// </summary>
        internal uint Column;

        /// <summary>
        ///     Length of the token in characters.
        /// </summary>
        internal uint Length;

        /// <summary>
        ///     Line at which the token starts.
        /// </summary>
        internal uint Line;


        /// <summary>
        ///     Sets up the token with the specified type the payload is asserted to be <c>null</c>.
        /// </summary>
        /// <param name="type">Type of the token.</param>
        public OfcToken(OfcTokenType type) : this(type, null, 0)
        {
        }

        /// <summary>
        ///     Sets up the token with the specified type and payload.
        /// </summary>
        /// <param name="type">Type of the token.</param>
        /// <param name="payload">Payload of the token.</param>
        public OfcToken(OfcTokenType type, [CanBeNull] string payload, uint position) : this(type, payload, position, 0, 0, 0)
        {
        }

        /// <summary>
        ///     Sets up the token with the specified type and payload as well es token positioning and sizing information.
        /// </summary>
        /// <param name="type">Type of the token.</param>
        /// <param name="payload">Payload of the token.</param>
        /// <param name="column">Column at which the token starts.</param>
        /// <param name="length">Length of the token in characters.</param>
        /// <param name="line">Line at which the token starts.</param>
        public OfcToken(OfcTokenType type, [CanBeNull] string payload, uint position, uint column, uint length, uint line)
        {
            Type = type;
            Payload = payload;

            Position = position;
            Column = column;
            Length = length;
            Line = line;
        }
    }
}