using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MzLite.Model
{

    public interface IProjectItem<TModelItem> 
        : IParamContainer 
        where TModelItem : IModelItem
    {
        Guid ItemID { get; }
        TModelItem Item { get; set; }
        bool HasItem { get; }
    }

    public abstract class ProjectItem<TModelItem> 
        : ParamContainer, IProjectItem<TModelItem>, INotifyPropertyChanged, INotifyPropertyChanging
        where TModelItem : class, IModelItem
    {

        
        private readonly Guid itemID;
        private TModelItem item;
        
        internal ProjectItem(Guid id)
            : base() 
        {
            if (id.Equals(default(Guid)))
                throw new ArgumentException("id: default value not allowed for id's.");
            this.itemID = id;
        }

        #region IProjectItem<TModelItem> Members

        [JsonProperty(Required = Required.Always)]
        public Guid ItemID
        {
            get { return itemID; }            
        }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public TModelItem Item
        {
            get { return item; }
            set
            {
                if (value != item)
                {
                    NotifyPropertyChanging("Item");
                    this.item = value;
                    NotifyPropertyChanged("Item");
                }
            }
        }

        public bool HasItem { get { return item != null; } }

        #endregion  
      
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;
            return this.ItemID.Equals(((IProjectItem<TModelItem>)obj).ItemID);
        }

        public override int GetHashCode()
        {
            return ItemID.GetHashCode();
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

    public abstract class NamedProjectItem<TModelItem> : ProjectItem<TModelItem>, INamedItem
        where TModelItem : class, IModelItem
    {        
        private string name;
        
        internal NamedProjectItem(Guid id, string name)
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

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MzLiteProject
    {



    }

    public abstract class ObservableProjectItemCollection<TProjectItem, TModelItem> : ObservableKeyedCollection<Guid, TProjectItem>
        where TProjectItem : class, IProjectItem<TModelItem>
        where TModelItem : class, IModelItem
    {

        internal ObservableProjectItemCollection()
            : base()
        {
        }

        protected override Guid GetKeyForItem(TProjectItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.ItemID;
        }
    }

    //[JsonObject(MemberSerialization.OptIn, IsReference=true)]
    //public sealed class ProjectSample : NamedProjectItem<Sample>
    //{
    //    [JsonConstructor]
    //    public ProjectSample([JsonProperty("ItemID")] Guid id, [JsonProperty("Name")] string name) : base(id, name) { }

    //    [JsonProperty]
    //    SamplePreparationList Preparations { get { return Item.Preparations; } }

    //    [JsonProperty]
    //    SampleTreatmentList Treatments { get { return Item.Treatments; } }
    //}

    //[JsonArray]
    //public sealed class ProjectSampleList : ObservableProjectItemCollection<ProjectSample, Sample>
    //{
        
    //    [JsonConstructor]
    //    internal ProjectSampleList() : base() { }
        
    //}
}
