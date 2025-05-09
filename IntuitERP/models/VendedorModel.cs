using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace IntuitERP.models
{
    [Table("vendedor")]
    public class VendedorModel
    {
        [Key]
        [Column("CodVendedor")]
        public int CodVendedor { get; set; }

        [Column("NomeVendedor")]
        [Required]
        [StringLength(255)]
        public string NomeVendedor { get; set; }

        [Column("totalvendas")]
        public int? TotalVendas { get; set; } = 0;

        [Column("vendasfinalizadas")]
        public int? VendasFinalizadas { get; set; } = 0;

        [Column("vendascanceladas")]
        public int? VendasCanceladas { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<VendaModel>? Vendas { get; set; }

        public virtual ICollection<CompraModel>? Compras { get; set; }
    }
}