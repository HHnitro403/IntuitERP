using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace IntuitERP.models
{
    [Table("fornecedor")]
    public class FornecedorModel
    {
        [Key]
        [Column("CodFornecedor")]
        public int CodFornecedor { get; set; }

        [Column("CodCidade")]
        [Required]
        public int CodCidade { get; set; }

        [Column("RazaoSocial")]
        [StringLength(255)]
        public string? RazaoSocial { get; set; }

        [Column("NomeFantasia")]
        [StringLength(255)]
        public string? NomeFantasia { get; set; }

        [Column("CNPJ")]
        [StringLength(18)]
        public string? CNPJ { get; set; }

        [Column("Email")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("Telefone")]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [Column("Endereco")]
        [StringLength(255)]
        public string? Endereco { get; set; }

        [Column("DataCadastro")]
        public DateTime? DataCadastro { get; set; }

        [Column("DataUltimaCompra")]
        public DateTime? DataUltimaCompra { get; set; }

        [Column("Ativo")]
        public bool? Ativo { get; set; }

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