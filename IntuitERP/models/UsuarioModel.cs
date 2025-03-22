using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class UsuarioModel
    {
        public int CodUsuarios { get; set; } // Primary key, auto-incremented
        public string Usuario { get; set; } // Username
        public string Senha { get; set; } // Password (should be hashed in practice)

        // Permissions for Produtos
        public bool PermissaoProdutosCreate { get; set; }
        public bool PermissaoProdutosRead { get; set; }
        public bool PermissaoProdutosUpdate { get; set; }
        public bool PermissaoProdutosDelete { get; set; }

        // Permissions for Vendas
        public bool PermissaoVendasCreate { get; set; }
        public bool PermissaoVendasRead { get; set; }
        public bool PermissaoVendasUpdate { get; set; }
        public bool PermissaoVendasDelete { get; set; }

        // Permissions for Relatorios (only generate)
        public bool PermissaoRelatoriosGenerate { get; set; }

        // Permissions for Vendedores
        public bool PermissaoVendedoresCreate { get; set; }
        public bool PermissaoVendedoresRead { get; set; }
        public bool PermissaoVendedoresUpdate { get; set; }
        public bool PermissaoVendedoresDelete { get; set; }

        // Permissions for Fornecedores
        public bool PermissaoFornecedoresCreate { get; set; }
        public bool PermissaoFornecedoresRead { get; set; }
        public bool PermissaoFornecedoresUpdate { get; set; }
        public bool PermissaoFornecedoresDelete { get; set; }

        // Permissions for Clientes
        public bool PermissaoClientesCreate { get; set; }
        public bool PermissaoClientesRead { get; set; }
        public bool PermissaoClientesUpdate { get; set; }
        public bool PermissaoClientesDelete { get; set; }

        // Constructor (optional)
        public UsuarioModel()
        {
            // Initialize permissions to false by default
            PermissaoProdutosCreate = false;
            PermissaoProdutosRead = false;
            PermissaoProdutosUpdate = false;
            PermissaoProdutosDelete = false;

            PermissaoVendasCreate = false;
            PermissaoVendasRead = false;
            PermissaoVendasUpdate = false;
            PermissaoVendasDelete = false;

            PermissaoRelatoriosGenerate = false;

            PermissaoVendedoresCreate = false;
            PermissaoVendedoresRead = false;
            PermissaoVendedoresUpdate = false;
            PermissaoVendedoresDelete = false;

            PermissaoFornecedoresCreate = false;
            PermissaoFornecedoresRead = false;
            PermissaoFornecedoresUpdate = false;
            PermissaoFornecedoresDelete = false;

            PermissaoClientesCreate = false;
            PermissaoClientesRead = false;
            PermissaoClientesUpdate = false;
            PermissaoClientesDelete = false;
        }
    }

}
    
