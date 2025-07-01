using Microsoft.Maui.Controls.Internals;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("cliente")]
    [Preserve(AllMembers = true)]
    public class ClienteModel
    {
        [Key]
        [Column("CodCliente")]
        public int CodCliente { get; set; }

        [Column("CodCidade")]
        public int CodCidade { get; set; }

        [Column("Nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("Email")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("Telefone")]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [Column("DataNascimento")]
        public DateTime? DataNascimento { get; set; }

        [Column("CPF")]
        [StringLength(14)]
        public string? CPF { get; set; }

        [Column("Endereco")]
        [StringLength(255)]
        public string? Endereco { get; set; }

        [Column("Numero")]
        [StringLength(255)]
        public string? Numero { get; set; }

        [Column("Bairro")]
        [StringLength(255)]
        public string? Bairro { get; set; }

        [Column("CEP")]
        [StringLength(255)]
        [Required]
        public string CEP { get; set; }

        [Column("DataCadastro")]
        public DateTime? DataCadastro { get; set; }

        [Column("DataUltimaCompra")]
        public DateTime? DataUltimaCompra { get; set; }

        [Column("Ativo")]
        [Required]
        public bool Ativo { get; set; }

        // Navigation properties
        [ForeignKey("CodCidade")]
        public virtual CidadeModel? Cidade { get; set; }

        public virtual ICollection<VendaModel>? Vendas { get; set; }

        public override string ToString()
        {
            return $"{CodCliente}: {Nome} - {CPF}";
        }
    }
}