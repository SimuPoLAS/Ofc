namespace Ofc.Parsing
{
    using JetBrains.Annotations;

    internal struct OfcToken
    {
        internal OfcTokenType Type;
        [CanBeNull] internal string Text;

        internal uint Column;
        internal uint Length;
        internal uint Line;


        public OfcToken(OfcTokenType type) : this(type, null)
        {
        }

        public OfcToken(OfcTokenType type, [CanBeNull] string text) : this(type, text, 0, 0, 0)
        {
        }

        public OfcToken(OfcTokenType type, [CanBeNull] string text, uint column, uint length, uint line)
        {
            Type = type;
            Text = text;

            Column = column;
            Length = length;
            Line = line;
        }
    }
}