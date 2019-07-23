using UnityEngine;

namespace AutoSuggest
{
    public class ValueAnimator
    {
        private float _smoothTime;
        private float _velocity;

        public ValueAnimator(float initialValue, float smoothTime)
        {
            Current = initialValue;
            Target = initialValue;
            _smoothTime = smoothTime;
        }

        public float Current { get; private set; }
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
