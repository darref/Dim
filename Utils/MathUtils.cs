using System;

namespace Dim.Utils;

public static class MathUtils
{
    public static bool IsPowerOfTwo(double n)
    {
        if (n <= 0) return false;
        double logResult = Math.Log(n, 2);
        return Math.Abs(logResult - Math.Round(logResult)) < 1e-10;
    }
}