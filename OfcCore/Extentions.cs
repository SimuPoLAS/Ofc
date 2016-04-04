namespace OfcCore
{
    using JetBrains.Annotations;

    public static class Extentions
    {
        [CanBeNull]
        public static T Get<T>(this IConfiguaration configuaration, string name) where T : class => configuaration[name] as T;
    }
}