using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("usuarios")]
    public class UsuarioModel
    {
        [Key]
        [Column("cod_usuarios")]
        public int CodUsuarios { get; set; }

        [Column("usuario")]
        [Required]
        [StringLength(255)]
        public string Usuario { get; set; }

        [Column("senha")]
        [Required]
        [StringLength(255)]
        public string Senha { get; set; }

        [Column("permissao_produtos_create")]
        public int PermissaoProdutosCreate { get; set; } = 0;

        [Column("permissao_produtos_read")]
        public int PermissaoProdutosRead { get; set; } = 0;

        [Column("permissao_produtos_update")]
        public int PermissaoProdutosUpdate { get; set; } = 0;

        [Column("permissao_produtos_delete")]
        public int PermissaoProdutosDelete { get; set; } = 0;

        [Column("permissao_vendas_create")]
        public int PermissaoVendasCreate { get; set; } = 0;

        [Column("permissao_vendas_read")]
        public int PermissaoVendasRead { get; set; } = 0;

        [Column("permissao_vendas_update")]
        public int PermissaoVendasUpdate { get; set; } = 0;

        [Column("permissao_vendas_delete")]
        public int PermissaoVendasDelete { get; set; } = 0;

        [Column("permissao_relatorios_generate")]
        public int PermissaoRelatoriosGenerate { get; set; } = 0;

        [Column("permissao_vendedores_create")]
        public int PermissaoVendedoresCreate { get; set; } = 0;

        [Column("permissao_vendedores_read")]
        public int PermissaoVendedoresRead { get; set; } = 0;

        [Column("permissao_vendedores_update")]
        public int PermissaoVendedoresUpdate { get; set; } = 0;

        [Column("permissao_vendedores_delete")]
        public int PermissaoVendedoresDelete { get; set; } = 0;

        [Column("permissao_fornecedores_create")]
        public int PermissaoFornecedoresCreate { get; set; } = 0;

        [Column("permissao_fornecedores_read")]
        public int PermissaoFornecedoresRead { get; set; } = 0;

        [Column("permissao_fornecedores_update")]
        public int PermissaoFornecedoresUpdate { get; set; } = 0;

        [Column("permissao_fornecedores_delete")]
        public int PermissaoFornecedoresDelete { get; set; } = 0;

        [Column("permissao_clientes_create")]
        public int PermissaoClientesCreate { get; set; } = 0;

        [Column("permissao_clientes_read")]
        public int PermissaoClientesRead { get; set; } = 0;

        [Column("permissao_clientes_update")]
        public int PermissaoClientesUpdate { get; set; } = 0;

        [Column("permissao_clientes_delete")]
        public int PermissaoClientesDelete { get; set; } = 0;
    }

}
