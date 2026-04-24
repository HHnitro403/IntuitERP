using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("vendedor")]
    public class VendedorModel
    {
        [Key]
        [Column("cod_vendedor")]
        public int CodVendedor { get; set; }

        [Column("nome_vendedor")]
        [Required]
        [StringLength(255)]
        public string NomeVendedor { get; set; }

        [Column("comissao")]
        public decimal Comissao { get; set; } = 0;

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Column("qtd_vendas")]
        public int? QtdVendas { get; set; } = 0;

        [Column("qtd_vendas_finalizadas")]
        public int? QtdVendasFinalizadas { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<VendaModel>? Vendas { get; set; }

        public virtual ICollection<CompraModel>? Compras { get; set; }

        public override string ToString()
        {
            return $"{CodVendedor}: {NomeVendedor}";
        }
    }
}
