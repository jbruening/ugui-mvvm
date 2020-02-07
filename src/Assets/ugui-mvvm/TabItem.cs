using UnityEngine;
using UnityEngine.UI;

namespace uguimvvm
{
    /// <summary>
    /// Represents a control that displays a single tab in a <see cref="TabControl"/>.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/Tabs/TabItem", 2)]
    public class TabItem : Button
    {
        private TabControl _tabs;
        private Image _image;

        [SerializeField]
        private UnityEngine.Sprite _active;
        [SerializeField]
        private Color _activeColor = Color.white;

        private bool _isOnTab;

        /// <summary>
        /// Unity callback invoked when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            _image = GetComponent<Image>();
            _tabs = GetComponentInParent<TabControl>();

            if (_active == null)
                _active = spriteState.highlightedSprite;

            onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Unity callback invoked when the parent property of the transform has changed.
        /// </summary>
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            _tabs = GetComponentInParent<TabControl>();
        }

        private void OnClick()
        {
            _tabs.SelectTab(this);
            SetSelected(true);
        }

        /// <summary>
        /// Sets if this tab is currently selected.
        /// </summary>
        /// <param name="state"><c>true</c> if this tab should be considered selected, otherwise <c>false</c>.</param>
        public void SetSelected(bool state)
        {
            //we're getting information before awake?
            if (_image == null) return;

            _isOnTab = state;
            switch (transition)
            {
                case Transition.ColorTint:
                    if (state)
                        _image.CrossFadeColor(_activeColor, 0, true, true);
                    else
                        _image.CrossFadeColor(colors.normalColor, 0, true, true);
                    break;
                default:
                    _image.overrideSprite = state ? _active : null;
                    break;
            }
        }

        //the following two overridden methods are what are known to perform sprite swaps in selectable
        /// <summary>
        /// Transition the <see cref="Selectable"/> to the requested state.
        /// </summary>
        /// <param name="state">State to transition to.</param>
        /// <param name="instant">Should the transition occur instantly.</param>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition == Transition.ColorTint)
            {
                if (state == SelectionState.Normal && _isOnTab)
                    SetSelected(_isOnTab);
                if (state == SelectionState.Highlighted && _isOnTab)
                    SetSelected(_isOnTab);
            }
            else
                SetSelected(_isOnTab);
        }

        /// <summary>
        /// Clear any internal state from the <see cref="Selectable"/> (used when disabling).
        /// </summary>
        protected override void InstantClearState()
        {
            base.InstantClearState();

            SetSelected(_isOnTab);
        }
    }
}