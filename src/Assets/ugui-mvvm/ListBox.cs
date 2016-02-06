using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace uguimvvm
{
    class ListBox : ItemsControl
    {        
        private GameObject _lastSelected;
        private static Func<ListBoxItem, bool> _selectionState = s => s == null ? false : s.IsSelected();

        private ItemInfo _selectedInfo;
        public object Selected
        {
            get { return _selectedInfo == null ? null : _selectedInfo.Item; }
            set
            {
                ItemInfo info;
                if (value == null) //because null always means 'no selected object'
                    info = null;
                else
                    //only reference equality, because overridden equality might get a bit weird...
                    info = _items.FirstOrDefault(i => ReferenceEquals(i.Item, value));

                SetSelected(info, true);
            }
        }

        [SerializeField]
        private UnityEvent _selectedChanged = null;
        public UnityEvent SelectedChanged { get { return _selectedChanged; } }

        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
            ValidateSelected();
        }

        protected override void OnItemsSourceChanged()
        {
            base.OnItemsSourceChanged();
            ValidateSelected();
        }

        //because there are no events for when the selected object changes....
        void LateUpdate()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected == _lastSelected) return;
            _lastSelected = selected;
            
            //verify that our 'last' actual control is still selected
            if (_selectedInfo != null && _selectedInfo.Control != null)
            {
                if (_selectionState(_selectedInfo.Control.GetComponent<ListBoxItem>()))
                    return;
            }


            //otherwise we can change it to whatever is really selected now
            SetSelected(GetItemInfo(selected), false);
        }

        private ItemInfo GetItemInfo(GameObject selected)
        {
            if (selected == null) return null;

            foreach (var info in _items)
            {
                if (info.Control == selected)
                    return info;
            }

            //we only use the first parent, in the case of nested listboxes
            var parentItem = selected.GetComponentInParent<ListBoxItem>();
            var parent = parentItem == null ? null : parentItem.gameObject;
            foreach(var info in _items)
            {
                if (info.Control == parent)
                    return info;
            }

            return null;
        }

        void ValidateSelected()
        {
            //this ensures that the selected object we have is still the same. If it was removed, then it will cause it to change to null
            Selected = Selected;
        }

        void SetSelected(ItemInfo info, bool updateEventSystem)
        {
            if (info == _selectedInfo) return;

            _selectedInfo = info;
            if (updateEventSystem)
            {
                if (_selectedInfo != null)
                    EventSystem.current.SetSelectedGameObject(_selectedInfo.Control);
                else
                    EventSystem.current.SetSelectedGameObject(null);
            }

            SelectedChanged.Invoke();
        }
    }
}
