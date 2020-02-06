#if !(UNITY_WSA || !NET_LEGACY)
using System;
using System.Collections;

namespace uguimvvm
{
    /// <summary>
    /// Represents the method that handles the <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Information about the event.</param>
    public delegate void NotifyCollectionChangedEventHandler(Object sender, NotifyCollectionChangedEventArgs e);

    /// <summary>
    /// Notifies listeners of dynamic changes, such as when an item is added and removed or the whole list is cleared.
    /// </summary>
    public interface INotifyCollectionChanged
    {
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    /// <summary>
    /// Provides data for the <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
    /// </summary>
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the action that caused the event.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        /// Gets the list of new items involved in the change.
        /// </summary>
        public IList NewItems { get; private set; }

        /// <summary>
        /// Gets the index at which the change occurred.
        /// </summary>
        public int NewStartingIndex { get; private set; }

        /// <summary>
        /// Gets the list of items affected by a <see cref="NotifyCollectionChangedAction.Replace"/>, <see cref="NotifyCollectionChangedAction.Remove"/>, or <see cref="NotifyCollectionChangedAction.Move"/> action.
        /// </summary>
        public IList OldItems { get; private set; }

        /// <summary>
        /// Gets the index at which a <see cref="NotifyCollectionChangedAction.Move"/>, <see cref="NotifyCollectionChangedAction.Remove"/>, or <see cref="NotifyCollectionChangedAction.Replace"/> action occurred.
        /// </summary>
        public int OldStartingIndex { get; private set; }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            if (action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Reset", "action");

            InitializeAdd(action, null, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[] { changedItem }, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                if (index != -1)
                    throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[] { changedItem }, index);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");

                InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                if (startingIndex != -1)
                    throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");
                if (startingIndex < -1)
                    throw new ArgumentException("IndexCannotBeNegative", "startingIndex");

                InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("NotifyCollectionChangedAction.Replace", "action");

            InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Replace", "action");

            InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, index, index);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Replace", "action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Replace", "action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Move", "action");
            if (index < 0)
                throw new ArgumentException("IndexCannotBeNegative", "index");

            object[] changedItems = new object[] { changedItem };
            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException("WrongActionForCtor, NotifyCollectionChangedAction.Move", "action");
            if (index < 0)
                throw new ArgumentException("IndexCannotBeNegative", "index");

            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == NotifyCollectionChangedAction.Add)
                InitializeAdd(action, changedItems, startingIndex);
            else if (action == NotifyCollectionChangedAction.Remove)
                InitializeRemove(action, changedItems, startingIndex);
            else
                throw new Exception(string.Format("Unsupported action: {0}", action.ToString()));
        }

        private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            Action = action;
            NewItems = (newItems == null) ? null : ArrayList.ReadOnly(newItems);
            NewStartingIndex = newStartingIndex;
        }

        private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            Action = action;
            OldItems = (oldItems == null) ? null : ArrayList.ReadOnly(oldItems);
            OldStartingIndex = oldStartingIndex;
        }

        private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {
            InitializeAdd(action, newItems, startingIndex);
            InitializeRemove(action, oldItems, oldStartingIndex);
        }
    }

    /// <summary>
    /// Describes the action that caused a <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
    /// </summary>
    public enum NotifyCollectionChangedAction
    {
        /// <summary> One or more items were added to the collection. </summary>
        Add,
        /// <summary> One or more items were removed from the collection. </summary>
        Remove,
        /// <summary> One or more items were replaced in the collection. </summary>
        Replace,
        /// <summary> One or more items were moved within the collection. </summary>
        Move,
        /// <summary> The contents of the collection changed dramatically. </summary>
        Reset,
    }
}
#endif
