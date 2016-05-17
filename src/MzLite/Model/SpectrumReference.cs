using System;
using Newtonsoft.Json;

namespace MzLite.Model
{
    
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class SpectrumReference 
    {

        private readonly string sourceFileID;
        private readonly string runID;
        private readonly string spectrumID;

        public SpectrumReference(string runID, string spectrumID)
            : this(null, runID, spectrumID) { }

        [JsonConstructor]
        public SpectrumReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("RunID")] string runID,
            [JsonProperty("SpectrumID")] string spectrumID)                    
        {            
            this.sourceFileID = sourceFileID;
            this.runID = runID;
            this.spectrumID = spectrumID;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SourceFileID
        {
            get { return sourceFileID; }
        }

        [JsonProperty(Required = Required.Always)]
        public string RunID
        {
            get { return runID; }
        }        

        [JsonProperty(Required = Required.Always)]
        public string SpectrumID
        {
            get { return spectrumID; }
        }

        public bool IsExternal
        {
            get { return SourceFileID != null; }
        }
    }
}
