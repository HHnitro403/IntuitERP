using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class UsuarioModel
    {
        public static int CodUsuarios { get; set; } // Primary key, auto-incremented
        public static string Senha { get; set; } // Password (should be hashed in practice)

        // Permissions for Produtos
        public static bool PermissaoProdutosCreate { get; set; }
        public static bool PermissaoProdutosRead { get; set; }
        public static bool PermissaoProdutosUpdate { get; set; }
        public static bool PermissaoProdutosDelete { get; set; }

        // Permissions for Vendas
        public static bool PermissaoVendasCreate { get; set; }
        public static bool PermissaoVendasRead { get; set; }
        public static bool PermissaoVendasUpdate { get; set; }
        public static bool PermissaoVendasDelete { get; set; }

        // Permissions for Relatorios (only generate)
        public static bool PermissaoRelatoriosGenerate { get; set; }

        // Permissions for Vendedores
        public static bool PermissaoVendedoresCreate { get; set; }
        public static bool PermissaoVendedoresRead { get; set; }
        public static bool PermissaoVendedoresUpdate { get; set; }
        public static bool PermissaoVendedoresDelete { get; set; }

        // Permissions for Fornecedores
        public static bool PermissaoFornecedoresCreate { get; set; }
        public static bool PermissaoFornecedoresRead { get; set; }
        public static bool PermissaoFornecedoresUpdate { get; set; }
        public static bool PermissaoFornecedoresDelete { get; set; }

        // Permissions for Clientes
        public static bool PermissaoClientesCreate { get; set; }
        public static bool PermissaoClientesRead { get; set; }
        public static bool PermissaoClientesUpdate { get; set; }
        public static bool PermissaoClientesDelete { get; set; }

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

