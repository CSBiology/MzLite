using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a data processing.
    /// Captures the processing steps applied and the use of data processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class DataProcessing : ModelItem
    {

        private readonly DataProcessingStepList processingSteps = new DataProcessingStepList();

        [JsonConstructor]
        public DataProcessing([JsonProperty("ID")] string id) : base(id) { }

        [JsonProperty]
        public DataProcessingStepList ProcessingSteps { get { return processingSteps; } }
    }

    /// <summary>
    /// The model item container for data processings.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingList : ObservableModelItemCollection<DataProcessing>
    {
        [JsonConstructor]
        internal DataProcessingList() : base() { }
    }

    /// <summary>
    /// Expansible description of a data processing step and use of processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DataProcessingStep : NamedItem
    {
        
        private Software software;
        
        [JsonConstructor]
        public DataProcessingStep([JsonProperty("Name")] string name) : base(name) {  }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Software Software 
        {
            get { return software; }
            set
            {
                if (software != value)
                {
                    NotifyPropertyChanging("Software");
                    this.software = value;
                    NotifyPropertyChanged("Software");
                }
            }
        }
    }

    /// <summary>
    /// The container for data processing steps.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingStepList : ObservableNamedItemCollection<DataProcessingStep>
    {
        [JsonConstructor]
        internal DataProcessingStepList() : base() { }

    }
}
