using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Exposes the root class of the MzLite model.
    /// Captures the use of a mass spectrometer, the sample model, the mz data generated 
    /// and the processing of that data at the level of the peak lists.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MzLiteProject : ProjectItem
    {

        private readonly SampleList samples = new SampleList();
        private readonly DataFileList dataFiles = new DataFileList();
        private readonly DataProcessingList dataProcessings = new DataProcessingList();

        private MzLiteProject() : base() { }

        public MzLiteProject(string name) : base(name) { }               
        
        [JsonProperty]
        public SampleList Samples { get { return samples; } }

        [JsonProperty]
        public DataFileList DataFiles { get { return dataFiles; } }

        [JsonProperty]
        public DataProcessingList DataProcessings { get { return dataProcessings; } }
    }
}
