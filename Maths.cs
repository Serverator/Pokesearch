using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Maths
{
    public static byte Min(byte a, byte b) => a < b ? a : b;
    public static byte Max(byte a, byte b) => a > b ? a : b;

    public static float Min(float a, float b) => a < b ? a : b;
    public static float Max(float a, float b) => a > b ? a : b;

    public static float ToEntropy(int guesses, double count = 1) => count == 0 ? 0 : (float)-Math.Log2((double)count / guesses);
    public static float FromEntropy(float entropy) => (float)Math.Pow(2,entropy);

    public static bool AproxEqual(float a, float b) => Math.Abs(a - b) < 0.001f;
}
