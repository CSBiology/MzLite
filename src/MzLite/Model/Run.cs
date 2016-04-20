using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a ms run.
    /// Represents the entry point to the storage of peak lists that are result
    /// of instrument scans or data processings.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class Run : ProjectItem
    {

        [JsonProperty("Sample")]
        private Sample sample;
        [JsonProperty("DataFile")]
        private DataFile dataFile;
        [JsonProperty("Instrument")]
        private Instrument instrument;
        [JsonProperty("SpectrumProcessing")]
        private DataProcessing spectrumProcessing;
        [JsonProperty("ChromatogramProcessing")]
        private DataProcessing chromatogramProcessing;

        private Run() : base() { }

        public Run(string name) : base(name) { }

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

        public DataFile DataFile
        {
            get { return dataFile; }
            set
            {
                if (value != dataFile)
                {
                    NotifyPropertyChanging("DataFile");
                    this.dataFile = value;
                    NotifyPropertyChanged("DataFile");
                }
            }
        }

        public Instrument Instrument
        {
            get { return instrument; }
            set
            {
                if (value != instrument)
                {
                    NotifyPropertyChanging("Instrument");
                    this.instrument = value;
                    NotifyPropertyChanged("Instrument");
                }
            }
        }

        public DataProcessing SpectrumProcessing
        {
            get { return spectrumProcessing; }
            set
            {
                if (value != spectrumProcessing)
                {
                    NotifyPropertyChanging("SpectrumProcessing");
                    this.spectrumProcessing = value;
                    NotifyPropertyChanged("SpectrumProcessing");
                }
            }
        }

        public DataProcessing ChromatogramProcessing
        {
            get { return chromatogramProcessing; }
            set
            {
                if (value != chromatogramProcessing)
                {
                    NotifyPropertyChanging("ChromatogramProcessing");
                    this.chromatogramProcessing = value;
                    NotifyPropertyChanged("ChromatogramProcessing");
                }
            }
        }
    }

    /// <summary>
    /// The project item container for ms runs.
    /// </summary>
    [JsonArray]
    public sealed class RunList : ProjectItemContainer<Run>
    {
        [JsonConstructor]
        internal RunList() : base() { }
    }

    
}
