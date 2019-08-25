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

    public static class BindingModeExtensions
    {
        public static bool IsTargetBoundToSource(this BindingMode bindingMode)
        {
            return bindingMode != BindingMode.OneWayToSource;
        }

        public static bool IsSourceBoundToTarget(this BindingMode bindingMode)
        {
            return bindingMode == BindingMode.OneWayToSource || bindingMode == BindingMode.TwoWay;
        }
    }
}
