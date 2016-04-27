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

        private string location;

        [JsonConstructor]
        public DataFile(
            [JsonProperty("ID")] string id, 
            [JsonProperty("Name")] string name, 
            [JsonProperty("Location")] string location) 
            : base(id, name) 
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException("location");
            this.location = location;
        }

        [JsonProperty(Required = Required.Always)]
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
