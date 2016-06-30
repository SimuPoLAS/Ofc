namespace Ofc.CLI
{
    internal struct RoundingData
    {
        internal double Min;
        internal double Max;
        internal double Epsilon;

        public RoundingData(double min, double max, double epsilon)
        {
            Min = min;
            Max = max;
            Epsilon = epsilon;
        }


        public bool Equals(RoundingData other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max) && Epsilon.Equals(other.Epsilon);
        }

        /// <summary>Gibt an, ob diese Instanz und ein angegebenes Objekt gleich sind.</summary>
        /// <returns>true, wenn <paramref name="obj" /> und diese Instanz denselben Typ aufweisen und denselben Wert darstellen, andernfalls false. </returns>
        /// <param name="obj">Das Objekt, das mit der aktuellen Instanz verglichen werden soll. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RoundingData && Equals((RoundingData)obj);
        }

        /// <summary>Gibt den Hashcode für diese Instanz zurück.</summary>
        /// <returns>Eine 32-Bit-Ganzzahl mit Vorzeichen. Diese ist der Hashcode für die Instanz.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Min.GetHashCode();
                hashCode = (hashCode * 397) ^ Max.GetHashCode();
                hashCode = (hashCode * 397) ^ Epsilon.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Gibt den voll qualifizierten Typnamen dieser Instanz zurück.</summary>
        /// <returns>Eine <see cref="T:System.String" />-Klasse, die den voll qualifizierten Typnamen enthält.</returns>
        public override string ToString()
        {
            return $"Min: {Min}, Max: {Max}, Epsilon: {Epsilon}";
        }
    }
}