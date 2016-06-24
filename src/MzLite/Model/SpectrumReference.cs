#region license
// The MIT License (MIT)

// SpectrumReference.cs

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
