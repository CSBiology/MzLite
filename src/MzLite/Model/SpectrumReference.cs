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

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            SpectrumReference other = obj as SpectrumReference;
            if (other == null)
                return false;
            if (IsExternal)
                return spectrumID.Equals(other.spectrumID) && sourceFileID.Equals(other.sourceFileID);
            else
                return spectrumID.Equals(other.spectrumID);
        }

        public override int GetHashCode()
        {
            if (IsExternal)
                return Tuple.Create(spectrumID, sourceFileID).GetHashCode();
            else
                return spectrumID.GetHashCode();
        }
    }
}
