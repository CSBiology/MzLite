using System;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MzLite.Model
{
    
    /// <summary>
    /// Exposes an interface of an mz model item that can be referenced by a native id.
    /// </summary>
    public interface IModelItem
    {
        string NativeID { get; }
    }

    /// <summary>
    /// An abstract base class of a expansible description item that can be referenced by a native id.
    /// </summary>    
    public abstract class ModelItem : ParamContainer, IModelItem, INotifyPropertyChanged, INotifyPropertyChanging
    {

        [JsonProperty("NativeID", Required = Required.Always)]
        private string id;

        internal ModelItem() : base() { }

        internal ModelItem(string id)
            : base() 
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");
            this.id = id;
        }

        public string NativeID { get { return id; } }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;            
            return this.id.Equals(((IModelItem)obj).NativeID);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

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
    /// An abstract base class of a expansible description item that can be referenced by a native id and has an additional name.
    /// </summary>
    public abstract class NamedModelItem : ModelItem, INamedItem
    {
        [JsonProperty("Name", Required = Required.Always)]
        private string name;

        internal NamedModelItem() : base() { }

        internal NamedModelItem(string id, string name)
            : base(id) 
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
    }

    /// <summary>
    /// Base class of an observable collection of model items that can be accessed by it's native id.     
    /// </summary>    
    public abstract class ObservableModelItemCollection<T> : ObservableKeyedCollection<string, T>
        where T : class, IModelItem
    {
        
        internal ObservableModelItemCollection()
            : base()
        {
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.NativeID;
        }           
    }
}
