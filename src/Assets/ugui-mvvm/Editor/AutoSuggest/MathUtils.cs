using System.Diagnostics;

namespace AutoSuggest
{
    public static class MathUtils
    {
        public static int Clamp(int i, int min, int max)
        {
            Debug.Assert(min < max);

            if (i < min)
            {
                return min;
            }
            else if (i > max)
            {
                return max;
            }
            else
            {
                return i;
            }
        }
    }
}
