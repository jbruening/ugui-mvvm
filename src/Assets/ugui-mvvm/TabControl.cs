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
    public class TabControl : Selector
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
                SetTabsState(null);
                return;
            }

            if (SelectedInfo != null)
                return;

            var info = _items.FirstOrDefault(i => i.Control == _lastTab) ?? _items[0];
            SetSelected(info);
            SetTabsState(info.Control);
        }

        protected override void OnSelectedChanged(bool fromProperty)
        {
            if (SelectedInfo == null)
            {
                SetTabsState(null);
                return;
            }

            _lastTab = SelectedInfo.Control;
            if (!fromProperty) return;
            SetTabsState(_lastTab);
        }

        private void SetTabsState(GameObject tab)
        {
            var i = 0;
            foreach (Transform child in transform)
            {
                if (i == 0 && tab == null)
                    SetTabSelected(child.gameObject, true);
                else
                    SetTabSelected(child.gameObject, tab == child.gameObject);
                i++;
            }
        }

        private void SetTabSelected(GameObject tab, bool state)
        {
            if (tab == null) return;
            tab.GetComponent<TabItem>().SetSelected(state);
        }

        internal void SelectTab(TabItem item)
        {
            SetSelected(_items.FirstOrDefault(i => i.Control == item.gameObject));
            if (_items.Count == 0)
                SetTabsState(item.gameObject);
        }
    }
}
