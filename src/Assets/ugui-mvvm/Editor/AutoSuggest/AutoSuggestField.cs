using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoSuggest
{
    /// <summary>
    /// A specialized version of a TextField that also includes a non-focused (but still invocable)
    /// drop-down, populated by the passed in ISuggestionProvider.  Supports arrow/enter keys and mouse clicks.
    /// </summary>
    public class AutoSuggestField
    {
        /// <summary>
        /// Options for configuring behaviors of the <see cref="AutoSuggestField"/>.
        /// </summary>
        public class Options
        {
            /// <summary>
            /// How should the control display when showing suggestions?  Inline or Overlay?
            /// If overlay, then the caller must call OnGUISecondRenderPass after subsequent controls render themselves.
            /// </summary>
            public DisplayMode DisplayMode { get; set; } = DisplayMode.Inline;

            /// <summary>
            /// How many suggestions to show in the auto-suggest drop down.
            /// </summary>
            public int MaxSuggestionsToDisplay { get; set; } = 7;
        }

        private const int _scrollDistance = 3;
        private const string _keyFieldNamePrefix = "AutoSuggestField";
        private static int _controlCount = 0;

        private static GUIStyle _dropDownBoxStyle = null;
        private static GUIStyle _itemStyle = null;
        private static GUIStyle _selectedItemStyle = null;
        private static Vector2 _scrollbarSize;

        private List<Suggestion> _cachedSuggestions = new List<Suggestion>();
        private string _textForCachedSuggestions = null;
        private bool _focusedForCachedSuggestions = false;
        private bool _cacheInvalid = false;
        private readonly object _cacheInvalidationLock = new object();
        private ISuggestionProvider _suggestionProvider;
        private readonly GUIContent _label;
        private ValueAnimator _heightAnimator = new ValueAnimator(0.0f, 0.5f);
        private DrawSpaceClaimer _drawSpaceClaimer;
        private Options _options;
        private int _selectedIndex = 0;
        private int _scrolledIndex = 0;
        private bool _isFocused = false;
        private bool _setSelectedSuggestionToTextField = false;
        private readonly int _controlId;
        private Rect _textFieldPosition;
        private bool _prevRenderWasFirstPass = false;
        private bool _haveWarnedAboutRenderPassError = false;

        private bool ThreadSafeCacheInvalid
        {
            get
            {
                lock (_cacheInvalidationLock)
                {
                    return _cacheInvalid;
                }
            }

            set
            {
                lock (_cacheInvalidationLock)
                {
                    _cacheInvalid = value;
                }
            }
        }

        /// <summary>
        /// Auto-suggest field for Unity Editor.
        /// </summary>
        /// <param name="suggestionProvider">Provides the list of suggestions to show based on what the user typed.</param>
        /// <param name="label">The label to render with the control.</param>
        /// <param name="options">Configuration options for the behaviors of this control.</param>
        public AutoSuggestField(ISuggestionProvider suggestionProvider, GUIContent label, Options options)
        {
            _suggestionProvider = suggestionProvider;
            _label = label;
            _options = options;
            _drawSpaceClaimer = new DrawSpaceClaimer(_options.DisplayMode);

            _suggestionProvider.SuggestionsChanged += SuggestionProvider_SuggestionsChanged;
            EditorApplication.update += EditorApplication_Update;

            _controlId = _controlCount++;
        }

        /// <summary>
        /// Renders the control at the current location in the layout, akin to EditorGUILayout calls.
        /// </summary>
        /// <param name="text">The value to populate in the TextField portion of the control.</param>
        /// <returns>The new value of the text field based on user input in the text field or in the dropdown suggestion list.</returns>
        public string OnGUI(string text)
        {
            const bool isFirstRenderPass = true;
            EnforceRenderPassOrdering(isFirstRenderPass);

            if (Event.current.type == EventType.KeyDown && _isFocused && _cachedSuggestions.Any())
            {
                if (Event.current.keyCode == KeyCode.Return)
                {
                    SetCurrentSelectedIndexToTextField();
                }
                else
                {
                    OnKeyPressed();
                }
            }

            string controlName = _keyFieldNamePrefix + _controlId;

            if (_setSelectedSuggestionToTextField && _cachedSuggestions.Any())
            {
                // When setting the text on the control, in order to get it to render correctly, we must set focus off of it, render it, then set focus back on it again.
                text = _cachedSuggestions[_selectedIndex].Value;
            }

            // Draw the text field
            // Note: We assign a name to the field so we can later check if it is focused
            string newText;
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                GUI.SetNextControlName(controlName);
                newText = EditorGUILayout.TextField(_label, text);
            }

            if (Event.current.type == EventType.Repaint)
            {
                // The name of the focused control is not set properly on Layout.  Wait until Repaint to check.
                _isFocused = (GUI.GetNameOfFocusedControl() == controlName);
            }

            if (_setSelectedSuggestionToTextField)
            {
                GUI.FocusControl(controlName);
                _isFocused = true;
                _setSelectedSuggestionToTextField = false;
            }

            if (newText != _textForCachedSuggestions
                || _isFocused != _focusedForCachedSuggestions
                || ThreadSafeCacheInvalid)
            {
                if (Event.current.type == EventType.Layout)
                {
                    _textForCachedSuggestions = newText;
                    _focusedForCachedSuggestions = _isFocused;
                    ThreadSafeCacheInvalid = false;

                    var suggestions = _suggestionProvider.GetSuggestions(newText, _isFocused);
                    _cachedSuggestions = (suggestions != null) ? suggestions.ToList() : new List<Suggestion>();
                    _selectedIndex = 0;
                    _scrolledIndex = 0;
                }
                else
                {
                    EditorWindow.focusedWindow?.Repaint();
                }
            }

            // Capture the position of the text field for later rendering of the drop-down
            _textFieldPosition = EditorGUILayout.GetControlRect(false, 0);

            // Draw the auto-suggestion overlay.
            DrawAutoSuggestionOverlay(_textFieldPosition, isFirstRenderPass);

            return newText;
        }

        /// <summary>
        /// When Options.FillMode is SpaceFillMode.TakeSpaceIfNeeded, you must call OnGUI, then render other controls in the pane, then call OnGUISecondPass every frame.
        /// Otherwise, you do not need to call OnGUISecondPass.
        /// </summary>
        public void OnGUISecondPass()
        {
            const bool isFirstRenderPass = false;
            EnforceRenderPassOrdering(isFirstRenderPass);

            if (_options.DisplayMode == DisplayMode.Overlay)
            {
                DrawAutoSuggestionOverlay(_textFieldPosition, isFirstRenderPass);
            }
        }

        private void DrawAutoSuggestionOverlay(Rect textFieldPosition, bool isFirstRenderPass)
        {
            // If styles haven't been computed yet, do so now
            CreateStylesIfNeeded();

            var suggestionButtonHeight = EditorGUIUtility.singleLineHeight;
            var suggestionButtonHeightWithSpacing = suggestionButtonHeight + EditorGUIUtility.standardVerticalSpacing;
            var errorHeight = 2.0f * EditorGUIUtility.singleLineHeight;
            var errorHeightWithSpacing = errorHeight + EditorGUIUtility.standardVerticalSpacing;

            int numberToDisplay = Math.Min(_cachedSuggestions.Count, _options.MaxSuggestionsToDisplay);

            float height = (_cachedSuggestions.Any()) ? EditorGUIUtility.standardVerticalSpacing : 0.0f;
            // Determine height based on first N items.
            for (int i = 0; i < numberToDisplay; i++)
            {
                if (_cachedSuggestions[i].IsErrorMessage)
                {
                    height += errorHeightWithSpacing;
                }
                else
                {
                    height += suggestionButtonHeightWithSpacing;
                }
            }

            Rect currentPosition = new Rect(
                x: textFieldPosition.x + EditorGUIUtility.labelWidth,
                y: textFieldPosition.y - EditorGUIUtility.standardVerticalSpacing,
                width: Math.Max(0, textFieldPosition.width - EditorGUIUtility.labelWidth),
                height: height);

            if (Event.current.isScrollWheel && currentPosition.Contains(Event.current.mousePosition))
            {
                if (Event.current.delta.y > 0)
                {
                    _scrolledIndex += _scrollDistance;
                }
                else
                {
                    _scrolledIndex -= _scrollDistance;
                }

                ClampScrolledIndex(false);
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint)
            {
                _heightAnimator.Target = currentPosition.height;
            }

            _drawSpaceClaimer.ClaimDrawSpace(isFirstRenderPass, _heightAnimator.Current);
            currentPosition.height = _heightAnimator.Current;

            if (currentPosition.height > _scrollbarSize.y * 2
                && _cachedSuggestions.Count > _options.MaxSuggestionsToDisplay)
            {
                var barPosition = new Rect(
                    currentPosition.xMax - _scrollbarSize.x,
                    currentPosition.y,
                    _scrollbarSize.x,
                    currentPosition.height);

                EditorGUIUtility.AddCursorRect(barPosition, MouseCursor.Arrow);

                float scrollMin = 0.0f;
                float scrollMax = _cachedSuggestions.Count;

                _scrolledIndex = (int)GUI.VerticalScrollbar(barPosition, _scrolledIndex, _options.MaxSuggestionsToDisplay, scrollMin, scrollMax, GUI.skin.verticalScrollbar);
                ClampScrolledIndex(false);
                currentPosition.width -= _scrollbarSize.x;
            }

            using (var scrollScope = new GUI.ScrollViewScope(currentPosition, Vector2.zero, currentPosition, false, false, GUIStyle.none, GUIStyle.none))
            {
                GUI.Box(currentPosition, string.Empty, _dropDownBoxStyle);
                scrollScope.handleScrollWheel = true;
                EditorGUIUtility.AddCursorRect(currentPosition, MouseCursor.Arrow);

                // Draw each of the suggestions
                currentPosition.y += EditorGUIUtility.standardVerticalSpacing;
                currentPosition.height = suggestionButtonHeight;
                int maxRemainingSuggestionsToDisplay = _options.MaxSuggestionsToDisplay;

                for (int i = _scrolledIndex; i < _cachedSuggestions.Count; i++)
                {
                    var suggestion = _cachedSuggestions[i];
                    var style = _itemStyle;

                    if (i == _selectedIndex)
                    {
                        style = _selectedItemStyle;
                    }

                    if (suggestion.IsErrorMessage)
                    {
                        currentPosition.height = errorHeight;

                        EditorGUI.HelpBox(currentPosition, suggestion.DisplayText, MessageType.Error);

                        currentPosition.y += errorHeightWithSpacing;
                    }
                    else
                    {
                        currentPosition.height = suggestionButtonHeight;

                        if (GUI.Button(currentPosition, suggestion.RichDisplayText, style))
                        {
                            _selectedIndex = i;
                            SetCurrentSelectedIndexToTextField();
                        }

                        currentPosition.y += suggestionButtonHeightWithSpacing;
                    }

                    if (--maxRemainingSuggestionsToDisplay == 0)
                    {
                        break;
                    }
                }
            }
        }

        private void SetCurrentSelectedIndexToTextField()
        {
            if (!_cachedSuggestions[_selectedIndex].IsErrorMessage)
            {
                // To get the TextField to render correctly, we must set focus away from it,
                // then render it with the new text, then set focus back to it.
                _setSelectedSuggestionToTextField = true;
                GUI.FocusControl("");
            }
        }

        private void SuggestionProvider_SuggestionsChanged()
        {
            ThreadSafeCacheInvalid = true;
        }

        private void EditorApplication_Update()
        {
            if (_heightAnimator.Update() || ThreadSafeCacheInvalid)
            {
                // If the height animator is still moving or there is a pending cache invalidation, trigger a repaint.
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        private void OnKeyPressed()
        {
            var current = Event.current;

            if (current.keyCode == KeyCode.UpArrow)
            {
                _selectedIndex--;
            }
            else if (current.keyCode == KeyCode.DownArrow)
            {
                _selectedIndex++;
            }
            else
            {
                // Some other key was pressed.
                return;
            }

            current.Use();
            ClampSelectedIndex();
            ClampScrolledIndex(true);
        }

        private void ClampSelectedIndex()
        {
            _selectedIndex = MathUtils.Clamp(_selectedIndex, 0, _cachedSuggestions.Count() - 1);
        }

        private void ClampScrolledIndex(bool scrollSelectedItemIntoView)
        {
            if (scrollSelectedItemIntoView)
            {
                var maxScrolledIndex = _selectedIndex;
                var minScrolledIndex = _selectedIndex - (_options.MaxSuggestionsToDisplay - 1);
                _scrolledIndex = MathUtils.Clamp(_scrolledIndex, minScrolledIndex, maxScrolledIndex);
            }

            if (_cachedSuggestions.Count < _options.MaxSuggestionsToDisplay)
            {
                // Only one "page" of results to display.  Don't allow scrolling.
                _scrolledIndex = 0;
            }
            else
            {
                _scrolledIndex = MathUtils.Clamp(_scrolledIndex, 0, _cachedSuggestions.Count - _options.MaxSuggestionsToDisplay);
            }
        }

        private void CreateStylesIfNeeded()
        {
            if (_dropDownBoxStyle == null)
            {
                _dropDownBoxStyle = GUI.skin.textField;
                _itemStyle = new GUIStyle(GUI.skin.label);
                _itemStyle.richText = true;

                _selectedItemStyle = new GUIStyle(_itemStyle);

                var selectedItemBackgroundTexture = new Texture2D(1, 1);
                selectedItemBackgroundTexture.SetPixel(0, 0, GUI.skin.settings.selectionColor);
                selectedItemBackgroundTexture.Apply();
                _selectedItemStyle.normal.background = selectedItemBackgroundTexture;

                _scrollbarSize = GUI.skin.verticalScrollbar.CalcSize(new GUIContent(""));
            }
        }

        /// <summary>
        /// Call this function exactly once per OnGUI and once per OnGUISecondPass
        /// </summary>
        /// <param name="isFirstRenderPass"></param>
        private void EnforceRenderPassOrdering(bool isFirstRenderPass)
        {
            if (_haveWarnedAboutRenderPassError)
            {
                // Already gave a warning/error.  No need to spam it every frame.
                return;
            }

            if (_options.DisplayMode == DisplayMode.Overlay)
            {
                if (_prevRenderWasFirstPass == isFirstRenderPass)
                {
                    // Called the same render pass twice in a row.  They should call them in an alternating order.
                    string currentPassName = (isFirstRenderPass) ? nameof(OnGUI) : nameof(OnGUISecondPass);
                    Debug.LogError($"When using AutoSuggestField in Overlay mode, you must call OnGUI, then render other controls in the pane, then call OnGUISecondPass.  " +
                        $"You have called {currentPassName} twice in a row.");
                    _haveWarnedAboutRenderPassError = true;
                }
            }
            else
            {
                // Second pass is not required
                if (!isFirstRenderPass)
                {
                    Debug.LogWarning("When using AutoSuggestField in Inline mode, there is no need to call OnGUISecondPass()");
                    _haveWarnedAboutRenderPassError = true;
                }
            }

            _prevRenderWasFirstPass = isFirstRenderPass;
        }
    }
}
