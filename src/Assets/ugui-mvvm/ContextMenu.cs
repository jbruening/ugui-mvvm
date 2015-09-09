using UnityEngine;

namespace uguimvvm
{
    public class ContextMenu : MonoBehaviour
    {
        [SerializeField] 
        private float _keepAlive = 1f;

        private RectTransform _rect;
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private float _dieTime;

        private void Awake()
        {
            _rect = transform as RectTransform;
        }

        //todo: hide if user clicks on some other control, rather than/or timeout
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

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            Show(Input.mousePosition, null);
        }

        public void Show(Vector2 position, Component dataContext)
        {
            _rect.position = new Vector3(position.x - 1, position.y + 1, _rect.position.z);

            RefreshDieTime();
            gameObject.SetActive(true);

            var context = GetComponent<DataContext>();
            if (context != null && dataContext != null)
                context.UpdateValue(dataContext is DataContext ? (dataContext as DataContext).Value : dataContext);
        }

        private bool ShouldHide()
        {
            return _dieTime <= Time.time;
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