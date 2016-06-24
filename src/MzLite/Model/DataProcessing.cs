#region license
// The MIT License (MIT)

// DataProcessing.cs

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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a data processing.
    /// Captures the processing steps applied and the use of data processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class DataProcessing : ModelItem
    {

        private readonly DataProcessingStepList processingSteps = new DataProcessingStepList();

        [JsonConstructor]
        public DataProcessing([JsonProperty("ID")] string id) : base(id) { }

        [JsonProperty]
        public DataProcessingStepList ProcessingSteps { get { return processingSteps; } }
    }

    /// <summary>
    /// The model item container for data processings.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingList : ObservableModelItemCollection<DataProcessing>
    {
        [JsonConstructor]
        internal DataProcessingList() : base() { }
    }

    /// <summary>
    /// Expansible description of a data processing step and use of processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DataProcessingStep : NamedItem
    {
        
        private Software software;
        
        [JsonConstructor]
        public DataProcessingStep([JsonProperty("Name")] string name) : base(name) {  }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Software Software 
        {
            get { return software; }
            set
            {
                if (software != value)
                {
                    NotifyPropertyChanging("Software");
                    this.software = value;
                    NotifyPropertyChanged("Software");
                }
            }
        }
    }

    /// <summary>
    /// The container for data processing steps.
    /// </summary>
    [JsonArray]
    public sealed class DataProcessingStepList : ObservableNamedItemCollection<DataProcessingStep>
    {
        [JsonConstructor]
        internal DataProcessingStepList() : base() { }

    }
}
