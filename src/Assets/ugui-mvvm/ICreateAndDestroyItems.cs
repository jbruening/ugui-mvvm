using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static uguimvvm.ItemsControl;

namespace uguimvvm
{
    /// <summary>
    /// The behaviors for ItemsControl's item management and those item's GameObjects
    /// </summary>
    public interface ICreateDestroyItems
    {
        GameObject CreateItemControl(object item);
        void DestroyItemControl(ItemInfo item);
        GameObject InstantiateItem();
        void SetItemTemplate(GameObject itemTemplate);
        void SetItemTemplate(DataTemplateSelector dataTemplateSelector);
    }
}
