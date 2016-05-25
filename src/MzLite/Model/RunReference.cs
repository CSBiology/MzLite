using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class RunReference
    {

        private readonly SourceFile sourceFile;
        private readonly string runID;
        
        [JsonConstructor]
        public RunReference(
            [JsonProperty("SourceFile")] SourceFile sourceFile,
            [JsonProperty("RunID")] string runID)
        {

            if (sourceFile == null)
                throw new ArgumentNullException("sourceFile");
            if (runID == null)
                throw new ArgumentNullException("runID");

            this.sourceFile = sourceFile;
            this.runID = runID;
        }

        [JsonProperty(Required = Required.Always)]
        public SourceFile SourceFile
        {
            get { return sourceFile; }
        }
        
        [JsonProperty(Required = Required.Always)]
        public string RunID
        {
            get { return runID; }
        }        

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            RunReference other = obj as RunReference;
            if (other == null)
                return false;
            return runID.Equals(other.runID) && sourceFile.Equals(other.sourceFile);            
        }

        public override int GetHashCode()
        {
            return Tuple.Create(runID, sourceFile.GetHashCode()).GetHashCode();            
        }

    }
}
