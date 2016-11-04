using System.ComponentModel;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Information pertaining to the entire mzML file 
    /// (i.e. not specific to any part of the data set) is stored here.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class FileDescription : INotifyPropertyChanged, INotifyPropertyChanging
    {

        private readonly FileContent fileContent = new FileContent();
        private readonly SourceFileList sourceFiles = new SourceFileList();
        private Contact contact;

        [JsonConstructor]
        public FileDescription() { }

        [JsonProperty]
        public SourceFileList SourceFiles { get { return sourceFiles; } }

        [JsonProperty(Required = Required.Always, ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public FileContent FileContent { get { return fileContent; } }

        [JsonProperty]
        public Contact Contact
        {
            get
            {
                return contact;
            }

            set
            {
                if (this.contact != value)
                {
                    NotifyPropertyChanging("Contact");
                    this.contact = value;
                    NotifyPropertyChanged("Contact");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        protected void NotifyPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion

    }

    /// <summary>
    /// This summarizes the different types of spectra that can be expected in the file. 
    /// This is expected to aid processing software in skipping files that do not contain appropriate spectrum types for it. 
    /// It should also describe the nativeID format used in the file by referring to an appropriate CV term.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class FileContent : ParamContainer { }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Contact : ParamContainer { }
}
