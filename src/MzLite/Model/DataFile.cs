using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a data file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class DataFile : ProjectItem
    {

        [JsonProperty("Location", Required = Required.Always)]
        private string location;

        private DataFile() : base() { }

        public DataFile(string name, string location) 
            : base(name) 
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
    /// The project item container for data files.
    /// </summary>
    [JsonArray]
    public sealed class DataFileList : ProjectItemContainer<DataFile>
    {
        [JsonConstructor]
        internal DataFileList() : base() { }

    }
}
