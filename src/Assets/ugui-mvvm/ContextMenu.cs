using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Represents a control that displays a transient piece of UI, intended for use with a list of contextual commands.
    /// </summary>
    public class ContextMenu : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Timeout of the menu if Hide When Timeout is selected")]
        private float _keepAlive = 1f;
        [SerializeField]
        [Tooltip("Hide the menu if the mouse isn't over the menu after the Keep Alive time")]
        private bool _hideWhenTimeout = false;
        [SerializeField]
        [Tooltip("Hide the menu if the user clicks off the menu")]
        private bool _hideWhenOffClick = true;

        private RectTransform _rect;
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private float _dieTime;

        /// <summary>
        /// Flag indicating that the menu is currently showing in the scene.
        /// </summary>
        public bool IsVisible { get { return gameObject.activeSelf; } }

        private void Awake()
        {
            _rect = transform as RectTransform;
        }

        private void Update()
        {
            if (!IsMouseOverRect())
            {
                if (ShouldHide())
                    Hide();
                return;
            }

            RefreshDieTime();
        }

        /// <summary>
        /// Hides the menu.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the menu at the current mouse position.
        /// </summary>
        public void Show()
        {
            Show(Input.mousePosition, null);
        }

        /// <summary>
        /// Shows the menu at the given position.
        /// </summary>
        /// <param name="position">The point the menu should be placed relative to.</param>
        /// <param name="dataContext">
        /// The value to use as the root <see cref="DataContext"/> for bindings in this control.
        /// If <paramref name="dataContext"/> is a <see cref="DataContext"/> its <see cref="DataContext.Value"/> will be used for the root <see cref="DataContext"/>'s <see cref="DataContext.Value"/>.
        /// Otherwise <paramref name="dataContext"/> will be used directly as the root <see cref="DataContext"/>'s <see cref="DataContext.Value"/>.
        /// </param>
        public void Show(Vector2 position, Component dataContext)
        {
            if (IsVisible) return;

            _rect.position = new Vector3(position.x - 1, position.y + 1, _rect.position.z);

            RefreshDieTime();
            gameObject.SetActive(true);

            var context = GetComponent<DataContext>();
            if (context != null && dataContext != null)
                context.UpdateValue(dataContext is DataContext ? (dataContext as DataContext).Value : dataContext);
        }

        private bool ShouldHide()
        {
            if (_hideWhenOffClick && Input.GetMouseButtonUp(0))
                return true;
            if (_hideWhenTimeout)
                return _dieTime <= Time.time;
            return false;
        }

        private void RefreshDieTime()
        {
            _dieTime = Time.time + _keepAlive;
        }

        private bool IsMouseOverRect()
        {
            Vector2 mousePosition = Input.mousePosition;
            _rect.GetWorldCorners(_worldCorners);

            return mousePosition.x >= _worldCorners[0].x && mousePosition.x < _worldCorners[2].x
                   && mousePosition.y >= _worldCorners[0].y && mousePosition.y < _worldCorners[2].y;
        }
    }
}