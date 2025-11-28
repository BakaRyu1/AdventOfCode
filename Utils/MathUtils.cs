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
}
