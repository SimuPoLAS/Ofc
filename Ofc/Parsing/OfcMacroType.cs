namespace Ofc.Parsing
{
    /// <summary>
    ///     Represents all types of directive.
    /// </summary>
    internal enum OfcMacroType
    {
        /// <summary>
        ///     Represents the include directive.
        /// </summary>
        /// <remarks>
        ///     #include "file2"
        /// </remarks>
        Include,

        /// <summary>
        ///     Represents the inputMode directive.
        /// </summary>
        InputMode,

        /// <summary>
        ///     Represents the remove directive.
        /// </summary>
        Remove
    }
}