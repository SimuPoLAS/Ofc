using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using OfcCore.Utility;

namespace OfcAlgorithm.Integration
{
    public struct OfcNumber
    {
        public bool IsNegative;
        public long Number;
        public short Exponent;
        public double Reconstructed => Number * (IsNegative ? -1 : 1) * Math.Pow(10, Exponent);
        public byte NeededBitsNumber;
        public readonly byte NeededBitsExponent;
        //private static readonly long[] NeededBits;

        //static OfcNumber()
        //{
        //    NeededBits = new long[64];
        //    for (var i = 0; i < NeededBits.Length; i++)
        //        NeededBits[i] = (long)(Math.Pow(2, i + 1) - 1);
        //}

        public OfcNumber(long number, short exponent)
        {
            IsNegative = number < 0;
            Number = Math.Abs(number);
            Exponent = exponent;
            NeededBitsNumber = Number.GetNeededBits();
            NeededBitsExponent = Utility.GetNeededBits(Exponent);

            //for (byte i = 0; i < 64; i++)
            //    if (NeededBits[i] >= Number)
            //    {
            //        NeededBitsNumber = (byte)(i + 1);
            //        break;
            //    }

            //for (byte i = 0; i < 64; i++)
            //    if (NeededBits[i] >= Math.Abs(Exponent))
            //    {
            //        NeededBitsExponent = (byte)(i + 1);
            //        break;
            //    }
        }

        public static OfcNumber Parse(string value)
        {
            var valueLength = (ushort)value.Length;
            var commaIndex = valueLength;
            for (ushort i = 0; i < valueLength; i++)
            {
                if (value[i] == '.')
                {
                    commaIndex = i;
                    continue;
                }
                if (value[i] == 'e')
                    return new OfcNumber(long.Parse(value.Substring(0, i), NumberStyles.Any), (short)(-(i - 1) + commaIndex + short.Parse(value.Substring(i + 1))));
            }

            return new OfcNumber(long.Parse(value, NumberStyles.Any), (short)(commaIndex == valueLength ? 0 : -(valueLength - 1) + commaIndex));
        }

        public override string ToString() => $"{{Reconstructed: {Reconstructed}, Number: {Number}({NeededBitsNumber}), Exponent: {Exponent}({NeededBitsExponent}}}";

        public static implicit operator double(OfcNumber sc1) => sc1.Reconstructed;
        public static implicit operator int(OfcNumber sc1) => (int)sc1.Reconstructed;

        public static bool operator <(OfcNumber first, OfcNumber second) => first.Reconstructed < second.Reconstructed;
        public static bool operator >(OfcNumber first, OfcNumber second) => first.Reconstructed > second.Reconstructed;

        public static bool operator <(OfcNumber first, double second) => first.Reconstructed < second;
        public static bool operator >(OfcNumber first, double second) => first.Reconstructed > second;

        public static bool operator <(double first, OfcNumber second) => first < second.Reconstructed;
        public static bool operator >(double first, OfcNumber second) => first > second.Reconstructed;


        //public static OfcNumber operator +(OfcNumber num1, OfcNumber num2)
        //{
        //    num1.Number += num2.Number * (num2.IsNegative ? -1 : 1);
        //    num1.Exponent += num2.Exponent;
        //    return num1;
        //}

        //public static OfcNumber operator -(OfcNumber num1, OfcNumber num2)
        //{
        //    num1.Number -= num2.Number * (num2.IsNegative ? -1 : 1);
        //    num1.Exponent -= num2.Exponent;
        //    return num1;
        //}

        public OfcNumber AddEach(OfcNumber other)
        {
            var num = this;
            num.Number += other.Number * (other.IsNegative ? -1 : 1);
            num.Exponent += other.Exponent;
            return num;
        }

        public OfcNumber SubtractEach(OfcNumber other)
        {
            var num = this;
            num.Number -= other.Number * (other.IsNegative ? -1 : 1);
            num.Exponent -= other.Exponent;
            return num;
        }

        public double SubtractGetDouble(OfcNumber num2) => Reconstructed - num2.Reconstructed;

        public double AddGetDouble(OfcNumber num2) => Reconstructed + num2.Reconstructed;

        public OfcNumber LinearMultiplyEach(int num)
        {
            var res = this;
            res.Number *= num;
            res.Exponent *= (short)num;
            return res;
        }

        public bool Equals(OfcNumber other)
        {
            return IsNegative == other.IsNegative && Number == other.Number && Exponent == other.Exponent;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsNegative.GetHashCode();
                hashCode = (hashCode * 397) ^ Number.GetHashCode();
                hashCode = (hashCode * 397) ^ Exponent.GetHashCode();
                return hashCode;
            }
        }
    }
}
