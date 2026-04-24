using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("fornecedor")]
    public class FornecedorModel
    {
        [Key]
        [Column("cod_fornecedor")]
        public int CodFornecedor { get; set; }

        [Column("cod_cidade")]
        public int? CodCidade { get; set; }

        [Column("razao_social")]
        [StringLength(255)]
        public string? RazaoSocial { get; set; }

        [Column("nome_fantasia")]
        [StringLength(255)]
        public string? NomeFantasia { get; set; }

        [Column("cnpj")]
        [StringLength(20)]
        public string? CNPJ { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("telefone")]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [Column("endereco")]
        [StringLength(255)]
        public string? Endereco { get; set; }

        [Column("numero")]
        [StringLength(50)]
        public string? Numero { get; set; }

        [Column("bairro")]
        [StringLength(100)]
        public string? Bairro { get; set; }

        [Column("cep")]
        [StringLength(20)]
        public string? CEP { get; set; }

        [Column("ativo")]
        public bool? Ativo { get; set; } = true;

        // Navigation properties
        [ForeignKey("CodCidade")]
        public virtual CidadeModel? Cidade { get; set; }

        public virtual ICollection<CompraModel>? Compras { get; set; }

        public virtual ICollection<ProdutoModel>? Produtos { get; set; }

        public override string ToString()
        {
            return $"{CodFornecedor}: {RazaoSocial}/{NomeFantasia} - {CNPJ}";
        }
    }
}
