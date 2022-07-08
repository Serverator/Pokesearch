using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Simple math static class, just because I like creating them
/// </summary>
public static class Maths
{
    public static byte Min(byte a, byte b) => a < b ? a : b;
    public static byte Max(byte a, byte b) => a > b ? a : b;

    public static float Min(float a, float b) => a < b ? a : b;
    public static float Max(float a, float b) => a > b ? a : b;

    public static int Min(int a, int b) => a < b ? a : b;
    public static int Max(int a, int b) => a > b ? a : b;

    public static int Length(this int number)
    {
        int i = 1;
        while (number >= 10)
        {
            number /= 10;
            i++;
        }
        return i;
    }

    /// <summary>
    /// Converts value into "Entropy" from information theory
    /// </summary>
    /// <param name="allStates"> All number states </param>
    /// <param name="knownStates"> Known states </param>
    /// <returns> Entropy value </returns>
    public static float ToEntropy(float allStates, float knownStates = 1) => knownStates == 0 ? 0 : (float)-Math.Log2((double)knownStates / allStates);

    /// <summary>
    /// Converts from "Entropy" to number of states
    /// </summary>
    public static float FromEntropy(float entropy) => (float)Math.Pow(2, entropy);

    public static bool AproxEqual(float a, float b) => Math.Abs(a - b) < 0.001f;
}
