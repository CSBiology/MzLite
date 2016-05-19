using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public class Software : ModelItem<string>
    {

        [JsonConstructor]
        public Software([JsonProperty("ID")] string id) : base(id) { }
    }

    /// <summary>
    /// The model item container for processing software.
    /// </summary>
    [JsonArray]
    public sealed class SoftwareList : ObservableModelItemCollection<string, Software>
    {
        [JsonConstructor]
        internal SoftwareList() : base() { }
        
    }
}
