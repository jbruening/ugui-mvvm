using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uguimvvm
{
    public class ShowContext : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] 
        private ContextMenu _menu = null;

        [SerializeField]
        [Tooltip("Null will just use the first INotifyPropertyChanged object up the tree")] 
        private MonoBehaviour _dataContext = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Show(eventData.position);
            }
        }

        private void Show(Vector2 position)
        {
            if (_menu != null)
            {
                _menu.Show(position, _dataContext ?? gameObject.GetComponentInParent(typeof (INotifyPropertyChanged)));
            }
        }
    }
}