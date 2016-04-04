namespace OfcCore.Converters
{
    using System.Globalization;
    using System.IO;

    public class DoubleConverter : IConverter<double>
    {
        public void Write(Stream output, double value)
        {
            throw new System.NotImplementedException();
        }

        public double Read(Stream input)
        {
            throw new System.NotImplementedException();
        }

        public double FromString(string target)
        {
            return double.Parse(target);
        }

        public string ToString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}