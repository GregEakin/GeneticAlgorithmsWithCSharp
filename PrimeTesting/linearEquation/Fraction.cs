using System;

namespace PrimeTesting.linearEquation
{
    public class Fraction : IComparable
    {
        public int Numerator { get; }

        public int Denominator { get; }

        public Fraction(int number)
        {
            Numerator = number;
            Denominator = 1;
        }

        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator is zero!", nameof(denominator));

            if (numerator == 0)
            {
                Numerator = 0;
                Denominator = 1;
                return;
            }

            var absNumerator = Math.Abs(numerator);
            var absDenominator = Math.Abs(denominator);
            var divisor = GreatestCommonDivisor(absNumerator, absDenominator);
            if (denominator >= 0)
            {
                Numerator = numerator / divisor;
                Denominator = denominator / divisor;
            }
            else
            {
                Numerator = -numerator / divisor;
                Denominator = -denominator / divisor;
            }
        }

        //public Fraction(long numerator, long denominator)
        //{
        //    if (int.MinValue > numerator || numerator > int.MaxValue)
        //        throw new ArgumentException("Numerator out of range!", nameof(numerator));
        //    if (int.MinValue > denominator || denominator > int.MaxValue)
        //        throw new ArgumentException("Denominator out of range!", nameof(denominator));
        //    if (denominator == 0)
        //        throw new ArgumentException("Denominator is zero!", nameof(denominator));

        //    var num = (int) numerator;
        //    var dem = (int) denominator;

        //    var absNumerator = Math.Abs(num);
        //    var absDenominator = Math.Abs(dem);
        //    var divisor = GreatestCommonDivisor(absNumerator, absDenominator);
        //    if (denominator >= 0)
        //    {
        //        Numerator = num / divisor;
        //        Denominator = dem / divisor;
        //    }
        //    else
        //    {
        //        Numerator = -num / divisor;
        //        Denominator = -dem / divisor;
        //    }
        //}

        public override string ToString()
        {
            return Denominator == 1
                ? Numerator.ToString()
                : $"{Numerator}/{Denominator}";
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Fraction that:
                    return Numerator == that.Numerator && Denominator == that.Denominator;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            if (Numerator == 0)
                return 0;
            return Numerator ^ Denominator;
        }

        public static Fraction Abs(Fraction fraction)
        {
            return fraction.Numerator > 0
                ? fraction
                : -fraction;
        }

        public static Fraction operator -(Fraction d1)
        {
            return new Fraction(-d1.Numerator, d1.Denominator);
        }

        public static Fraction operator +(int d1, Fraction d2)
        {
            var fraction = new Fraction(d1 * d2.Denominator + d2.Numerator, d2.Denominator);
            return fraction;
        }

        public static Fraction operator +(Fraction d1, int d2)
        {
            var fraction = new Fraction(d1.Numerator + d2 * d1.Denominator, d1.Denominator);
            return fraction;
        }

        public static Fraction operator +(Fraction d1, Fraction d2)
        {
            var gcd = GreatestCommonDivisor(d1.Denominator, d2.Denominator);
            var numerator = d1.Numerator * (d2.Denominator / gcd) + d2.Numerator * (d1.Denominator / gcd);
            var denominator = d1.Denominator * (d2.Denominator / gcd);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator -(int d1, Fraction d2)
        {
            var numerator = d1 * d2.Denominator - d2.Numerator;
            var fraction = new Fraction(numerator, d2.Denominator);
            return fraction;
        }

        public static Fraction operator -(Fraction d1, int d2)
        {
            var numerator = d1.Numerator - d2 * d1.Denominator;
            var fraction = new Fraction(numerator, d1.Denominator);
            return fraction;
        }

        public static Fraction operator -(Fraction d1, Fraction d2)
        {
            var gcd = GreatestCommonDivisor(d1.Denominator, d2.Denominator);
            var numerator = d1.Numerator * (d2.Denominator / gcd) - d2.Numerator * (d1.Denominator / gcd);
            var denominator = d1.Denominator * (d2.Denominator / gcd);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(Fraction d1, Fraction d2)
        {
            var g1 = GreatestCommonDivisor(d1.Numerator, d2.Denominator);
            var g2 = GreatestCommonDivisor(d1.Denominator, d2.Numerator);
            var numerator = (d1.Numerator / g1) * (d2.Numerator / g2);
            var denominator = (d1.Denominator / g2) * (d2.Denominator / g1);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(int d1, Fraction d2)
        {
            var g1 = GreatestCommonDivisor(d1, d2.Denominator);
            var numerator = (d1 / g1) * d2.Numerator;
            var denominator = d2.Denominator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(Fraction d1, int d2)
        {
            var g1 = GreatestCommonDivisor(d1.Denominator, d2);
            var numerator = d1.Numerator * (d2 / g1);
            var denominator = d1.Denominator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(Fraction d1, Fraction d2)
        {
            var g1 = GreatestCommonDivisor(d1.Numerator, d2.Numerator);
            var g2 = GreatestCommonDivisor(d1.Denominator, d2.Denominator);
            var numerator = (d1.Numerator / g1) * (d2.Denominator / g2);
            var denominator = (d1.Denominator / g2) * (d2.Numerator / g1);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(int d1, Fraction d2)
        {
            var g1 = GreatestCommonDivisor(d1, d2.Numerator);
            var numerator = (d1 / g1) * d2.Denominator;
            var denominator = d2.Numerator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(Fraction d1, int d2)
        {
            var g1 = GreatestCommonDivisor(d1.Numerator, d2);
            var numerator = d1.Numerator / g1;
            var denominator = d1.Denominator * (d2 / g1);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static explicit operator int(Fraction v)
        {
            var d = (double) v.Numerator / v.Denominator;
            return (int) d;
        }

        public static explicit operator double(Fraction v)
        {
            var d = (double) v.Numerator / v.Denominator;
            return d;
        }

        public static int GreatestCommonDivisor(int a, int b)
        {
            while (true)
            {
                if (a == 0) return b;
                if (b == 0) return a;
                var r = a % b;

                a = b;
                b = r;
            }
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Fraction that:
                    return ((double) this).CompareTo((double) that);
                default:
                    throw new ArgumentException("Object is not a Fraction");
            }
        }
    }
}