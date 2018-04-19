﻿using System;
using System.Collections.Generic;
// MRMW_CHANGE - BEGIN: Replacing uguimvvm ObservableCollection<T> and INotifyCollectionChanged with System variants.
using System.Collections.Specialized;
// MRMW_CHANGE - END: Replacing uguimvvm ObservableCollection<T> and INotifyCollectionChanged with System variants.
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
            SetTabsState(_lastTab);
        }

        private void SetTabsState(GameObject tab)
        {
            for(var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                if (i == 0 && tab == null)
                    SetTabSelected(child, true);
                else
                    SetTabSelected(child, tab == child);
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
