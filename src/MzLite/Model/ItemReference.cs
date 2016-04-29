using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    public abstract class SourceFileReference
    {
        
        private readonly string sourceFileID;

        internal SourceFileReference(string sourceFileID)
        {            
            this.sourceFileID = sourceFileID;
        }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string SourceFileID
        {
            get { return sourceFileID; }
        }

    }

    public abstract class RunReference : SourceFileReference
    {

        private readonly string runID;

        internal RunReference(string runID)
            : this(null, runID) { }

        internal RunReference(
            string sourceFileID,
            string runID)
            : base(sourceFileID)
        {
            if (string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");
            this.runID = runID;
        }

        [JsonProperty(Required = Required.Always)]
        public string RunID
        {
            get { return runID; }
        }

        public bool IsExternal
        {
            get { return SourceFileID != null; }
        }

    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class SpectrumReference : RunReference
    {

        private readonly string spectrumID;

        public SpectrumReference(string runID, string spectrumID)
            : this(null, runID, spectrumID) { }

        [JsonConstructor]
        public SpectrumReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("RunID")] string runID,
            [JsonProperty("SpectrumID")] string spectrumID)
            : base(sourceFileID, runID)
        {
            if (string.IsNullOrWhiteSpace(spectrumID))
                throw new ArgumentNullException("spectrumID");
            this.spectrumID = spectrumID;
        }

        [JsonProperty(Required = Required.Always)]
        public string SpectrumID
        {
            get { return spectrumID; }
        }
    }
}
