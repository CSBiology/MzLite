using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MzLite.Model
{
    public abstract class NotifyChangedCollection<T> : ObservableCollection<T> 
        where T : class
    {

        public NotifyChangedCollection() : base() { }

        public NotifyChangedCollection(IEnumerable<T> collection) : base(collection) { }

        public NotifyChangedCollection(List<T> list) : base(list) { }
    }
}
