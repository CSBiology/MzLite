using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Exposes the root class of the MzLite data model.
    /// Captures the use of mass spectrometers, sample descriptions, the mz data generated 
    /// and the processing of that data at the level of peak lists.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MzLiteModel : NamedItem
    {

        private readonly SampleList samples = new SampleList();
        private readonly DataFileList dataFiles = new DataFileList();
        private readonly DataProcessingList dataProcessings = new DataProcessingList();
        private readonly SoftwareList software = new SoftwareList();
        private readonly InstrumentList instruments = new InstrumentList();
        private readonly RunList runs = new RunList();

        private MzLiteModel() : base() { }

        public MzLiteModel(string name) : base(name) { }

        [JsonProperty]
        public SampleList Samples { get { return samples; } }

        [JsonProperty]
        public DataFileList DataFiles { get { return dataFiles; } }

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
