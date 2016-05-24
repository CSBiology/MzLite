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

        private readonly SourceFile sourceFile;        

        [JsonConstructor]
        public ProjectRun(
            [JsonProperty("ID")] string id,
            [JsonProperty("SourceFile")] SourceFile sourceFile) 
            : base(id) 
        {
            this.sourceFile = sourceFile;
        }

        [JsonProperty(Required=Required.Always)]
        public SourceFile SourceFile
        {
            get { return sourceFile; }            
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
