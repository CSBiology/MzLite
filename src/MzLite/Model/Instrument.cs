using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of the hardware configuration of a mass spectrometer.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public class Instrument : ModelItem
    {

        [JsonProperty("Software", NullValueHandling = NullValueHandling.Ignore)]
        private Software software;

        private readonly ComponentList components = new ComponentList();

        private Instrument() : base() { }

        public Instrument(string id) : base(id) { }

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

    public enum ComponentType
    {
        Source, MassAnalyzer, Detector
    }

    /// <summary>
    /// Expansible description of a mass spectrometer component.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Component : NamedItem
    {

        [JsonProperty("ComponentType", Required = Required.Always)]
        private readonly ComponentType componentType;

        private Component() { }

        public Component(string name, ComponentType componentType) 
            : base(name)
        {
            this.componentType = componentType;
        }
        
        public ComponentType ComponentType { get { return componentType; } }
    }

    /// <summary>
    /// The container for mass spectrometer components.
    /// </summary>
    [JsonArray]
    public sealed class ComponentList : ObservableNamedItemCollection<Component>
    {
        [JsonConstructor]
        internal ComponentList() : base() { }
    }
}
