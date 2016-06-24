#region license
// The MIT License (MIT)

// ObservableKeyedCollection.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany
 

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
