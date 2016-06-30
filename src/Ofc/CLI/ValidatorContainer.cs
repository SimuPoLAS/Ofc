namespace Ofc.CLI
{
    using System;
    using System.Collections.Generic;

    internal class ValidatorContainer : IValidatorContainer
    {
        private Dictionary<Type, IValidator> _validators = new Dictionary<Type, IValidator>();


        public IValidator GetValidator<T>()
        {
            IValidator v;
            return !_validators.TryGetValue(typeof(T), out v) ? null : v;
        }

        public void SetValidator<T>(IValidator validator)
        {
            if (validator == null) _validators.Remove(typeof(T));
            else _validators[typeof(T)] = validator;
        }
    }
}