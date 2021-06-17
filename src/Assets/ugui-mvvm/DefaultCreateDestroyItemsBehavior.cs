using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Defualt Behavior for items is to Instantiate and Destroy the GameObjects associated to the Items
    /// as needed. 
    /// </summary>
    public class DefaultCreateDestroyItemsBehavior : ICreateDestroyItems
    {
        private GameObject _itemTemplate;

        private DataTemplateSelector _itemTemplateSelector;

        private Transform _itemParent;

        public DefaultCreateDestroyItemsBehavior(Transform parent, GameObject itemTemplate, DataTemplateSelector itemTemplateSelector)
        {
            _itemParent = parent;
            _itemTemplate = itemTemplate;
            _itemTemplateSelector = itemTemplateSelector;
        }

        public GameObject CreateItemControl(object item)
        {
            CheckItemTemplateSelector(item);

            return InstantiateItem();
        }

        public void DestroyItemControl(ItemsControl.ItemInfo item)
        {
            var rect = item.Control.GetComponent<RectTransform>();
            if (rect == null)
                item.Control.transform.parent = null;
            else
                rect.SetParent(null, false);

            Object.Destroy(item.Control);
        }

        public GameObject InstantiateItem()
        {
            if (_itemTemplate == null)
            {
                throw new System.NotImplementedException("There is no ItemTemplate set for this ItemsControl list");
            }

            var newGameObject = Object.Instantiate(_itemTemplate);
            var rect = newGameObject.GetComponent<RectTransform>();
            if (rect == null)
                newGameObject.transform.parent = _itemParent;
            else
                rect.SetParent(_itemParent, false);

            return newGameObject;
        }

        public void CheckItemTemplateSelector(object item)
        {
            // MRMW Start Change - Prefab Selector support
            _itemTemplate = _itemTemplateSelector != null ? _itemTemplateSelector.SelectTemplate(item) : _itemTemplate;
            // MRMW End Change - Prefab Selector support
        }

        public void SetItemTemplate(GameObject itemTemplate)
        {
            throw new System.NotImplementedException();
        }

        public void SetItemTemplate(DataTemplateSelector dataTemplateSelector)
        {
            throw new System.NotImplementedException();
        }
    }
}
