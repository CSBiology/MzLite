using System.Collections.Generic;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a data processing.
    /// Captures the processing steps applied and the use of data processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class DataProcessing : ProjectItem
    {

        private readonly DataProcessingStepList processingSteps = new DataProcessingStepList();

        private DataProcessing() : base() { }

        public DataProcessing(string name) : base(name) { }

        [JsonProperty]
        public DataProcessingStepList ProcessingSteps { get { return processingSteps; } }
    }

    /// <summary>
    /// The project item container for data processings.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingList : ProjectItemCollection<DataProcessing>
    {

        [JsonConstructor]
        public DataProcessingList() : base() { }

        public DataProcessingList(IEnumerable<DataProcessing> processings) : base(processings) { }

    }

    /// <summary>
    /// Expansible description of a data processing step and use of processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DataProcessingStep : NamedItem
    {

        private DataProcessingStep() : base() { }

        public DataProcessingStep(string name) : base(name) {  }
       
    }

    /// <summary>
    /// The container for data processing steps.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingStepList : NamedItemCollection<DataProcessingStep>
    {
        internal DataProcessingStepList() : base() { }

    }
}
