using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MzLite.Model
{
    
    /// <summary>
    /// Exposes an interface of items that have a name.
    /// </summary>
    public interface INamedItem
    {
        string Name { get; }
        void SetName(string name);
    }

    /// <summary>
    /// An abstract base class of expansible description items that have a name.
    /// </summary> 
    public abstract class NamedItem : ParamContainer, INamedItem, INotifyPropertyChanged, INotifyPropertyChanging
    {

        [JsonProperty("Name", Required = Required.Always)]
        private string name;

        internal NamedItem() : base() { }

        internal NamedItem(string name)
            : base()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        #region INamedItem Members

        public string Name 
        { 
            get 
            { 
                return name; 
            }            
        }

        void INamedItem.SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Name cannot be null or empty.");

            if (this.name != name)
            {
                NotifyPropertyChanging("Name");
                this.name = name;
                NotifyPropertyChanged("Name");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;        

        protected void NotifyPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        protected void NotifyPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion                

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion

        #region INotifyPropertyChanging Members

        event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion
    }

    /// <summary>
    /// Base class of an observable collection of items that can be accessed by name. 
    /// </summary>    
    public abstract class ObservableNamedItemCollection<T> : ObservableKeyedCollection<string, T>
        where T : INamedItem
    {

        internal ObservableNamedItemCollection()
            : base()
        {
        }
               
        public void Rename(T item, string newName)
        {
            base.ChangeItemKey(item, newName);
            item.SetName(newName);
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.Name;
        }        
        
    }
}
