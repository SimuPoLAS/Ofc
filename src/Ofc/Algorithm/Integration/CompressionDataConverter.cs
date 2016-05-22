namespace Ofc.Algorithm.Integration
{
    using System;
    using System.Globalization;
    using System.IO;
    using JetBrains.Annotations;
    using Ofc.Core;

    public class CompressionDataConverter : IConverter<OfcNumber>
    {
        public void Write([NotNull]BinaryWriter writer, OfcNumber data)
        {
            writer.Write(data);
        }


        public void Write([NotNull]Stream writer, OfcNumber value)
        {
            writer.Write(BitConverter.GetBytes(value.Reconstructed), 0, 8);
        }

        public OfcNumber Read([NotNull]Stream reader)
        {
            throw new NotImplementedException(); // todo needs a rewrite
        }

        public OfcNumber FromString(string value)
        {
            return OfcNumber.Parse(value);
        }

        public string ToString(OfcNumber value)
        {
            return value.Reconstructed.ToString(CultureInfo.InvariantCulture);
        }

        public OfcNumber FromDouble(double value)
        {
            return FromString(value.ToString(CultureInfo.InvariantCulture));
        }

        public double ToDouble(OfcNumber value)
        {
            return value.Reconstructed;
        }
    }
}