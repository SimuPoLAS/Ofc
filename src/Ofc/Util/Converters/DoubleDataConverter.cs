namespace Ofc.Util.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Ofc.Core;

    public class DoubleDataConverter : IConverter<double>
    {
        private readonly Encoding _encoding;


        public DoubleDataConverter() : this(Encoding.UTF8)
        {
        }

        public DoubleDataConverter(Encoding encoding)
        {
            _encoding = encoding;
        }


        public void Write(Stream output, double value)
        {
            var bytes = _encoding.GetBytes(value.ToString(CultureInfo.InvariantCulture));
            output.Write(bytes, 0, bytes.Length);
        }

        public double Read(Stream input)
        {
            throw new NotImplementedException();
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