using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MzLite.Model
{

    /// <summary>
    /// Base class of a collection of items that can be accessed by keys whose embedded in items. 
    /// </summary> 
    public abstract class KeyCollection<TKey, T> : Collection<T>, INotifyCollectionChanged
    {

        private readonly IDictionary<TKey, T> itemDictionary;

        protected KeyCollection(IEqualityComparer<TKey> keyComparer)
        {
            itemDictionary = new Dictionary<TKey, T>(keyComparer);
        }

        protected KeyCollection(IEqualityComparer<TKey> keyComparer, IEnumerable<T> items)
            : base(new List<T>(items))
        {
            itemDictionary = new Dictionary<TKey, T>(keyComparer);
            foreach (var item in items)
                this.itemDictionary.Add(GetKeyForItem(item), item);
        }

        #region KeyCollection Members

        protected abstract TKey GetKeyForItem(T item);        

        protected IDictionary<TKey, T> ItemDictionary { get { return itemDictionary; } }

        public T this[TKey key]
        {
            get
            {
                return itemDictionary[key];
            }
        }

        public bool ContainsKey(TKey key)
        {
            return itemDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return itemDictionary.Keys; }
        }

        public bool RemoveByKey(TKey key)
        {
            T item;

            if (itemDictionary.TryGetValue(key, out item))
            {
                return Remove(item);
            }

            return false;
        }

        public bool TryGetByKey(TKey key, out T item)
        {
            return itemDictionary.TryGetValue(key, out item);
        }

        #endregion

        #region Collection Override Members

        protected override void ClearItems()
        {
            this.itemDictionary.Clear();
            base.ClearItems();
            NotifyCollectionReset();
        }

        protected override void InsertItem(int index, T item)
        {
            this.itemDictionary.Add(GetKeyForItem(item), item);
            base.InsertItem(index, item);
            NotifyItemAdded(index, item);
        }

        protected override void RemoveItem(int index)
        {
            T old = base[index];
            if (old != null)
                this.itemDictionary.Remove(GetKeyForItem(old));
            base.RemoveItem(index);
            NotifyItemRemoved(index);
        }

        protected override void SetItem(int index, T item)
        {
            T old = base[index];
            if (old != null)
                this.itemDictionary.Remove(GetKeyForItem(old));
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
