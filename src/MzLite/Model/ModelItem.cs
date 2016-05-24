using System;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MzLite.Model
{

    
    /// <summary>
    /// An abstract base class of a expansible description model item that can be referenced by an id.
    /// </summary>    
    public abstract class ModelItem : ParamContainer, INotifyPropertyChanged, INotifyPropertyChanging
    {

        private string id;

        internal ModelItem(string id)
            : base()
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");
            this.id = id;
        }

        #region ModelItem Members

        [JsonProperty(Required = Required.Always)]
        public string ID
        {
            get { return id; }
            internal set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("value");

                if (!this.id.Equals(id))
                {
                    NotifyPropertyChanging("ID");
                    this.id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }        

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;
            return this.id.Equals(((ModelItem)obj).ID);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
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
    /// An abstract base class of a expansible description model item that can be referenced by an id and has an additional name.
    /// </summary>
    public abstract class NamedModelItem : ModelItem
    {

        private string name;

        internal NamedModelItem(string id, string name)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get
            {
                return name;
            }
            set
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
    }

    /// <summary>
    /// Base class of an observable collection of model items that can be accessed by their embedded ids.     
    /// </summary>    
    public abstract class ObservableModelItemCollection<T> : ObservableKeyedCollection<string, T>
        where T : ModelItem
    {

        internal ObservableModelItemCollection()
            : base()
        {
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.ID;
        }
    }

}
