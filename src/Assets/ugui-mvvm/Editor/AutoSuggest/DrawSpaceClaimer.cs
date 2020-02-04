using UnityEditor;
using UnityEngine;

namespace AutoSuggest
{
    /// <summary>
    /// Class for dynamically reserving space in the <see cref="EditorGUILayout"/> control layout.
    /// </summary>
    public class DrawSpaceClaimer
    {
        private readonly DisplayMode _displayMode;
        private float _drawLocation;
        private float _additionDrawSpaceToClaim;
        private bool _hasShownError = false;
        private bool _hasRenderedFirstPass = false;
        private bool _hasRenderedSecondPass = false;

        /// <summary>
        /// Class for dynamically reserving space in the <see cref="EditorGUILayout"/> control layout.
        /// </summary>
        /// <param name="displayMode">The kind of display methodology for which space should be reserved.</param>
        public DrawSpaceClaimer(DisplayMode displayMode)
        {
            _displayMode = displayMode;
        }

        /// <summary>
        /// Reserves space (if needed) in the <see cref="EditorGUILayout"/> control layout.
        /// </summary>
        /// <param name="isFirstRenderPass">Flag indicating if this is the first/default/normal render pass, or instead a subsequent render pass executing outside of the normal ordering.</param>
        /// <param name="desiredHeight">The amount of vertical space that should be reserved.</param>
        public void ClaimDrawSpace(bool isFirstRenderPass, float desiredHeight)
        {
            if (_displayMode == DisplayMode.Inline)
            {
                // In inline mode, claim the space on the first render pass.
                if (isFirstRenderPass)
                {
                    EditorGUILayout.GetControlRect(false, desiredHeight);
                }
            }
            else if (_displayMode == DisplayMode.Overlay)
            {
                // In overlay mode, claim the space on the second render pass if we need to.
                if (isFirstRenderPass)
                {
                    if (_hasRenderedFirstPass && !_hasRenderedSecondPass && !_hasShownError)
                    {
                        _hasShownError = true;
                        Debug.LogError("Attempting to display Unity Editor UI that only takes up space if needed.  " +
                            "This requires that the caller call ClaimDrawSpace a second time each OnGUI with isFirstRenderPass set to false.  " +
                            "Either switch to DisplayMode.Inline or call ClaimDrawSpace a second time.");
                    }

                    // On first pass, store location of where we started
                    _drawLocation = EditorGUILayout.GetControlRect(false, 0.0f).y;
                    _hasRenderedFirstPass = true;
                }
                else
                {
                    var currentDrawLocation = EditorGUILayout.GetControlRect(false, 0.0f).y;
                    var currentHeight = currentDrawLocation - _drawLocation;

                    // Layout passes return all 0's in calls to GetControlRect.
                    bool isRealRenderPass =
                        Event.current.type != EventType.Layout &&
                        Event.current.type != EventType.Used;

                    if (isRealRenderPass && currentHeight < desiredHeight)
                    {
                        // Current height of panel is smaller than required.  Increase by the difference.
                        _additionDrawSpaceToClaim = desiredHeight - currentHeight;
                    }

                    EditorGUILayout.GetControlRect(false, _additionDrawSpaceToClaim);
                    _hasRenderedSecondPass = true;
                }
            }
        }
    }
}
