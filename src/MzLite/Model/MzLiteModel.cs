using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Exposes the root class of the mz data model.
    /// Captures the use of mass spectrometers, sample descriptions, the mz data generated 
    /// and the processing of that data at the level of peak lists.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MzLiteModel : NamedItem
    {

        private readonly SampleList samples = new SampleList();        
        private readonly SourceFileList sourceFiles = new SourceFileList();
        private readonly DataProcessingList dataProcessings = new DataProcessingList();
        private readonly SoftwareList software = new SoftwareList();
        private readonly InstrumentList instruments = new InstrumentList();
        private readonly RunList runs = new RunList();

        [JsonConstructor]
        public MzLiteModel([JsonProperty("Name")] string name) : base(name) { }

        [JsonProperty]
        public SampleList Samples { get { return samples; } }        

        [JsonProperty]
        public SourceFileList SourceFiles { get { return sourceFiles; } }

        [JsonProperty]
        public SoftwareList Software { get { return software; } }

        [JsonProperty]
        public DataProcessingList DataProcessings { get { return dataProcessings; } }

        [JsonProperty]
        public InstrumentList Instruments { get { return instruments; } }

        [JsonProperty]
        public RunList Runs { get { return runs; } }

    }
}
