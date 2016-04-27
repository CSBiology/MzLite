using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SourceFileReference
    {
        
        private readonly string sourceFileID;

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

    public abstract class PeakListReference : RunReference
    {

        private readonly string peakListID;
        private readonly PeakListType peakListType;

        internal PeakListReference(PeakListType peakListType, string runID, string peakListID)
            : this(peakListType, RunReference.SourceFileLocal, runID, peakListID) { }

        internal PeakListReference(PeakListType peakListType, string sourceFileID, string runID, string peakListID)
            : base(sourceFileID, runID)
        {
            if (string.IsNullOrWhiteSpace(peakListID))
                throw new ArgumentNullException("peakList");
            this.peakListID = peakListID;
            this.peakListType = peakListType;
        }

        [JsonProperty(Required = Required.Always)]
        public string PeakListID
        {
            get { return peakListID; }
        }

        public PeakListType PeakListType
        {
            get { return peakListType; }
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class SpectrumReference : PeakListReference
    {
        
        public SpectrumReference(string runID, string spectrumID)
            : base(PeakListType.MassSpectrum, runID, spectrumID) { }

        [JsonConstructor]
        public SpectrumReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("RunID")] string runID,
            [JsonProperty("PeakListID")] string spectrumID)
            : base(PeakListType.MassSpectrum, sourceFileID, runID, spectrumID) { }        
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ChromatogramReference : PeakListReference
    {

        public ChromatogramReference(string runID, string chromatogramID)
            : base(PeakListType.Chromatogram, runID, chromatogramID) { }

        [JsonConstructor]
        public ChromatogramReference(
            [JsonProperty("SourceFileID")] string sourceFileID,
            [JsonProperty("RunID")] string runID,
            [JsonProperty("PeakListID")] string chromatogramID)
            : base(PeakListType.Chromatogram, sourceFileID, runID, chromatogramID) { }
    }
}
