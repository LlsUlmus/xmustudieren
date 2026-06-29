using System.ComponentModel.DataAnnotations;

namespace Ricebird.Configuration.Models
{
    public class Configuration : EntityBase
    {
        [MaxLength(50)]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;

    }
}
