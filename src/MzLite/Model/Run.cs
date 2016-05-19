using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a ms run.
    /// Represents the entry point to the storage of peak lists that are result
    /// of instrument scans or data processings.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Run : ModelItem<string>
    {
        private Sample sample;
        private Instrument defaultInstrument;
        private DataProcessing defaultSpectrumProcessing;
        private DataProcessing defaultChromatogramProcessing;

        [JsonConstructor]
        public Run([JsonProperty("ID")] string id) : base(id) { }

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
    /// The model item container for ms runs.
    /// </summary>
    [JsonArray]
    public sealed class RunList : ObservableModelItemCollection<string, Run>
    {
        [JsonConstructor]
        internal RunList() : base() { }
    }


}
