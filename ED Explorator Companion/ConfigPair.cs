using LiteDB;

namespace ED_Explorator_Companion
{
    internal class ConfigPair
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}