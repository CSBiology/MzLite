using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MzLite.Model
{
    
    /// <summary>
    /// Exposes an interface of items that can be identified by a name.
    /// </summary>
    public interface INamedItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        string Name { get; }
        void SetName(string name);
    }

    /// <summary>
    /// An abstract base class of expansible description items that can be identified by a name.
    /// </summary> 
    public abstract class NamedItem : ParamContainer, INamedItem
    {

        [JsonProperty("Name", Required = Required.Always)]
        private string name;

        protected NamedItem() : base() { }

        protected NamedItem(string name)
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
    /// Base class of collections of items that can be accessed by name. 
    /// </summary>    
    public abstract class NamedItemCollection<T> : KeyCollection<string, T>
        where T : INamedItem
    {

        protected NamedItemCollection()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        protected NamedItemCollection(IEnumerable<T> items)
            : base(StringComparer.InvariantCultureIgnoreCase, items)
        {
        }
                
        public void Rename(T item, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentNullException("newName");
            if (item == null)
                throw new ArgumentNullException("item");
            if (base.ItemDictionary.ContainsKey(newName))
                throw new InvalidOperationException("newName already exists.");
            if (!base.Items.Contains(item))
                throw new InvalidOperationException("Item not member of this collection.");
            base.ItemDictionary.Remove(GetKeyForItem(item));
            item.SetName(newName);
            base.ItemDictionary.Add(GetKeyForItem(item), item);
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.Name;
        }        
        
    }
}
