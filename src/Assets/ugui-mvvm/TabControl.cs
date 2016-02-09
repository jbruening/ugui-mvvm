using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uguimvvm.Primitives;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;

namespace uguimvvm
{
    [AddComponentMenu("UI/Tabs/TabControl", 1)]
    class TabControl : Selector
    {
        private GameObject _lastTab;

        protected override void OnItemsSourceChanged()
        {
            base.OnItemsSourceChanged();
            Reselect();
        }

        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
            Reselect();
        }

        private void Reselect()
        {
            if (_items.Count == 0)
            {
                DeselectTab(_lastTab);
                _lastTab = null;
                return;
            }

            if (SelectedInfo != null)
                return;
            
            SetSelected(_items.FirstOrDefault(i => i.Control == _lastTab) ?? _items[0]);
        }

        protected override void OnSelectedChanged(bool fromProperty)
        {
            if (SelectedInfo == null)
            {
                return;
            }

            DeselectTab(_lastTab);
            _lastTab = SelectedInfo.Control;
            if (!fromProperty) return;
            _lastTab.GetComponent<TabItem>().SetSelected(true);
        }

        private void DeselectTab(GameObject lastTab)
        {
            if (lastTab == null) return;
            lastTab.GetComponent<TabItem>().SetSelected(false);
        }

        internal void SelectTab(TabItem item)
        {
            SetSelected(_items.FirstOrDefault(i => i.Control == item.gameObject));
        }
    }
}
