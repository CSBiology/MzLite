#region license
// The MIT License (MIT)

// Run.cs

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

using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Base class of a ms run which is associated to a sample description.
    /// </summary>
    public abstract class RunBase : ModelItem
    {
        private Sample sample;

        internal RunBase([JsonProperty("ID")] string id) : base(id) { }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Sample Sample
        {
            get { return sample; }
            set
            {
                if (value != sample)
                {
                    NotifyPropertyChanging("Sample");
                    this.sample = value;
                    NotifyPropertyChanged("Sample");
                }
            }
        }
    }

    /// <summary>
    /// Expansible description of a ms run in a mz data model.
    /// Represents the entry point to the storage of peak lists that are result
    /// of instrument scans or data processings.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Run : RunBase
    {
        
        private Instrument defaultInstrument;
        private DataProcessing defaultSpectrumProcessing;
        private DataProcessing defaultChromatogramProcessing;

        [JsonConstructor]
        public Run([JsonProperty("ID")] string id) : base(id) { }        

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Instrument DefaultInstrument
        {
            get { return defaultInstrument; }
            set
            {
                if (value != defaultInstrument)
                {
                    NotifyPropertyChanging("DefaultInstrument");
                    this.defaultInstrument = value;
                    NotifyPropertyChanged("DefaultInstrument");
                }
            }
        }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataProcessing DefaultSpectrumProcessing
        {
            get { return defaultSpectrumProcessing; }
            set
            {
                if (value != defaultSpectrumProcessing)
                {
                    NotifyPropertyChanging("DefaultSpectrumProcessing");
                    this.defaultSpectrumProcessing = value;
                    NotifyPropertyChanged("DefaultSpectrumProcessing");
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataProcessing DefaultChromatogramProcessing
        {
            get { return defaultChromatogramProcessing; }
            set
            {
                if (value != defaultChromatogramProcessing)
                {
                    NotifyPropertyChanging("DefaultChromatogramProcessing");
                    this.defaultChromatogramProcessing = value;
                    NotifyPropertyChanged("DefaultChromatogramProcessing");
                }
            }
        }
    }

    /// <summary>
    /// Expansible description of a ms run in a project model.
    /// Represents the entry point to the storage of peak lists that are result
    /// of instrument scans or data processings.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class ProjectRun : RunBase
    {

        private RunReference runReference;        

        [JsonConstructor]
        public ProjectRun(
            [JsonProperty("ID")] string id) 
            : base(id) 
        {  }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public RunReference RunReference
        {
            get { return runReference; }
            set
            {
                if (value != runReference)
                {
                    NotifyPropertyChanging("RunReference");
                    this.runReference = value;
                    NotifyPropertyChanged("RunReference");
                }
            }
        }
        
    }

    /// <summary>
    /// The model item container for ms runs.
    /// </summary>
    [JsonArray]
    public sealed class RunList : ObservableModelItemCollection<Run>
    {
        [JsonConstructor]
        internal RunList() : base() { }
    }

    /// <summary>
    /// The project item container for ms runs.
    /// </summary>
    [JsonArray]
    public sealed class ProjectRunList : ObservableModelItemCollection<ProjectRun>
    {
        [JsonConstructor]
        internal ProjectRunList() : base() { }
    }
}
