namespace Ofc.CLI.Validators
{
    using System.Globalization;

    internal class RoudingValidator : IValidator
    {
        public bool Validate(string value, ref object data)
        {
            var splits = value.Split(',');
            if (splits.Length != 3) return false;

            double min;
            if (!double.TryParse(splits[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out min)) return false;
            double max;
            if (!double.TryParse(splits[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out max)) return false;
            double eps;
            if (!double.TryParse(splits[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out eps)) return false;

            data = new RoundingData(min, max, eps);
            return true;
        }
    }
}