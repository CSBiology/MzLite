using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class SpectrumReference
    {

        private readonly string sourceFileID;        
        private readonly SpectrumLocator spectrumID;

        public SpectrumReference(SpectrumLocator spectrumID)
            : this(null, spectrumID) { }

        [JsonConstructor]
        public SpectrumReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("SpectrumID")] SpectrumLocator spectrumID)
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
        public SpectrumLocator SpectrumID
        {
            get { return spectrumID; }
        }

        public bool IsExternal
        {
            get { return SourceFileID != null; }
        }
    }
}
