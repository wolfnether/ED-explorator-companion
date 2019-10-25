using System.ComponentModel.DataAnnotations;

namespace ED_Explorator_Companion
{
    internal class ConfigPair
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}