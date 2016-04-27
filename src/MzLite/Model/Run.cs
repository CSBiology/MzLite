using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a ms run.
    /// Represents the entry point to the storage of peak lists that are result
    /// of instrument scans or data processings.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Run : ModelItem
    {        
        private Sample sample;        
        private Instrument instrument;
        private DataProcessing spectrumProcessing;
        private DataProcessing chromatogramProcessing;

        [JsonConstructor]
        public Run([JsonProperty("ID")] string id) : base(id) { }
                
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
            get { return instrument; }
            set
            {
                if (value != instrument)
                {
                    NotifyPropertyChanging("DefaultInstrument");
                    this.instrument = value;
                    NotifyPropertyChanged("DefaultInstrument");
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataProcessing DefaultSpectrumProcessing
        {
            get { return spectrumProcessing; }
            set
            {
                if (value != spectrumProcessing)
                {
                    NotifyPropertyChanging("DefaultSpectrumProcessing");
                    this.spectrumProcessing = value;
                    NotifyPropertyChanged("DefaultSpectrumProcessing");
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataProcessing DefaultChromatogramProcessing
        {
            get { return chromatogramProcessing; }
            set
            {
                if (value != chromatogramProcessing)
                {
                    NotifyPropertyChanging("DefaultChromatogramProcessing");
                    this.chromatogramProcessing = value;
                    NotifyPropertyChanged("DefaultChromatogramProcessing");
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

    
}
