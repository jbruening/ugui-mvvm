using System;
using System.ComponentModel;
using UnityEngine;

namespace uguimvvm
{
    public enum BindingMode
    {
        OneTime,
        OneWayToTarget,
        OneWayToSource,
        TwoWay,

        // Obsolete values.  Only here to maintain backwards compatibility.
        [Obsolete]
        [HideInInspector]
        [EditorBrowsable(EditorBrowsableState.Never)]
        OneWayToView = OneWayToTarget,

        [Obsolete]
        [HideInInspector]
        [EditorBrowsable(EditorBrowsableState.Never)]
        OneWayToViewModel = OneWayToSource,
    }
}
