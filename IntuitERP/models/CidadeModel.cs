using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("cidade")]
    public class CidadeModel
    {
        [Key]
        [Column("CodCIdade")]
        public int CodCIdade { get; set; }

        [Column("Cidade")]
        [StringLength(255)]
        public string? Cidade { get; set; }

        [Column("UF")]
        [StringLength(2)]
        public string? UF { get; set; }

        /// <summary>
        /// Returns a string representation of the CidadeModel object.
        /// </summary>
        /// <returns>A formatted string with the city's code, name, and state.</returns>
        public override string ToString()
        {
            return $"{CodCIdade}: {Cidade} - {UF}";
        }

        public string ToStringList()
        {
            return $"{Cidade} - {UF}";
        }
    }
}