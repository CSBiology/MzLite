using System;
using Newtonsoft.Json;

namespace MzLite.Model
{    

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SourceFileReference
    {
        
        private readonly string sourceFileID;
        
        [JsonConstructor]
        public SourceFileReference([JsonProperty("SourceFileID")] string sourceFileID) 
        {
            if (string.IsNullOrWhiteSpace(sourceFileID))
                throw new ArgumentNullException("sourceFileID");
            this.sourceFileID = sourceFileID;
        }
        
        [JsonProperty(Required=Required.Always)]
        public string SourceFileID
        {
            get { return sourceFileID; }
        }

    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RunReference : SourceFileReference
    {

        public const string SourceFileLocal = "#local";

        private readonly string runID;

        public RunReference(SourceFileReference sfref, string runID) 
            : this(sfref.SourceFileID, runID)
        { 
        }

        public RunReference(string runID)
            : this(SourceFileLocal, runID) { }

        [JsonConstructor]
        public RunReference(
            [JsonProperty("SourceFileID")] string sourceFileID, 
            [JsonProperty("RunID")] string runID) 
            : base(sourceFileID)
        {            
            if (string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");
            this.runID = runID;
        }
        
        [JsonProperty(Required=Required.Always)]
        public string RunID
        {
            get { return runID; }
        }        

        public bool IsExternal
        {
            get { return SourceFileID != SourceFileLocal; }
        }
        
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SpectrumReference : RunReference
    {

        private readonly string spectrumID;

        public SpectrumReference(RunReference rref, string spectrumID)
            : this(rref.SourceFileID, rref.RunID, spectrumID) { }

        public SpectrumReference(string runID, string spectrumID)
            : this(RunReference.SourceFileLocal, runID, spectrumID) { }

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
