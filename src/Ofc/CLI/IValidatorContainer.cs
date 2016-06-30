namespace Ofc.CLI
{
    using JetBrains.Annotations;

    internal interface IValidatorContainer
    {
        [CanBeNull]
        IValidator GetValidator<T>();

        void SetValidator<T>(IValidator validator);
    }
}