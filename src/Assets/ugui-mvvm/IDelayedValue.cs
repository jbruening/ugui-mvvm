using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uguimvvm
{
    public interface IDelayedValue
    {
        /// <summary>
        /// get the value if its finished, or subscribe to receive it.
        /// </summary>
        /// <remarks>This is supposed to act as an atomic operation. It should never both subscribe and return true, nor should it return false and not subscribe.
        /// Additionally, this should dispose of whenready once it completes, if not ready now</remarks>
        /// <param name="whenReady"></param>
        /// <param name="readyValue">the value, if it's ready now</param>
        /// <returns>true, if the value was ready now, and applied to readyValue. False if not ready and whenReady was subscribed</returns>
        bool ValueOrSubscribe(Action<object> whenReady, ref object readyValue);
    }
}
