using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("itensvenda")]
    public class ItemVendaModel
    {
        [Key]
        [Column("CodItem")]
        public int CodItem { get; set; }

        [Column("CodVenda")]
        public int? CodVenda { get; set; }

        [Column("CodProduto")]
        public int? CodProduto { get; set; }

        [Column("Descricao")]
        public string? Descricao { get; set; }

        [Column("quantidade")]
        public int? Quantidade { get; set; }

        [Column("valor_unitario", TypeName = "decimal(10,2)")]
        public decimal? ValorUnitario { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [Column("desconto", TypeName = "decimal(10,2)")]
        public decimal? Desconto { get; set; }

        // Navigation properties
        [ForeignKey("CodVenda")]
        public virtual VendaModel? Venda { get; set; }

        [ForeignKey("CodProduto")]
        public virtual ProdutoModel? Produto { get; set; }
    }
}
