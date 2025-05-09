using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("usuarios")]
    public class UsuarioModel
    {
        [Key]
        [Column("CodUsuarios")]
        public int CodUsuarios { get; set; }

        [Column("Usuario")]
        [Required]
        [StringLength(255)]
        public string UsuarioNome { get; set; }

        [Column("Senha")]
        [Required]
        [StringLength(255)]
        public string Senha { get; set; }

        [Column("PermissaoProdutosCreate")]
        public bool PermissaoProdutosCreate { get; set; } = false;

        [Column("PermissaoProdutosRead")]
        public bool PermissaoProdutosRead { get; set; } = false;

        [Column("PermissaoProdutosUpdate")]
        public bool PermissaoProdutosUpdate { get; set; } = false;

        [Column("PermissaoProdutosDelete")]
        public bool PermissaoProdutosDelete { get; set; } = false;

        [Column("PermissaoVendasCreate")]
        public bool PermissaoVendasCreate { get; set; } = false;

        [Column("PermissaoVendasRead")]
        public bool PermissaoVendasRead { get; set; } = false;

        [Column("PermissaoVendasUpdate")]
        public bool PermissaoVendasUpdate { get; set; } = false;

        [Column("PermissaoVendasDelete")]
        public bool PermissaoVendasDelete { get; set; } = false;

        [Column("PermissaoRelatoriosGenerate")]
        public bool PermissaoRelatoriosGenerate { get; set; } = false;

        [Column("PermissaoVendedoresCreate")]
        public bool PermissaoVendedoresCreate { get; set; } = false;

        [Column("PermissaoVendedoresRead")]
        public bool PermissaoVendedoresRead { get; set; } = false;

        [Column("PermissaoVendedoresUpdate")]
        public bool PermissaoVendedoresUpdate { get; set; } = false;

        [Column("PermissaoVendedoresDelete")]
        public bool PermissaoVendedoresDelete { get; set; } = false;

        [Column("PermissaoFornecedoresCreate")]
        public bool PermissaoFornecedoresCreate { get; set; } = false;

        [Column("PermissaoFornecedoresRead")]
        public bool PermissaoFornecedoresRead { get; set; } = false;

        [Column("PermissaoFornecedoresUpdate")]
        public bool PermissaoFornecedoresUpdate { get; set; } = false;

        [Column("PermissaoFornecedoresDelete")]
        public bool PermissaoFornecedoresDelete { get; set; } = false;

        [Column("PermissaoClientesCreate")]
        public bool PermissaoClientesCreate { get; set; } = false;

        [Column("PermissaoClientesRead")]
        public bool PermissaoClientesRead { get; set; } = false;

        [Column("PermissaoClientesUpdate")]
        public bool PermissaoClientesUpdate { get; set; } = false;

        [Column("PermissaoClientesDelete")]
        public bool PermissaoClientesDelete { get; set; } = false;
    }

}

