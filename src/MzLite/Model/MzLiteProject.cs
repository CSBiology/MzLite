using Newtonsoft.Json;

namespace MzLite.Model
{

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MzLiteProject : NamedItem
    {

        private readonly SourceFileList sourceFiles = new SourceFileList();
        private readonly SampleList samples = new SampleList();
        private readonly ProjectRunList runs = new ProjectRunList();

        [JsonConstructor]
        public MzLiteProject([JsonProperty("Name")] string name) : base(name) { }

        [JsonProperty]
        public SourceFileList SourceFiles { get { return sourceFiles; } }

        [JsonProperty]
        public SampleList Samples { get { return samples; } }

        [JsonProperty]
        public ProjectRunList Runs { get { return runs; } }
    }
   
}
