namespace Ofc.Algorithm.Integration
{
    using System;
    using System.Globalization;
    using Ofc.Util;

    public struct OfcNumber
    {
        public bool IsNegative;
        public long Number;
        public short Exponent;
        public double Reconstructed => Number * (IsNegative ? -1 : 1) * Math.Pow(10, Exponent);
        public byte NeededBitsNumber;
        public byte NeededBitsExponent;
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
            if (Number != 0)
                Exponent = exponent;
            else
                Exponent = 0;
            NeededBitsNumber = Number.GetNeededBits();
            NeededBitsExponent = Utility.GetNeededBits(Math.Abs(Exponent));
            //RecalculateMetadata();
        }

        /// <summary>
        /// Needs to be called after chaning values - the metadata fields are no properties for performance reasons
        /// </summary>
        public void RecalculateMetadata()
        {
            IsNegative = Number < 0;
            Number = Math.Abs(Number);
            NeededBitsNumber = Number.GetNeededBits();
            NeededBitsExponent = Utility.GetNeededBits(Math.Abs(Exponent));
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
                if (value[i] == 'e' || value[i] == 'E')
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

        /// <summary>
        /// Will not expand the mantissa (number) for more accuracy, even if needed! (Preventing huge numbers)
        /// Also, no overflow checks here
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Will add a OfcNumber to another OfcNumber and return the result</returns>
        public static OfcNumber operator +(OfcNumber first, OfcNumber second)
        {
            first.Number += (int)(second.Number * Math.Pow(10, second.Exponent - first.Exponent));
            first.RecalculateMetadata();
            return first;
        }

        /// <summary>
        /// Will not expand the mantissa (number) for more accuracy, even if needed! (Preventing huge numbers)
        /// Also, no overflow checks here
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Will add a OfcNumber to another OfcNumber and return the result</returns>
        public static OfcNumber operator -(OfcNumber first, OfcNumber second)
        {
            first.Number -= (int)(second.Number * Math.Pow(10, second.Exponent - first.Exponent));
            first.RecalculateMetadata();
            return first;
        }

        /// <summary>
        /// Mantissa + Mantissa, Exponent + Exponent
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The resulting OfcNumber</returns>
        public OfcNumber AddEach(OfcNumber other)
        {
            var num = this;
            num.Number += other.Number * (other.IsNegative ? -1 : 1);
            num.Exponent += other.Exponent;
            return num;
        }

        /// <summary>
        /// Mantissa - Mantissa, Exponent - Exponent
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The resulting OfcNumber</returns>
        public OfcNumber SubtractEach(OfcNumber other)
        {
            var num = this;
            num.Number -= other.Number * (other.IsNegative ? -1 : 1);
            num.Exponent -= other.Exponent;
            return num;
        }

        /// <summary>
        /// Mantissa * num, Exponent * num
        /// </summary>
        /// <param name="num">factor</param>
        /// <returns>The resulting OfcNumber</returns>
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
