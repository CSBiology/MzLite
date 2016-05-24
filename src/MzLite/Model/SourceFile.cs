using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a source file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class SourceFile : NamedModelItem
    {

        private string location;

        [JsonConstructor]
        public SourceFile(
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
    public sealed class SourceFileList : ObservableModelItemCollection<SourceFile>
    {
        [JsonConstructor]
        internal SourceFileList() : base() { }

    }
}
