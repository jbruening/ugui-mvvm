using UnityEngine;

namespace AutoSuggest
{
    /// <summary>
    /// Class for smoothly animating between two values.
    /// </summary>
    public class ValueAnimator
    {
        private readonly float _smoothTime;
        private float _velocity;

        /// <summary>
        /// Class for smoothly animating between two values.
        /// </summary>
        /// <param name="initialValue">The initial value to use as both <see cref="Current"/> and <see cref="Target"/>.</param>
        /// <param name="smoothTime">The approximate amount of time to take for <see cref="Current"/> to reach <see cref="Target"/>.</param>
        public ValueAnimator(float initialValue, float smoothTime)
        {
            Current = initialValue;
            Target = initialValue;
            _smoothTime = smoothTime;
        }

        /// <summary>
        /// The current animated value.
        /// </summary>
        public float Current { get; private set; }

        /// <summary>
        /// The value to animate <see cref="Current"/> towards.
        /// </summary>
        public float Target { get; set; }

        /// <summary>
        /// Animates Current to be closer to Target.
        /// </summary>
        /// <returns>True if Current changed.</returns>
        public bool Update()
        {
            var prev = Current;
            Current = Mathf.SmoothDamp(Current, Target, ref _velocity, _smoothTime);

            return !Mathf.Approximately(prev, Current);
        }
    }
}
