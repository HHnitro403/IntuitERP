using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IntuiERP.Avalonia.UI.models
{

    [Table("itens_compra")]
    public class ItemCompraModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cod_compra")]
        public int CodCompra { get; set; }

        [Column("cod_produto")]
        public int CodProduto { get; set; }

        [Column("quantidade")]
        public int quantidade { get; set; }

        [Column("preco_unitario")]
        public decimal preco_unitario { get; set; }

        [Column("subtotal")]
        public decimal subtotal { get; set; }

        // Navigation properties
        [ForeignKey("CodCompra")]
        public virtual CompraModel? Compra { get; set; }

        [ForeignKey("CodProduto")]
        public virtual ProdutoModel? Produto { get; set; }
    }
}
