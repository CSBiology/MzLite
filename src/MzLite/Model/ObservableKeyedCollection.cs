using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MzLite.Model
{
        
    /// <summary>
    /// Base class of an observable collection of items that can be accessed by keys whose embedded in items. 
    /// </summary> 
    public abstract class ObservableKeyedCollection<TKey, T> : KeyedCollection<TKey, T>, INotifyCollectionChanged
    {
        
        internal ObservableKeyedCollection(IEqualityComparer<TKey> keyComparer) 
            : base(keyComparer)
        {            
        }

        internal ObservableKeyedCollection() : base() { }

        public bool TryGetItemByKey(TKey key, out T item) 
        {
            if (base.Dictionary == null)
            {
                item = default(T);
                return false;
            }
            else
            {
                return base.Dictionary.TryGetValue(key, out item);
            }
        }

        #region KeyedCollection Override Members

        protected override void ClearItems()
        {            
            base.ClearItems();
            NotifyCollectionReset();
        }

        protected override void InsertItem(int index, T item)
        {            
            base.InsertItem(index, item);
            NotifyItemAdded(index, item);
        }

        protected override void RemoveItem(int index)
        {            
            base.RemoveItem(index);
            NotifyItemRemoved(index);
        }

        protected override void SetItem(int index, T item)
        {            
            base.SetItem(index, item);
            NotifyItemSet(index, item);
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void NotifyItemAdded(int index, T item)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        private void NotifyItemSet(int index, T item)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
            }
        }

        private void NotifyItemRemoved(int index)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, index));
            }
        }

        private void NotifyCollectionReset()
        {
            if (CollectionChanged != null)
            {
                CollectionChanged.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #endregion
    }

}
