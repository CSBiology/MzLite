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
