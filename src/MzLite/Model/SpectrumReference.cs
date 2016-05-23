using System;
using Newtonsoft.Json;

namespace MzLite.Model
{
    
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class SpectrumReference
    {

        private readonly string sourceFileID;
        private readonly string spectrumID;

        public SpectrumReference(string spectrumID)
            : this(null, spectrumID) { }

        [JsonConstructor]
        public SpectrumReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("SpectrumID")] string spectrumID)
        {
            
            if (spectrumID == null)
                throw new ArgumentNullException("spectrumID");

            this.sourceFileID = sourceFileID;
            this.spectrumID = spectrumID;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SourceFileID
        {
            get { return sourceFileID; }
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
