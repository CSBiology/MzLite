using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a processing software.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public class Software : ProjectItem
    {

        private Software() : base() { }

        public Software(string name) : base(name) { }


    }

    /// <summary>
    /// The project item container for processing software.
    /// </summary>
    [JsonArray]
    public sealed class SoftwareList : ProjectItemContainer<Software>
    {
        [JsonConstructor]
        internal SoftwareList() : base() { }
    }
}
