using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("produto")]
    public class ProdutoModel
    {
        [Key]
        [Column("CodProduto")]
        public int CodProduto { get; set; }

        [Column("Descricao")]
        [StringLength(255)]
        public string? Descricao { get; set; }

        [Column("Categoria")]
        [StringLength(100)]
        public string? Categoria { get; set; }

        [Column("PrecoUnitario", TypeName = "decimal(10, 2)")]
        public decimal? PrecoUnitario { get; set; }

        [Column("SaldoEst")]
        public int? SaldoEst { get; set; }

        [Column("FornecedorP_ID")]
        public int? FornecedorP_ID { get; set; }

        [Column("DataCadastro")]
        public DateTime? DataCadastro { get; set; }

        [Column("EstMinimo")]
        public int? EstMinimo { get; set; }

        [Column("EstoqueID")]
        public int? EstoqueID { get; set; }

        [Column("VarianteID")]
        public int? VarianteID { get; set; }

        [Column("Tipo")]
        [StringLength(50)]
        public string? Tipo { get; set; }

        [Column("Ativo")]
        public bool? Ativo { get; set; }

        // Navigation properties
        [ForeignKey("FornecedorP_ID")]
        public virtual FornecedorModel? Fornecedor { get; set; }

        public virtual ICollection<EstoqueModel>? Estoques { get; set; }

        public virtual ICollection<ItemCompraModel>? ItensCompra { get; set; }

        public virtual ICollection<ItemVendaModel>? ItensVenda { get; set; }

        public override string ToString()
        {
            return $"{CodProduto}: {Descricao} - {PrecoUnitario} - {SaldoEst}";
        }
    }

    public class ProdutoFilterModel
    {
        public int? CodProduto { get; set; }
        public string? Descricao { get; set; }
        public string? Categoria { get; set; }
        public decimal? PrecoUnitario { get; set; }
        public int? SaldoEst { get; set; }
        public int? FornecedorP_ID { get; set; }
        public DateTime? DataCadastro { get; set; }
        public int? EstMinimo { get; set; }
        public int? EstoqueID { get; set; }
        public int? VarianteID { get; set; }
        public string? Tipo { get; set; }
        public bool? Ativo { get; set; }
        public bool comparativo { get; set; } = false;
        public bool positivo { get; set; } = true;

        public override string ToString()
        {
            return $"{CodProduto}: {Descricao} - {PrecoUnitario} - {SaldoEst}";
        }
    }
}