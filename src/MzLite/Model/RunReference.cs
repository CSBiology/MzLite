#region license
// The MIT License (MIT)

// RunReference.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany
 

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
