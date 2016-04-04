namespace OfcCore
{
    using JetBrains.Annotations;

    public interface IConfiguaration
    {
        [CanBeNull]
        object Get(string name);

        bool Has(string name);

        bool Remove(string name);

        void Set(string name, [CanBeNull] object value);

        [CanBeNull]
        object this[string name] { get; set; }
    }
}