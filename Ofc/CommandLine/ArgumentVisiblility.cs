namespace Ofc.CommandLine
{
    using System;

    /// <summary>
    ///     Describes where an argument is visible.
    /// </summary>
    [Flags]
    public enum ArgumentVisiblility
    {
        /// <summary>
        ///     The argument is hidden.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The argument is shown in the usage section.
        /// </summary>
        Usage = 1,

        /// <summary>
        ///     The argument is shown in the descriptive section.
        /// </summary>
        Description = 2,

        /// <summary>
        ///     The argument is shown everywhere.
        /// </summary>
        All = 3
    }
}