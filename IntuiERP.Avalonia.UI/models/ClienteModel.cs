using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("cliente")]
    public class ClienteModel
    {
        [Key]
        [Column("cod_cliente")]
        public int CodCliente { get; set; }

        [Column("cod_cidade")]
        public int CodCidade { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("telefone")]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [Column("data_nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Column("cpf")]
        [StringLength(14)]
        public string? CPF { get; set; }

        [Column("endereco")]
        [StringLength(255)]
        public string? Endereco { get; set; }

        [Column("numero")]
        [StringLength(255)]
        public string? Numero { get; set; }

        [Column("bairro")]
        [StringLength(255)]
        public string? Bairro { get; set; }

        [Column("cep")]
        [StringLength(255)]
        [Required]
        public string CEP { get; set; } = string.Empty;

        [Column("data_cadastro")]
        public DateTime? DataCadastro { get; set; }

        [Column("data_ultima_compra")]
        public DateTime? DataUltimaCompra { get; set; }

        [Column("ativo")]
        [Required]
        public bool Ativo { get; set; }

        // Navigation properties
        [ForeignKey("cod_cidade")]
        public virtual CidadeModel? Cidade { get; set; }

        public virtual ICollection<VendaModel>? Vendas { get; set; }

        public override string ToString()
        {
            return $"{CodCliente}: {Nome} - {CPF}";
        }
    }
}
