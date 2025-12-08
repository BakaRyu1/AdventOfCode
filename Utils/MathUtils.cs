using System.Numerics;

namespace AdventOfCode.Utils;

internal static class MathUtils
{
    public static T GreatestCommonDenominator<T>(T a, T b) where T : INumber<T>
    {
        while (b != T.Zero)
        {
            var tmp = b;
            b = a % b;
            a = tmp;
        }
        return a;
    }
    public static T LowestCommonMultiplicator<T>(T a, T b) where T : INumber<T> => (a / GreatestCommonDenominator(a, b)) * b;
    public static T IntPow<T>(T x, uint pow) where T : INumber<T>
    {
        T ret = T.CreateChecked(1);
        while (pow != 0)
        {
            if ((pow & 1) == 1)
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }
    private static void AddDivisors<T>(ref T num, T divisor, List<T> divisors) where T : IBinaryInteger<T>
    {
        var divRem = T.DivRem(num, divisor);
        while (T.IsZero(divRem.Remainder))
        {
            divisors.Add(divisor);
            num = divRem.Quotient;
            divRem = T.DivRem(num, divisor);
        }
    }
    public static List<T> GetPrimeDivisors<T>(T num) where T : IBinaryInteger<T>
    {
        var divisors = new List<T>();
        var two = T.CreateChecked(2);
        AddDivisors(ref num, two, divisors);
        var three = T.CreateChecked(3);
        AddDivisors(ref num, three, divisors);
        var i = T.CreateChecked(5);
        var six = T.CreateChecked(6);
        while (i * i <= num)
        {
            AddDivisors(ref num, i, divisors);
            AddDivisors(ref num, i + two, divisors);
            i += six;
        }
        if (num > T.One)
            divisors.Add(num);
        return divisors;
    }
    public static List<T> GetDivisors<T>(T num) where T : IBinaryInteger<T>
    {
        if (num == T.One)
            return [T.One];
        var divisors = new List<T>();
        if (num > T.One)
        {
            var primeFactors = GetPrimeDivisors(num);
            divisors.Add(T.One);
            var lastPrime = T.Zero;
            var factor = T.Zero;
            var sliceLen = 0;
            foreach (var prime in primeFactors)
            {
                if (lastPrime != prime)
                {
                    sliceLen = divisors.Count;
                    factor = prime;
                }
                else
                    factor *= prime;
                for (var i = 0; i < sliceLen; ++i)
                    divisors.Add(divisors[i] * factor);
                lastPrime = prime;
            }
        }
        return divisors;
    }
}
