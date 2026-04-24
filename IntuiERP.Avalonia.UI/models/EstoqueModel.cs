using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{

    [Table("estoque")]
    public class EstoqueModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cod_produto")]
        [Required]
        public int CodProduto { get; set; }

        [Column("tipo")]
        public char? Tipo { get; set; }

        [Column("qtd")]
        public int Qtd { get; set; }

        [Column("data")]
        [Required]
        public DateTime Data { get; set; }

        // Navigation properties
        [ForeignKey("CodProduto")]
        public virtual ProdutoModel? Produto { get; set; }

        [NotMapped]
        public string? ProdutoNome { get; set; }

        [NotMapped]
        public string? TipoDescricao { get; set; }
    }
}
