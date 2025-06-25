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
        public string Usuario { get; set; }

        [Column("Senha")]
        [Required]
        [StringLength(255)]
        public string Senha { get; set; }

        [Column("PermissaoProdutosCreate")]
        public int PermissaoProdutosCreate { get; set; } = 0;

        [Column("PermissaoProdutosRead")]
        public int PermissaoProdutosRead { get; set; } = 0;

        [Column("PermissaoProdutosUpdate")]
        public int PermissaoProdutosUpdate { get; set; } = 0;

        [Column("PermissaoProdutosDelete")]
        public int PermissaoProdutosDelete { get; set; } = 0;

        [Column("PermissaoVendasCreate")]
        public int PermissaoVendasCreate { get; set; } = 0;

        [Column("PermissaoVendasRead")]
        public int PermissaoVendasRead { get; set; } = 0;

        [Column("PermissaoVendasUpdate")]
        public int PermissaoVendasUpdate { get; set; } = 0;

        [Column("PermissaoVendasDelete")]
        public int PermissaoVendasDelete { get; set; } = 0;

        [Column("PermissaoRelatoriosGenerate")]
        public int PermissaoRelatoriosGenerate { get; set; } = 0;

        [Column("PermissaoVendedoresCreate")]
        public int PermissaoVendedoresCreate { get; set; } = 0;

        [Column("PermissaoVendedoresRead")]
        public int PermissaoVendedoresRead { get; set; } = 0;

        [Column("PermissaoVendedoresUpdate")]
        public int PermissaoVendedoresUpdate { get; set; } = 0;

        [Column("PermissaoVendedoresDelete")]
        public int PermissaoVendedoresDelete { get; set; } = 0;

        [Column("PermissaoFornecedoresCreate")]
        public int PermissaoFornecedoresCreate { get; set; } = 0;

        [Column("PermissaoFornecedoresRead")]
        public int PermissaoFornecedoresRead { get; set; } = 0;

        [Column("PermissaoFornecedoresUpdate")]
        public int PermissaoFornecedoresUpdate { get; set; } = 0;

        [Column("PermissaoFornecedoresDelete")]
        public int PermissaoFornecedoresDelete { get; set; } = 0;

        [Column("PermissaoClientesCreate")]
        public int PermissaoClientesCreate { get; set; } = 0;

        [Column("PermissaoClientesRead")]
        public int PermissaoClientesRead { get; set; } = 0;

        [Column("PermissaoClientesUpdate")]
        public int PermissaoClientesUpdate { get; set; } = 0;

        [Column("PermissaoClientesDelete")]
        public int PermissaoClientesDelete { get; set; } = 0;
    }

}

