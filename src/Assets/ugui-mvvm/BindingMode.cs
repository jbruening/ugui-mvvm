using System;
using System.ComponentModel;
using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Describes the direction of the data flow in a binding.
    /// </summary>
    public enum BindingMode
    {
        /// <summary>Updates the binding target when the application starts or when the data context changes.</summary>
        OneTime,
        /// <summary>Updates the target property when the source property changes.</summary>
        OneWayToTarget,
        /// <summary>Updates the source property when the target property changes.</summary>
        OneWayToSource,
        /// <summary>Causes changes to either the source property or the target property to automatically update the other.</summary>
        TwoWay,

        /// <summary>
        /// Obsolete value. Use <see cref="OneWayToTarget"/> instead.
        /// </summary>
        /// <remarks>Only here to maintain backwards compatibility.</remarks>
        [Obsolete("Replace OneWayToView with OneWayToTarget.")]
        [HideInInspector]
        [EditorBrowsable(EditorBrowsableState.Never)]
        OneWayToView = OneWayToTarget,

        /// <summary>
        /// Obsolete value. Use <see cref="OneWayToSource"/> instead.
        /// </summary>
        /// <remarks>Only here to maintain backwards compatibility.</remarks>
        [Obsolete("Replace OneWayToViewModel with OneWayToSource.")]
        [HideInInspector]
        [EditorBrowsable(EditorBrowsableState.Never)]
        OneWayToViewModel = OneWayToSource,
    }

    /// <summary>
    /// Helper methods for performing additional operations on <see cref="BindingMode"/> values.
    /// </summary>
    public static class BindingModeExtensions
    {
        /// <summary>
        /// Checks if the given <see cref="BindingMode"/> includes a binding that comes from the source, to the target.
        /// </summary>
        /// <param name="bindingMode">The <see cref="BindingMode"/> to evaluate.</param>
        /// <returns><c>true</c> if the <see cref="BindingMode"/> includes updating the target, otherwise <c>false</c>.</returns>
        public static bool IsTargetBoundToSource(this BindingMode bindingMode)
        {
            return bindingMode == BindingMode.OneWayToTarget || bindingMode == BindingMode.TwoWay;
        }

        /// <summary>
        /// Checks if the given <see cref="BindingMode"/> includes a binding that comes from the target, to the source.
        /// </summary>
        /// <param name="bindingMode">The <see cref="BindingMode"/> to evaluate.</param>
        /// <returns><c>true</c> if the <see cref="BindingMode"/> includes updating the source, otherwise <c>false</c>.</returns>
        public static bool IsSourceBoundToTarget(this BindingMode bindingMode)
        {
            return bindingMode == BindingMode.OneWayToSource || bindingMode == BindingMode.TwoWay;
        }
    }
}
