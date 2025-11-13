namespace IntuitERP.Services
{
    /// <summary>
    /// Service for checking user permissions before operations
    /// Provides centralized authorization logic
    /// </summary>
    public class PermissionService
    {
        private readonly UserContext _userContext;

        public PermissionService()
        {
            _userContext = UserContext.Instance;
        }

        #region Products Permissions

        public bool CanCreateProduct()
        {
            return _userContext.HasPermission("PermissaoProdutosCreate");
        }

        public bool CanReadProduct()
        {
            return _userContext.HasPermission("PermissaoProdutosRead");
        }

        public bool CanUpdateProduct()
        {
            return _userContext.HasPermission("PermissaoProdutosUpdate");
        }

        public bool CanDeleteProduct()
        {
            return _userContext.HasPermission("PermissaoProdutosDelete");
        }

        public void RequireProductCreate()
        {
            _userContext.RequirePermission("PermissaoProdutosCreate");
        }

        public void RequireProductRead()
        {
            _userContext.RequirePermission("PermissaoProdutosRead");
        }

        public void RequireProductUpdate()
        {
            _userContext.RequirePermission("PermissaoProdutosUpdate");
        }

        public void RequireProductDelete()
        {
            _userContext.RequirePermission("PermissaoProdutosDelete");
        }

        #endregion

        #region Sales Permissions

        public bool CanCreateSale()
        {
            return _userContext.HasPermission("PermissaoVendasCreate");
        }

        public bool CanReadSale()
        {
            return _userContext.HasPermission("PermissaoVendasRead");
        }

        public bool CanUpdateSale()
        {
            return _userContext.HasPermission("PermissaoVendasUpdate");
        }

        public bool CanDeleteSale()
        {
            return _userContext.HasPermission("PermissaoVendasDelete");
        }

        public void RequireSaleCreate()
        {
            _userContext.RequirePermission("PermissaoVendasCreate");
        }

        public void RequireSaleRead()
        {
            _userContext.RequirePermission("PermissaoVendasRead");
        }

        public void RequireSaleUpdate()
        {
            _userContext.RequirePermission("PermissaoVendasUpdate");
        }

        public void RequireSaleDelete()
        {
            _userContext.RequirePermission("PermissaoVendasDelete");
        }

        #endregion

        #region Sellers Permissions

        public bool CanCreateSeller()
        {
            return _userContext.HasPermission("PermissaoVendedoresCreate");
        }

        public bool CanReadSeller()
        {
            return _userContext.HasPermission("PermissaoVendedoresRead");
        }

        public bool CanUpdateSeller()
        {
            return _userContext.HasPermission("PermissaoVendedoresUpdate");
        }

        public bool CanDeleteSeller()
        {
            return _userContext.HasPermission("PermissaoVendedoresDelete");
        }

        public void RequireSellerCreate()
        {
            _userContext.RequirePermission("PermissaoVendedoresCreate");
        }

        public void RequireSellerRead()
        {
            _userContext.RequirePermission("PermissaoVendedoresRead");
        }

        public void RequireSellerUpdate()
        {
            _userContext.RequirePermission("PermissaoVendedoresUpdate");
        }

        public void RequireSellerDelete()
        {
            _userContext.RequirePermission("PermissaoVendedoresDelete");
        }

        #endregion

        #region Suppliers Permissions

        public bool CanCreateSupplier()
        {
            return _userContext.HasPermission("PermissaoFornecedoresCreate");
        }

        public bool CanReadSupplier()
        {
            return _userContext.HasPermission("PermissaoFornecedoresRead");
        }

        public bool CanUpdateSupplier()
        {
            return _userContext.HasPermission("PermissaoFornecedoresUpdate");
        }

        public bool CanDeleteSupplier()
        {
            return _userContext.HasPermission("PermissaoFornecedoresDelete");
        }

        public void RequireSupplierCreate()
        {
            _userContext.RequirePermission("PermissaoFornecedoresCreate");
        }

        public void RequireSupplierRead()
        {
            _userContext.RequirePermission("PermissaoFornecedoresRead");
        }

        public void RequireSupplierUpdate()
        {
            _userContext.RequirePermission("PermissaoFornecedoresUpdate");
        }

        public void RequireSupplierDelete()
        {
            _userContext.RequirePermission("PermissaoFornecedoresDelete");
        }

        #endregion

        #region Clients Permissions

        public bool CanCreateClient()
        {
            return _userContext.HasPermission("PermissaoClientesCreate");
        }

        public bool CanReadClient()
        {
            return _userContext.HasPermission("PermissaoClientesRead");
        }

        public bool CanUpdateClient()
        {
            return _userContext.HasPermission("PermissaoClientesUpdate");
        }

        public bool CanDeleteClient()
        {
            return _userContext.HasPermission("PermissaoClientesDelete");
        }

        public void RequireClientCreate()
        {
            _userContext.RequirePermission("PermissaoClientesCreate");
        }

        public void RequireClientRead()
        {
            _userContext.RequirePermission("PermissaoClientesRead");
        }

        public void RequireClientUpdate()
        {
            _userContext.RequirePermission("PermissaoClientesUpdate");
        }

        public void RequireClientDelete()
        {
            _userContext.RequirePermission("PermissaoClientesDelete");
        }

        #endregion

        #region Reports Permissions

        public bool CanGenerateReports()
        {
            return _userContext.HasPermission("PermissaoRelatoriosGenerate");
        }

        public void RequireReportGenerate()
        {
            _userContext.RequirePermission("PermissaoRelatoriosGenerate");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return _userContext.IsAuthenticated;
        }

        /// <summary>
        /// Requires user to be authenticated
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">When user is not authenticated</exception>
        public void RequireAuthentication()
        {
            _userContext.RequireAuthentication();
        }

        /// <summary>
        /// Gets a user-friendly error message for missing permissions
        /// </summary>
        public string GetPermissionDeniedMessage(string action)
        {
            return $"Você não tem permissão para {action}. Entre em contato com o administrador do sistema.";
        }

        #endregion
    }
}
