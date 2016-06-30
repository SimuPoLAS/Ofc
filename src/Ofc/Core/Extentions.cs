namespace Ofc.Core
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides extention methods for common types in this assembly.
    /// </summary>
    public static class Extentions
    {
        /// <summary>
        /// Provides a generic method to retrieve a configuration element from a configuration.
        /// </summary>
        /// <typeparam name="T">Type of the configuration element.</typeparam>
        /// <param name="configuaration">The configuration from which the element should be retrieved.</param>
        /// <param name="name">The name of the configuration element which should be retrieved.</param>
        /// <returns>The configuration item as the specified type. Or <c>null</c> if the element does not exist or the element could not be cast.</returns>
        [CanBeNull]
        public static T Get<T>(this IConfiguaration configuaration, string name) => (T)Convert.ChangeType(configuaration[name], typeof(T));

        public static bool True(this IConfiguaration configuaration, string name) => configuaration[name] as bool? ?? false;
    }
}