using System.Collections.Generic;
using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Caching behavior creates a pool of hidden GameObjects when it is constructed, then activates
    /// and deactives them as they are assigned to items. If more items are needed than the initial amount
    /// they will default to being instatiated and added GameObject pool. 
    /// </summary>
    public class CachingCreateDestroyItemsBehavior : ICreateDestroyItems
    {
        /// <summary>
        /// The collection of the item's GameObjects generated for caching 
        /// </summary>
        private readonly List<GameObject> _gameObjectsCached = new List<GameObject>();

        /// <summary>
        /// Caching Behavior looks to other behaviors for instantiating and handling templates
        /// </summary>
        private readonly ICreateDestroyItems _createDestroyImpl;

        /// <summary>
        /// Correlating items to their cached GameObjects  
        /// </summary>
        private readonly Dictionary<object, GameObject> _itemToGameObjectMap = new Dictionary<object, GameObject>();

        public CachingCreateDestroyItemsBehavior(ICreateDestroyItems createDestroyInfo, int initialPoolSize)
        {
            _createDestroyImpl = createDestroyInfo;

            // Instantiate the starting pool of GameObjects
            for (var i = 0; i < initialPoolSize; i++)
            {
                var newGameObject = InstantiateItem();
                // These item GameObjects haven't been asigned yet so hide them
                newGameObject.SetActive(false);
                _gameObjectsCached.Add(newGameObject);
            }  
        }

        public GameObject CreateItemControl(object item)
        {
            GameObject control = GetCachedControl();
 
            if (control != null)
            {
                // Unused controls were set inactive
                control.SetActive(true);
            }
            else
            {
                // There wasn't any in the pool left, create a new one and add it to the pool
                control = InstantiateItem();
                _gameObjectsCached.Add(control);
            }

            _itemToGameObjectMap.Add(item, control);

            return control;
        }

        public void DestroyItemControl(ItemsControl.ItemInfo item)
        {
            // Hide item
            item.Control.SetActive(false);
            // Free up gameobject from the pool so it can be reasigned
            _itemToGameObjectMap.Remove(item.Item);
        }

        private GameObject GetCachedControl()
        {
            GameObject control = null;

            // Find a control that isn't already mapped to an item 
            foreach (var go in _gameObjectsCached)
            {
                if (!_itemToGameObjectMap.ContainsValue(go))
                {
                    control = go;
                    break;
                }
            }

            return control;
        }

        public GameObject InstantiateItem()
        {
            return(_createDestroyImpl.InstantiateItem());
        }

        public void SetItemTemplate(GameObject itemTemplate)
        {
            _createDestroyImpl.SetItemTemplate(itemTemplate);
        }

        public void SetItemTemplate(DataTemplateSelector dataTemplateSelector)
        {
            _createDestroyImpl.SetItemTemplate(dataTemplateSelector);
        }
    }
}
