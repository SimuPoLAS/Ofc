namespace Ofc.Parsing
{
    /// <summary>
    ///     Represents a type of list.
    /// </summary>
    /// <remarks>
    /// The integer value represents the amount of horizontal elements.
    /// </remarks>
    internal enum OfcListType
    {
        /// <summary>
        /// Represents a list of any value.
        /// </summary>
        Anonymous = 0,

        /// <summary>
        /// Represents a list of scalar values.
        /// </summary>
        Scalar = 1,

        /// <summary>
        /// Represents a list of vector values.
        /// </summary>
        Vector = 3,

        /// <summary>
        /// Represents a list of tensor values.
        /// </summary>
        Tensor = 9
    }
}