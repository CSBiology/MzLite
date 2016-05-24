using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// An abstract base class of expansible description items that can be identified a name.
    /// </summary> 
    public abstract class NamedItem : ParamContainer, INotifyPropertyChanged, INotifyPropertyChanging
    {

        private string name;

        internal NamedItem(string name)
            : base()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        #region NamedItem Members

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get
            {
                return name;
            }

            internal set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("Name cannot be null or empty.");

                if (this.name != value)
                {
                    NotifyPropertyChanging("Name");
                    this.name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;
            return this.name.Equals(((NamedItem)obj).name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
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

    }

    /// <summary>
    /// Base class of an observable collection of items that can be accessed by name. 
    /// </summary>    
    public abstract class ObservableNamedItemCollection<T> : ObservableKeyedCollection<string, T>
        where T : NamedItem
    {

        internal ObservableNamedItemCollection()
            : base()
        {
        }

        public void Rename(T item, string newName)
        {
            base.ChangeItemKey(item, newName);
            item.Name = newName;
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.Name;
        }

    }
}
