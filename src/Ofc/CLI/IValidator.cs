namespace Ofc.CLI
{
    internal interface IValidator
    {
        bool Validate(string value, ref object data);
    }
}