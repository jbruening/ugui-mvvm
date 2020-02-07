using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uguimvvm
{
    /// <summary>
    /// Provides functionality for handling values that may be ready now, or possibly not till later.
    /// </summary>
    public interface IDelayedValue
    {
        /// <summary>
        /// Get the value if it is ready, or subscribe to receive it (if it is not ready).
        /// </summary>
        /// <remarks>This is supposed to act as an atomic operation. It should never both subscribe and return true, nor should it return false and not subscribe.
        /// Additionally, this should dispose of <paramref name="whenReady"/> once it completes, if not ready now</remarks>
        /// <param name="whenReady">A callback to be invoked when the value becomes ready, if the value is not ready now.</param>
        /// <param name="readyValue">The value, if it's ready now.</param>
        /// <returns><c>true</c>, if the value was ready now, and applied to <paramref name="readyValue"/>. <c>false</c> if not ready and <paramref name="whenReady"/> was subscribed.</returns>
        bool ValueOrSubscribe(Action<object> whenReady, ref object readyValue);
    }
}
