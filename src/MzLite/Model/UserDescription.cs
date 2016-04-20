using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MzLite.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class UserDescription : NamedItem
    {
        public UserDescription(string name) : base(name) { }
    }

    [JsonArray]
    public sealed class UserDescriptionList : ObservableCollection<UserDescription>
    {
        [JsonConstructor]
        internal UserDescriptionList() { }
    }
}
