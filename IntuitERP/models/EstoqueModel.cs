using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{

    [Table("estoque")]
    public class EstoqueModel
    {
        [Key]
        [Column("CodEst")]
        public int CodEst { get; set; }

        [Column("CodProduto")]
        [Required]
        public int CodProduto { get; set; }

        [Column("Tipo")]
        public char? Tipo { get; set; }

        [Column("Qtd")]
        public decimal? Qtd { get; set; }

        [Column("Data")]
        [Required]
        public DateTime Data { get; set; }

        // Navigation properties
        [ForeignKey("CodProduto")]
        public virtual ProdutoModel? Produto { get; set; }
    }
}
