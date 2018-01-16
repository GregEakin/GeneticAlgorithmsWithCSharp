using System;

namespace GeneticAlgorithms.LinearEquation
{
    public class Fraction : IComparable, IComparable<Fraction>
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
                    return CompareTo(that) == 0;
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

        public static Fraction operator -(Fraction f1)
        {
            return new Fraction(-f1.Numerator, f1.Denominator);
        }

        public static Fraction operator +(int f1, Fraction f2)
        {
            var fraction = new Fraction(f1 * f2.Denominator + f2.Numerator, f2.Denominator);
            return fraction;
        }

        public static Fraction operator +(Fraction f1, int f2)
        {
            var fraction = new Fraction(f1.Numerator + f2 * f1.Denominator, f1.Denominator);
            return fraction;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            var gcd = GreatestCommonDivisor(f1.Denominator, f2.Denominator);
            var numerator = f1.Numerator * (f2.Denominator / gcd) + f2.Numerator * (f1.Denominator / gcd);
            var denominator = f1.Denominator * (f2.Denominator / gcd);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator -(int f1, Fraction f2)
        {
            var numerator = f1 * f2.Denominator - f2.Numerator;
            var fraction = new Fraction(numerator, f2.Denominator);
            return fraction;
        }

        public static Fraction operator -(Fraction f1, int f2)
        {
            var numerator = f1.Numerator - f2 * f1.Denominator;
            var fraction = new Fraction(numerator, f1.Denominator);
            return fraction;
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            var gcd = GreatestCommonDivisor(f1.Denominator, f2.Denominator);
            var numerator = f1.Numerator * (f2.Denominator / gcd) - f2.Numerator * (f1.Denominator / gcd);
            var denominator = f1.Denominator * (f2.Denominator / gcd);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            var g1 = GreatestCommonDivisor(f1.Numerator, f2.Denominator);
            var g2 = GreatestCommonDivisor(f1.Denominator, f2.Numerator);
            var numerator = (f1.Numerator / g1) * (f2.Numerator / g2);
            var denominator = (f1.Denominator / g2) * (f2.Denominator / g1);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(int f1, Fraction f2)
        {
            var g1 = GreatestCommonDivisor(f1, f2.Denominator);
            var numerator = (f1 / g1) * f2.Numerator;
            var denominator = f2.Denominator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator *(Fraction f1, int f2)
        {
            var g1 = GreatestCommonDivisor(f1.Denominator, f2);
            var numerator = f1.Numerator * (f2 / g1);
            var denominator = f1.Denominator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            if (f2.Numerator == 0)
                throw new ArgumentException("Can't divide by zero!", nameof(f2));

            var g1 = GreatestCommonDivisor(f1.Numerator, f2.Numerator);
            var g2 = GreatestCommonDivisor(f1.Denominator, f2.Denominator);
            var numerator = (f1.Numerator / g1) * (f2.Denominator / g2);
            var denominator = (f1.Denominator / g2) * (f2.Numerator / g1);
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(int f1, Fraction f2)
        {
            if (f2.Numerator == 0)
                throw new ArgumentException("Can't divide by zero!", nameof(f2));

            var g1 = GreatestCommonDivisor(f1, f2.Numerator);
            var numerator = (f1 / g1) * f2.Denominator;
            var denominator = f2.Numerator / g1;
            var fraction = new Fraction(numerator, denominator);
            return fraction;
        }

        public static Fraction operator /(Fraction f1, int f2)
        {
            if (f2 == 0)
                throw new ArgumentException("Can't divide by zero!", nameof(f2));

            var g1 = GreatestCommonDivisor(f1.Numerator, f2);
            var numerator = f1.Numerator / g1;
            var denominator = f1.Denominator * (f2 / g1);
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
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fraction");
            }
        }

        public int CompareTo(Fraction that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            var denominatorComparison = Denominator.CompareTo(that.Denominator);
            if (denominatorComparison != 0) return denominatorComparison;
            return Numerator.CompareTo(that.Numerator);
        }
    }
}