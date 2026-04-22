using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    /// <summary>
    /// Represents a user-specific setting or preference.
    /// Users can modify their own settings.
    /// </summary>
    [Table("user_settings")]
    public class UserSettingModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("cod_usuario")]
        public int CodUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("setting_key")]
        public string SettingKey { get; set; }

        [Required]
        [Column("setting_value")]
        public string SettingValue { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("setting_type")]
        public string SettingType { get; set; } // string, int, decimal, boolean, json

        [Required]
        [MaxLength(50)]
        [Column("category")]
        public string Category { get; set; } // ui, workflow, reports, accessibility

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CodUsuario")]
        public virtual UsuarioModel Usuario { get; set; }

        /// <summary>
        /// Gets the typed value of the setting based on SettingType
        /// </summary>
        public T GetValue<T>()
        {
            if (string.IsNullOrEmpty(SettingValue))
                return default(T);

            try
            {
                var targetType = typeof(T);

                // Handle nullable types
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                // Special handling for booleans
                if (targetType == typeof(bool))
                {
                    return (T)(object)(SettingValue.ToLower() == "true" || SettingValue == "1");
                }

                // Special handling for JSON arrays/objects
                if (SettingType == "json")
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(SettingValue);
                }

                // Standard type conversion
                return (T)Convert.ChangeType(SettingValue, targetType);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the setting value from a typed object
        /// </summary>
        public void SetValue<T>(T value)
        {
            if (value == null)
            {
                SettingValue = string.Empty;
                return;
            }

            // JSON serialization for complex types
            if (SettingType == "json")
            {
                SettingValue = System.Text.Json.JsonSerializer.Serialize(value);
            }
            else
            {
                SettingValue = value.ToString();
            }

            UpdatedAt = DateTime.Now;
        }
    }
}

