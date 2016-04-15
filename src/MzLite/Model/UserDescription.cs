using Newtonsoft.Json;

namespace MzLite.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class UserDescription : NamedItem
    {
        public UserDescription(string name) : base(name) { }
    }

    [JsonArray]
    public sealed class UserDescriptionCollection : NotifyChangedCollection<UserDescription>
    {
        public UserDescriptionCollection() { }
    }
}
