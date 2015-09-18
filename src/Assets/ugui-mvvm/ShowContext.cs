using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uguimvvm
{
    public class ShowContext : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] 
        private ContextMenu _menu = null;

        [SerializeField]
        [Tooltip("Show the menu when hovering")]
        private bool _hover = false;
        [SerializeField]
        [Tooltip("When Hover is true, show the context menu after the mouse has stopped moving for the specified time")]
        private float _hoverStopTime = 0.5f;
        [SerializeField]
        [Tooltip("Show the menu on left click")]
        private bool _leftClick = false;
        [SerializeField]
        [Tooltip("Show the menu on right click")]
        private bool _rightClick = true;

        [SerializeField]
        [Tooltip("Null will just use the first INotifyPropertyChanged object up the tree")] 
        private MonoBehaviour _dataContext = null;

        private Vector2 _lastPos;
        private bool _isOver;
        private float _lastStopTime;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_leftClick && eventData.button == PointerEventData.InputButton.Left)
            {
                Show(eventData.position);
            }
            else if (_rightClick && eventData.button == PointerEventData.InputButton.Right)
            {
                Show(eventData.position);
            }
        }

        private void Show(Vector2 position)
        {
            if (_menu == null) return;
            _menu.Show(position, _dataContext ?? gameObject.GetComponentInParent(typeof (INotifyPropertyChanged)));
            _isOver = false;
        }

        void Hide()
        {
            if (_menu == null) return;
            _menu.Hide();
        }

        void Update()
        {
            if (!_hover) return;
            if (!_isOver) return;

            Vector2 pos = Input.mousePosition;
            if (pos != _lastPos)
                _lastStopTime = Time.time;
            _lastPos = pos;


            if (Time.time - _lastStopTime >= _hoverStopTime)
            {
                Show(pos);
            }
            else
                Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hover) return;
            if (_menu == null) return;
            _isOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_hover) return;
            if (_menu == null) return;
            _isOver = false;
        }
    }
}