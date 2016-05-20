namespace Ofc.Core
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents a configuration object which stores key-object pairs.
    /// </summary>
    public interface IConfiguaration
    {
        /// <summary>
        ///     Retrieves and object by name.
        /// </summary>
        /// <param name="name">Name of the object to retrieves.</param>
        /// <returns>The retrieved object or <c>null</c> if the given object can not be found.</returns>
        [CanBeNull]
        object Get(string name);

        /// <summary>
        ///     Checks if the configuration holds a key-object pair with the specified name.
        /// </summary>
        /// <param name="name">Name of the key-object pair which should be checked for existens.</param>
        /// <returns><c>true</c> if the key-object pair exists in the configuration, otherwise <c>false</c>.</returns>
        bool Has(string name);

        /// <summary>
        ///     Removes a key-object pair from the configuration.
        /// </summary>
        /// <param name="name">Name of the key-object pair to remove.</param>
        /// <returns>Returns <c>true</c> if the key-object pair was successfully removed, otherwise <c>false</c>.</returns>
        bool Remove(string name);

        /// <summary>
        ///     Stores an object by a name.
        /// </summary>
        /// <param name="name">Name under which the specified object will be stored.</param>
        /// <param name="value">Object which will be stored under the specified name.</param>
        void Set(string name, [CanBeNull] object value);

        /// <summary>
        ///     Retrieves or stores and object by a name.
        /// </summary>
        /// <param name="name">Name of the object to retrieve or store.</param>
        /// <returns>The retrieved object or <c>null</c> if the object can not be found in the configuration.</returns>
        [CanBeNull]
        object this[string name] { get; set; }
    }
}