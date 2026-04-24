using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("produto")]
    public class ProdutoModel
    {
        [Key]
        [Column("cod_produto")]
        public int CodProduto { get; set; }

        [Column("descricao")]
        [StringLength(255)]
        public string? Descricao { get; set; }

        [Column("categoria")]
        [StringLength(100)]
        public string? Categoria { get; set; }

        [Column("preco_unitario", TypeName = "decimal(10, 2)")]
        public decimal? PrecoUnitario { get; set; }

        [Column("saldo_est")]
        public int? SaldoEst { get; set; }

        [Column("fornecedor_id")]
        public int? FornecedorId { get; set; }

        [Column("data_cadastro")]
        public DateTime? DataCadastro { get; set; }

        [Column("est_minimo")]
        public int? EstMinimo { get; set; }

        [Column("estoque_id")]
        public int? EstoqueId { get; set; }

        [Column("variante_id")]
        public int? VarianteId { get; set; }

        [Column("tipo")]
        [StringLength(50)]
        public string? Tipo { get; set; }

        [Column("ativo")]
        public bool? Ativo { get; set; }

        // Navigation properties
        [ForeignKey("fornecedor_id")]
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
        public int? FornecedorId { get; set; }
        public DateTime? DataCadastro { get; set; }
        public int? EstMinimo { get; set; }
        public int? EstoqueId { get; set; }
        public int? VarianteId { get; set; }
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
