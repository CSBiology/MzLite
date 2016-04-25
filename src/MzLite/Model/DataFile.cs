using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a data file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class DataFile : NamedModelItem
    {

        [JsonProperty("Location", Required = Required.Always)]
        private string location;

        private DataFile() : base() { }

        public DataFile(string id, string name, string location) 
            : base(id, name) 
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException("location");
            this.location = location;
        }
        
        public string Location
        {
            get
            {
                return location;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new NullReferenceException("Location cannot be null or empty.");

                if (value != location)
                {
                    NotifyPropertyChanging("Location");
                    this.location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }
    }

    /// <summary>
    /// The model item container for data files.
    /// </summary>
    [JsonArray]
    public sealed class DataFileList : ObservableModelItemCollection<DataFile>
    {
        [JsonConstructor]
        internal DataFileList() : base() { }

    }
}
