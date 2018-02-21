using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace uguimvvm
{
    public class ItemsControl : MonoBehaviour
    {
        protected class ItemInfo
        {
            public readonly object Item;
            public readonly GameObject Control;
            public readonly RectTransform Rect;

            public ItemInfo(object item, GameObject control, RectTransform rect)
            {
                Item = item;
                Control = control;
                Rect = rect;
            }
        }

        [SerializeField]
        private GameObject _itemTemplate;
        public GameObject ItemTemplate
        {
            get { return _itemTemplate; }
            set
            {
                if (_itemTemplate == value) return;
                _itemTemplate = value;
                ResetCollection();
            }
        }

        [Tooltip("When Awaking, destroy any children that are not part of the ItemsSource or the prefab.\nThis is useful where you want to view the items control as it would be with example objects, but don't actually want them to be children at runtime")]
        [SerializeField]
        private bool _destroyChildrenOnAwake = false;

        private IEnumerable _itemsSource;
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value) return;
                ResetBindings(_itemsSource, value);
                _itemsSource = value;
                ResetCollection();
                OnItemsSourceChanged();
                ItemsSourceChanged.Invoke();
            }
        }

        protected readonly List<ItemInfo> _items = new List<ItemInfo>();

        [SerializeField]
        private UnityEvent _itemsSourceChanged = null;
        public UnityEvent ItemsSourceChanged { get { return _itemsSourceChanged; } }

        public bool HasItems { get { return _items.Count > 0; } }

        [SerializeField]
        private UnityEvent _hasItemsChanged = null;
        public UnityEvent HasItemsChanged { get { return _hasItemsChanged; } }

        void Awake()
        {
            if (_destroyChildrenOnAwake)
            {
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    var cg = transform.GetChild(i).gameObject;
                    if (cg == ItemTemplate)
                        cg.SetActive(false);
                    else if (_items.All(c => c.Control != cg))
                        Destroy(cg);
                }
            }
        }

        private void ResetBindings(IEnumerable oldvalue, IEnumerable newvalue)
        {
            if (oldvalue is INotifyCollectionChanged)
            {
                (oldvalue as INotifyCollectionChanged).CollectionChanged -= CollectionChanged;
            }
            if (newvalue is INotifyCollectionChanged)
            {
                (newvalue as INotifyCollectionChanged).CollectionChanged += CollectionChanged;
            }
        }

        protected virtual void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                default:
                    ResetCollection();
                    break;
            }
        }

        private void MoveItem(int oldIndex, int newIndex)
        {
            var item = _items[oldIndex];
            //not using RemoveAt, because we can just move the control around.
            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, item);
            //we just need to change this object's position in the children
            var rect = item.Rect;
            rect.SetSiblingIndex(newIndex);
        }

        private void AddItems(IEnumerable newItems)
        {
            var trans = transform;
            foreach (var item in newItems)
            {
                var control = Instantiate(_itemTemplate);
                control.SetActive(true);

                var rect = control.GetComponent<RectTransform>();
                if (rect == null)
                    control.transform.parent = trans;
                else
                    rect.SetParent(trans, false);

                var info = new ItemInfo(item, control, rect);
                _items.Add(info);

                var context = control.GetComponent<DataContext>();
                if (context != null)
                    context.UpdateValue(item);

                OnItemAdded(info);
            }

            HasItemsChanged.Invoke();
        }

        /// <summary>
        /// After an item is added to the controls
        /// </summary>
        /// <param name="info"></param>
        protected virtual void OnItemAdded(ItemInfo info) { }

        private void RemoveItems(IEnumerable oldItems)
        {
            foreach (var item in oldItems)
            {
                RemoveAt(_items.FindIndex(i => i.Item == item));
            }

            HasItemsChanged.Invoke();
        }

        /// <summary>
        /// After an item is removed from controls
        /// </summary>
        /// <param name="info"></param>
        protected virtual void OnItemRemoved(ItemInfo info) { }

        private void ResetCollection()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
            AddItems(_itemsSource);
        }

        /// <summary>
        /// Fired after the controls have re-created, before the ItemsSourceChanged event is invoked
        /// </summary>
        protected virtual void OnItemsSourceChanged() { }

        void RemoveAt(int idx)
        {
            if (idx < 0) return;
            var item = _items[idx];
            _items.RemoveAt(idx);
            Destroy(item.Control);
            OnItemRemoved(item);
        }

        void OnDestroy()
        {
            if (_itemsSource is INotifyCollectionChanged)
            {
                (_itemsSource as INotifyCollectionChanged).CollectionChanged -= CollectionChanged;
            }
        }
    }
}