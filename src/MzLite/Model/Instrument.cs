#region license
// The MIT License (MIT)

// Instrument.cs

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
    /// Expansible description of the hardware configuration of a mass spectrometer.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Instrument : ModelItem
    {
        
        private Software software;

        private readonly ComponentList components = new ComponentList();

        [JsonConstructor]
        public Instrument([JsonProperty("ID")] string id) : base(id) { }

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

        [JsonProperty]
        public ComponentList Components { get { return components; } }
    }

    /// <summary>
    /// The model item container for instrument configurations.
    /// </summary>
    [JsonArray]
    public sealed class InstrumentList : ObservableModelItemCollection<Instrument>
    {
        [JsonConstructor]
        internal InstrumentList() : base() { }
    }
    
    /// <summary>
    /// Expansible description of a mass spectrometer component.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Component : ParamContainer
    {
        [JsonConstructor]
        protected Component() { }
    }

    /// <summary>
    /// A source component.
    /// </summary>
    public sealed class SourceComponent : Component { }

    /// <summary>
    /// A mass analyzer (or mass filter) component.
    /// </summary>
    public sealed class AnalyzerComponent : Component { }

    /// <summary>
    /// A detector component.
    /// </summary>
    public sealed class DetectorComponent : Component { }

    /// <summary>
    /// The container for mass spectrometer components.
    /// </summary>
    [JsonArray(ItemTypeNameHandling = TypeNameHandling.All)]
    public sealed class ComponentList : ObservableCollection<Component>
    {
        [JsonConstructor]
        internal ComponentList() : base() { }
    }
}
