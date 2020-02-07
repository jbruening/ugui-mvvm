using System.Diagnostics;

namespace AutoSuggest
{
    /// <summary>
    /// Collection of utility functions for doing mathematical operations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Clamps the given value to within the specified range.
        /// </summary>
        /// <param name="i">The starting value to ensure falls within the specified range.</param>
        /// <param name="min">The minimum value to return.</param>
        /// <param name="max">The maximum value to return.</param>
        /// <returns>
        /// The given value if it is within the range of <c>min</c> to <c>max</c>.
        /// Returns <c>min</c> if the given value is less than <c>min</c>.
        /// Returns <c>max</c> if the given value is greater than <c>max</c>.
        /// </returns>
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
