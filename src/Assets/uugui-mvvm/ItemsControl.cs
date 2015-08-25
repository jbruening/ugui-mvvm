using System.Collections;
using UnityEngine;
using UnityEngine.Events;

class ItemsControl : MonoBehaviour
{
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
            ItemsSourceChanged.Invoke();
        }
    }

    [SerializeField]
    private UnityEvent _itemsSourceChanged;
    public UnityEvent ItemsSourceChanged { get { return _itemsSourceChanged; } }

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

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            //todo:
            //case NotifyCollectionChangedAction.Add:
            //    AddItems(e.NewItems);
            //    break;
            //case NotifyCollectionChangedAction.Remove:
            //    RemoveItems(e.OldItems);
            //    break;
            //case NotifyCollectionChangedAction.Move:
            //case NotifyCollectionChangedAction.Replace:
            //    RemoveItems(e.OldItems);
            //    AddItems(e.NewItems);
            //    break;
            case NotifyCollectionChangedAction.Reset:
            default:
                ResetCollection();
                break;
        }
    }

    private void ResetCollection()
    {
        ClearChildren();

        var trans = transform;
        foreach(var item in _itemsSource)
        {
            var control = Instantiate(_itemTemplate);
            control.SetActive(true);
            var rect = control.GetComponent<RectTransform>();
            if (rect == null)
                control.transform.parent = trans;
            else
                rect.SetParent(trans, false);
            var context = control.GetComponent<DataContext>();
            if (context == null) continue;
            context.UpdateValue(item);
        }
    }

    private void ClearChildren()
    {
        var trans = transform;
        var ci = trans.childCount;
        for(int i = 0; i < ci; i++)
        {
            var child = trans.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    void OnDestroy()
    {
        if (_itemsSource is INotifyCollectionChanged)
        {
            (_itemsSource as INotifyCollectionChanged).CollectionChanged -= CollectionChanged;
        }
    }
}