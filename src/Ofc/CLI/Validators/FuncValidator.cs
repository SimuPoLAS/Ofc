namespace Ofc.CLI.Validators
{
    internal class FuncValidator : IValidator
    {
        internal delegate bool Validator(string v, ref object d);

        private Validator _func;

        public FuncValidator(Validator v)
        {
            _func = v;
        }

        public bool Validate(string value, ref object data)
        {
            return _func(value, ref data);
        }
    }
}