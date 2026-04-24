using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("itens_venda")]
    public class ItemVendaModel
    {
        [Key]
        [Column("id")]
        public int CodItem { get; set; }

        [Column("cod_venda")]
        public int? CodVenda { get; set; }

        [Column("cod_produto")]
        public int? CodProduto { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("quantidade")]
        public int? quantidade { get; set; }

        [Column("preco_unitario", TypeName = "decimal(10,2)")]
        public decimal? valor_unitario { get; set; }

        [Column("subtotal", TypeName = "decimal(10,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? valor_total { get; set; }

        // Navigation properties
        [ForeignKey("cod_venda")]
        public virtual VendaModel? Venda { get; set; }

        [ForeignKey("cod_produto")]
        public virtual ProdutoModel? Produto { get; set; }
    }
}
