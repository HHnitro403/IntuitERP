using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBConfigurator.Avalonia.UI.Models
{
    [Table("Connection")]
    public class Configuration
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Server { get; set; } = string.Empty;

        [Required]
        public int Port { get; set; } = 5432;

        [Required]
        public string DataBase { get; set; } = string.Empty;

        [Required]
        public string User { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
