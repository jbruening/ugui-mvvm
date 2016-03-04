using UnityEngine;
using UnityEngine.UI;

namespace uguimvvm
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/Tabs/TabItem", 2)]
    public class TabItem : Button
    {
        private TabControl _tabs;
        private Image _image;

        [SerializeField]
        private UnityEngine.Sprite _active;

        private bool _isOnTab;

        protected override void Awake()
        {
            _image = GetComponent<Image>();
            _tabs = GetComponentInParent<TabControl>();

            if (_active == null)
                _active = spriteState.highlightedSprite;

            onClick.AddListener(OnClick);
        }

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

        public void SetSelected(bool state)
        {
            _isOnTab = state;
            switch (transition)
            {
                case Transition.ColorTint:
                    _image.CrossFadeColor(colors.highlightedColor, 0, true, true);
                    break;
                default:
                    _image.overrideSprite = state ? _active : null;
                    break;
            }
        }

        //the following two overridden methods are what are known to perform sprite swaps in selectable
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            SetSelected(_isOnTab);
        }

        protected override void InstantClearState()
        {
            base.InstantClearState();

            SetSelected(_isOnTab);
        }
    }
}