namespace Ofc.Core.Configurations
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents an empty configuration. It can not be filled.
    /// </summary>
    public class EmptyConfiguration : IConfiguaration
    {
        /// <summary>
        ///     Private singleton member.
        /// </summary>
        private static IConfiguaration instance;

        /// <summary>
        ///     Singlton instance of <see cref="EmptyConfiguration" />.
        /// </summary>
        public static IConfiguaration Instance => instance ?? (instance = new EmptyConfiguration());


        /// <summary>
        ///     Private constructor for singleton use only.
        /// </summary>
        private EmptyConfiguration()
        {
        }


        [ContractAnnotation("=> null")]
        public object Get(string name) => null;

        [ContractAnnotation("=> false")]
        public bool Has(string name) => false;

        [ContractAnnotation("=> false")]
        public bool Remove(string name) => false;

        public void Set(string name, [CanBeNull] object value)
        {
        }

        public object this[string name]
        {
            get { return null; }
            set { }
        }
    }
}