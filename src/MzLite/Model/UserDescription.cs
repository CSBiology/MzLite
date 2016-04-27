using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MzLite.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class UserDescription : NamedItem
    {

        [JsonConstructor]
        public UserDescription([JsonProperty("Name")] string name) : base(name) { }
    }

    [JsonArray]
    public sealed class UserDescriptionList : ObservableCollection<UserDescription>
    {
        [JsonConstructor]
        internal UserDescriptionList() { }
    }
}
