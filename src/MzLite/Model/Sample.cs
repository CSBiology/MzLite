using System.Collections.Generic;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a biological sample.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Sample : ProjectItem
    {

        private readonly SampleTreatmentList treatments = new SampleTreatmentList();
        private readonly SamplePreparationList preparations = new SamplePreparationList();

        private Sample() : base() { }
       
        public Sample(string name) : base(name) { }

        [JsonProperty]
        SamplePreparationList Preparations { get { return preparations; } }

        [JsonProperty]
        SampleTreatmentList Treatments { get { return treatments; } }
    }

    /// <summary>
    /// The project item container for samples.
    /// </summary>
    [JsonArray]
    public sealed class SampleList : ProjectItemCollection<Sample>
    {

        [JsonConstructor]
        public SampleList() : base() { }

        public SampleList(IEnumerable<Sample> samples) : base(samples) { }

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
    public sealed class SampleTreatmentList : NotifyChangedCollection<SampleTreatment> 
    {
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
    public sealed class SamplePreparationList : NotifyChangedCollection<SamplePreparation>
    {
        internal SamplePreparationList() { }
    }
    
}
