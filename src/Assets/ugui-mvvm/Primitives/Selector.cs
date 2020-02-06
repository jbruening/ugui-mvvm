using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace uguimvvm.Primitives
{
    /// <summary>
    /// Represents a control that enables a user to select an item from a collection of items.
    /// </summary>
    public abstract class Selector : ItemsControl
    {
        /// <summary>
        /// Gets the complete set of info about the selected value/control.
        /// </summary>
        protected ItemInfo SelectedInfo { get; private set; }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        public object Selected
        {
            get { return SelectedInfo == null ? null : SelectedInfo.Item; }
            set
            {
                ItemInfo info;
                if (value == null) //because null always means 'no selected object'
                    info = null;
                else
                    //only reference equality, because overridden equality might get a bit weird...
                    info = _items.FirstOrDefault(i => ReferenceEquals(i.Item, value));

                InternalSetSelected(info, true);
            }
        }

        /// <summary>
        /// Gets or sets the selected item/control.
        /// </summary>
        public GameObject SelectedControl
        {
            get { return SelectedInfo == null ? null : SelectedInfo.Control; }
        }

        [SerializeField]
        private UnityEvent _selectedChanged = null;

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public UnityEvent SelectedChanged { get { return _selectedChanged; } }

        /// <inheritdoc />
        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
            ValidateSelected();
        }

        /// <inheritdoc />
        protected override void OnItemsSourceChanged()
        {
            base.OnItemsSourceChanged();
            ValidateSelected();
        }

        /// <summary>
        /// Ensure the selected object we have is still correct
        /// </summary>
        protected void ValidateSelected()
        {
            //this ensures that the selected object we have is still the same. If it was removed, then it will cause it to change to null
            Selected = Selected;
        }

        /// <summary>
        /// Sets the selected value/control without marking it as a change triggered from the <see cref="Selected"/> property.
        /// </summary>
        /// <param name="info"></param>
        protected void SetSelected(ItemInfo info)
        {
            InternalSetSelected(info, false);
        }

        void InternalSetSelected(ItemInfo info, bool fromProperty)
        {
            if (info == SelectedInfo) return;

            SelectedInfo = info;

            OnSelectedChanged(fromProperty);
            SelectedChanged.Invoke();
        }

        /// <summary>
        /// When Selected changes. fromProperty will be false if SetSelected was used to change the Selected object
        /// </summary>
        /// <param name="fromProperty"></param>
        protected virtual void OnSelectedChanged(bool fromProperty) { }
    }
}
