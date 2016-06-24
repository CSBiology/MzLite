#region license
// The MIT License (MIT)

// Sample.cs

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

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a sample.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Sample : NamedModelItem
    {
        
        private readonly SampleTreatmentList treatments = new SampleTreatmentList();
        private readonly SamplePreparationList preparations = new SamplePreparationList();
        
        [JsonConstructor]
        public Sample([JsonProperty("ID")] string id, [JsonProperty("Name")] string name) : base(id, name) { }        

        [JsonProperty]
        public SamplePreparationList Preparations { get { return preparations; } }

        [JsonProperty]
        public SampleTreatmentList Treatments { get { return treatments; } }
    }

    /// <summary>
    /// The model item container for samples.
    /// </summary>
    [JsonArray]
    public sealed class SampleList : ObservableModelItemCollection<Sample>
    {
        [JsonConstructor]
        internal SampleList() : base() { }
    }

    /// <summary>
    /// Expansible description of a sample treatment.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SampleTreatment : ParamContainer
    {
        public SampleTreatment() { }
    }

    [JsonArray]
    public sealed class SampleTreatmentList : ObservableCollection<SampleTreatment> 
    {
        [JsonConstructor]
        internal SampleTreatmentList() { }
    }

    /// <summary>
    /// Expansible description of a sample preparation.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SamplePreparation : ParamContainer
    {
        public SamplePreparation() { }
    }

    [JsonArray]
    public sealed class SamplePreparationList : ObservableCollection<SamplePreparation>
    {
        [JsonConstructor]
        internal SamplePreparationList() { }
    }
    
}
