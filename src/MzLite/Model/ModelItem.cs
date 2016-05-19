using System;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MzLite.Model
{
    
    /// <summary>
    /// Exposes an interface of a expansible description model item that can be referenced by an id.
    /// </summary>
    public interface IModelItem<TID> : IParamContainer where TID : class
    {
        TID ID { get; }
    }

    /// <summary>
    /// An abstract base class of a expansible description model item that can be referenced by an id.
    /// </summary>    
    public abstract class ModelItem<TID> : ParamContainer, IModelItem<TID>, INotifyPropertyChanged, INotifyPropertyChanging
        where TID : class
    {

        private readonly TID id;

        internal ModelItem(TID id)
            : base() 
        {
            if (id == null)
                throw new ArgumentNullException("id");
            this.id = id;
        }

        [JsonProperty(Required = Required.Always)]
        public TID ID { get { return id; } }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;            
            return this.id.Equals(((IModelItem<TID>)obj).ID);
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
    /// An abstract base class of a expansible description model item that can be referenced by an id and has an additional name.
    /// </summary>
    public abstract class NamedModelItem<TID> : ModelItem<TID>, INamedItem
        where TID : class
    {
        
        private string name;

        internal NamedModelItem(TID id, string name)
            : base(id) 
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        #region INamedItem Members

        [JsonProperty(Required = Required.Always)]
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
    /// Base class of an observable collection of model items that can be accessed by their embedded ids.     
    /// </summary>    
    public abstract class ObservableModelItemCollection<TID, T> : ObservableKeyedCollection<TID, T>
        where T : class, IModelItem<TID>
        where TID : class
    {
        
        internal ObservableModelItemCollection()
            : base()
        {
        }

        protected override TID GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.ID;
        }           
    }

    
}
