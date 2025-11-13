-- ============================================================================
-- IntuitERP - Permission Setup Script
-- ============================================================================
-- This script provides templates for setting up different permission levels
-- for users in the IntuitERP system.
--
-- Permission Levels:
-- 1. Administrator - Full access to everything
-- 2. Manager - Can manage products, sales, clients, but not users
-- 3. Sales Person - Can view and create sales, view products/clients
-- 4. Viewer - Read-only access to most areas
-- 5. Custom - Template for custom permission combinations
-- ============================================================================

-- ============================================================================
-- LEVEL 1: ADMINISTRATOR (Full Access)
-- ============================================================================
-- Administrators have all permissions including user management capability
-- They can perform all CRUD operations on all entities

UPDATE usuarios
SET
    -- Products
    PermissaoProdutosCreate = 1,
    PermissaoProdutosRead = 1,
    PermissaoProdutosUpdate = 1,
    PermissaoProdutosDelete = 1,

    -- Sales
    PermissaoVendasCreate = 1,
    PermissaoVendasRead = 1,
    PermissaoVendasUpdate = 1,
    PermissaoVendasDelete = 1,

    -- Clients
    PermissaoClientesCreate = 1,
    PermissaoClientesRead = 1,
    PermissaoClientesUpdate = 1,
    PermissaoClientesDelete = 1,

    -- Sellers
    PermissaoVendedoresCreate = 1,
    PermissaoVendedoresRead = 1,
    PermissaoVendedoresUpdate = 1,
    PermissaoVendedoresDelete = 1,

    -- Suppliers
    PermissaoFornecedoresCreate = 1,
    PermissaoFornecedoresRead = 1,
    PermissaoFornecedoresUpdate = 1,
    PermissaoFornecedoresDelete = 1,

    -- Reports
    PermissaoRelatoriosGenerate = 1
WHERE Usuario = 'admin'; -- Replace 'admin' with your administrator username

-- ============================================================================
-- LEVEL 2: MANAGER (Management Access)
-- ============================================================================
-- Managers can manage products, sales, clients, suppliers, and sellers
-- They can view and generate reports
-- They CANNOT manage users (no admin access)

UPDATE usuarios
SET
    -- Products - Full access
    PermissaoProdutosCreate = 1,
    PermissaoProdutosRead = 1,
    PermissaoProdutosUpdate = 1,
    PermissaoProdutosDelete = 1,

    -- Sales - Full access
    PermissaoVendasCreate = 1,
    PermissaoVendasRead = 1,
    PermissaoVendasUpdate = 1,
    PermissaoVendasDelete = 1,

    -- Clients - Full access
    PermissaoClientesCreate = 1,
    PermissaoClientesRead = 1,
    PermissaoClientesUpdate = 1,
    PermissaoClientesDelete = 1,

    -- Sellers - Full access
    PermissaoVendedoresCreate = 1,
    PermissaoVendedoresRead = 1,
    PermissaoVendedoresUpdate = 1,
    PermissaoVendedoresDelete = 1,

    -- Suppliers - Full access
    PermissaoFornecedoresCreate = 1,
    PermissaoFornecedoresRead = 1,
    PermissaoFornecedoresUpdate = 1,
    PermissaoFornecedoresDelete = 1,

    -- Reports - Can generate
    PermissaoRelatoriosGenerate = 1
WHERE Usuario = 'manager'; -- Replace 'manager' with your manager username

-- ============================================================================
-- LEVEL 3: SALES PERSON (Sales-Focused Access)
-- ============================================================================
-- Sales people can create and view sales
-- They can view (but not modify) products, clients, and sellers
-- They cannot access suppliers or reports
-- They cannot delete anything

UPDATE usuarios
SET
    -- Products - Read only
    PermissaoProdutosCreate = 0,
    PermissaoProdutosRead = 1,
    PermissaoProdutosUpdate = 0,
    PermissaoProdutosDelete = 0,

    -- Sales - Create and Read only
    PermissaoVendasCreate = 1,
    PermissaoVendasRead = 1,
    PermissaoVendasUpdate = 0,
    PermissaoVendasDelete = 0,

    -- Clients - Read only
    PermissaoClientesCreate = 0,
    PermissaoClientesRead = 1,
    PermissaoClientesUpdate = 0,
    PermissaoClientesDelete = 0,

    -- Sellers - Read only
    PermissaoVendedoresCreate = 0,
    PermissaoVendedoresRead = 1,
    PermissaoVendedoresUpdate = 0,
    PermissaoVendedoresDelete = 0,

    -- Suppliers - No access
    PermissaoFornecedoresCreate = 0,
    PermissaoFornecedoresRead = 0,
    PermissaoFornecedoresUpdate = 0,
    PermissaoFornecedoresDelete = 0,

    -- Reports - No access
    PermissaoRelatoriosGenerate = 0
WHERE Usuario = 'salesperson'; -- Replace 'salesperson' with your sales person username

-- ============================================================================
-- LEVEL 4: VIEWER (Read-Only Access)
-- ============================================================================
-- Viewers can only read data, cannot create, update, or delete anything
-- Useful for analysts, auditors, or reporting staff

UPDATE usuarios
SET
    -- Products - Read only
    PermissaoProdutosCreate = 0,
    PermissaoProdutosRead = 1,
    PermissaoProdutosUpdate = 0,
    PermissaoProdutosDelete = 0,

    -- Sales - Read only
    PermissaoVendasCreate = 0,
    PermissaoVendasRead = 1,
    PermissaoVendasUpdate = 0,
    PermissaoVendasDelete = 0,

    -- Clients - Read only
    PermissaoClientesCreate = 0,
    PermissaoClientesRead = 1,
    PermissaoClientesUpdate = 0,
    PermissaoClientesDelete = 0,

    -- Sellers - Read only
    PermissaoVendedoresCreate = 0,
    PermissaoVendedoresRead = 1,
    PermissaoVendedoresUpdate = 0,
    PermissaoVendedoresDelete = 0,

    -- Suppliers - Read only
    PermissaoFornecedoresCreate = 0,
    PermissaoFornecedoresRead = 1,
    PermissaoFornecedoresUpdate = 0,
    PermissaoFornecedoresDelete = 0,

    -- Reports - Can generate
    PermissaoRelatoriosGenerate = 1
WHERE Usuario = 'viewer'; -- Replace 'viewer' with your viewer username

-- ============================================================================
-- LEVEL 5: MINIMAL ACCESS (Can Only Open Software)
-- ============================================================================
-- Users with minimal access can log in but have no permissions
-- They will see "Access Denied" messages when trying to access any module
-- Useful for temporary or restricted accounts

UPDATE usuarios
SET
    -- Products - No access
    PermissaoProdutosCreate = 0,
    PermissaoProdutosRead = 0,
    PermissaoProdutosUpdate = 0,
    PermissaoProdutosDelete = 0,

    -- Sales - No access
    PermissaoVendasCreate = 0,
    PermissaoVendasRead = 0,
    PermissaoVendasUpdate = 0,
    PermissaoVendasDelete = 0,

    -- Clients - No access
    PermissaoClientesCreate = 0,
    PermissaoClientesRead = 0,
    PermissaoClientesUpdate = 0,
    PermissaoClientesDelete = 0,

    -- Sellers - No access
    PermissaoVendedoresCreate = 0,
    PermissaoVendedoresRead = 0,
    PermissaoVendedoresUpdate = 0,
    PermissaoVendedoresDelete = 0,

    -- Suppliers - No access
    PermissaoFornecedoresCreate = 0,
    PermissaoFornecedoresRead = 0,
    PermissaoFornecedoresUpdate = 0,
    PermissaoFornecedoresDelete = 0,

    -- Reports - No access
    PermissaoRelatoriosGenerate = 0
WHERE Usuario = 'restricted'; -- Replace 'restricted' with your restricted username

-- ============================================================================
-- CUSTOM PERMISSION TEMPLATE
-- ============================================================================
-- Copy and modify this template for custom permission combinations
-- Set each permission to 1 (granted) or 0 (denied) as needed

/*
UPDATE usuarios
SET
    -- Products
    PermissaoProdutosCreate = 0,  -- Can create products?
    PermissaoProdutosRead = 1,    -- Can view products?
    PermissaoProdutosUpdate = 0,  -- Can edit products?
    PermissaoProdutosDelete = 0,  -- Can delete products?

    -- Sales
    PermissaoVendasCreate = 1,    -- Can create sales?
    PermissaoVendasRead = 1,      -- Can view sales?
    PermissaoVendasUpdate = 1,    -- Can edit sales?
    PermissaoVendasDelete = 0,    -- Can delete sales?

    -- Clients
    PermissaoClientesCreate = 1,  -- Can create clients?
    PermissaoClientesRead = 1,    -- Can view clients?
    PermissaoClientesUpdate = 1,  -- Can edit clients?
    PermissaoClientesDelete = 0,  -- Can delete clients?

    -- Sellers
    PermissaoVendedoresCreate = 0,  -- Can create sellers?
    PermissaoVendedoresRead = 1,    -- Can view sellers?
    PermissaoVendedoresUpdate = 0,  -- Can edit sellers?
    PermissaoVendedoresDelete = 0,  -- Can delete sellers?

    -- Suppliers
    PermissaoFornecedoresCreate = 0,  -- Can create suppliers?
    PermissaoFornecedoresRead = 1,    -- Can view suppliers?
    PermissaoFornecedoresUpdate = 0,  -- Can edit suppliers?
    PermissaoFornecedoresDelete = 0,  -- Can delete suppliers?

    -- Reports
    PermissaoRelatoriosGenerate = 1  -- Can generate reports?
WHERE Usuario = 'custom_user'; -- Replace with actual username
*/

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================
-- Use these queries to verify permission assignments

-- View all users and their permission summary
SELECT
    Usuario,
    CASE
        WHEN PermissaoProdutosCreate + PermissaoProdutosRead + PermissaoProdutosUpdate + PermissaoProdutosDelete +
             PermissaoVendasCreate + PermissaoVendasRead + PermissaoVendasUpdate + PermissaoVendasDelete +
             PermissaoClientesCreate + PermissaoClientesRead + PermissaoClientesUpdate + PermissaoClientesDelete +
             PermissaoVendedoresCreate + PermissaoVendedoresRead + PermissaoVendedoresUpdate + PermissaoVendedoresDelete +
             PermissaoFornecedoresCreate + PermissaoFornecedoresRead + PermissaoFornecedoresUpdate + PermissaoFornecedoresDelete +
             PermissaoRelatoriosGenerate = 21 THEN 'ADMINISTRATOR'
        WHEN PermissaoProdutosCreate + PermissaoProdutosRead + PermissaoProdutosUpdate + PermissaoProdutosDelete +
             PermissaoVendasCreate + PermissaoVendasRead + PermissaoVendasUpdate + PermissaoVendasDelete +
             PermissaoClientesCreate + PermissaoClientesRead + PermissaoClientesUpdate + PermissaoClientesDelete +
             PermissaoVendedoresCreate + PermissaoVendedoresRead + PermissaoVendedoresUpdate + PermissaoVendedoresDelete +
             PermissaoFornecedoresCreate + PermissaoFornecedoresRead + PermissaoFornecedoresUpdate + PermissaoFornecedoresDelete +
             PermissaoRelatoriosGenerate = 0 THEN 'NO ACCESS'
        ELSE 'CUSTOM'
    END AS PermissionLevel,
    -- Product permissions
    CONCAT_WS(',',
        IF(PermissaoProdutosCreate, 'P:C', NULL),
        IF(PermissaoProdutosRead, 'P:R', NULL),
        IF(PermissaoProdutosUpdate, 'P:U', NULL),
        IF(PermissaoProdutosDelete, 'P:D', NULL)
    ) AS Products,
    -- Sales permissions
    CONCAT_WS(',',
        IF(PermissaoVendasCreate, 'V:C', NULL),
        IF(PermissaoVendasRead, 'V:R', NULL),
        IF(PermissaoVendasUpdate, 'V:U', NULL),
        IF(PermissaoVendasDelete, 'V:D', NULL)
    ) AS Sales,
    -- Client permissions
    CONCAT_WS(',',
        IF(PermissaoClientesCreate, 'Cl:C', NULL),
        IF(PermissaoClientesRead, 'Cl:R', NULL),
        IF(PermissaoClientesUpdate, 'Cl:U', NULL),
        IF(PermissaoClientesDelete, 'Cl:D', NULL)
    ) AS Clients,
    -- Reports permission
    IF(PermissaoRelatoriosGenerate, 'Yes', 'No') AS Reports
FROM usuarios
ORDER BY Usuario;

-- View specific user permissions
SELECT
    Usuario,
    -- Products
    PermissaoProdutosCreate AS Prod_Create,
    PermissaoProdutosRead AS Prod_Read,
    PermissaoProdutosUpdate AS Prod_Update,
    PermissaoProdutosDelete AS Prod_Delete,
    -- Sales
    PermissaoVendasCreate AS Sales_Create,
    PermissaoVendasRead AS Sales_Read,
    PermissaoVendasUpdate AS Sales_Update,
    PermissaoVendasDelete AS Sales_Delete,
    -- Clients
    PermissaoClientesCreate AS Client_Create,
    PermissaoClientesRead AS Client_Read,
    PermissaoClientesUpdate AS Client_Update,
    PermissaoClientesDelete AS Client_Delete,
    -- Sellers
    PermissaoVendedoresCreate AS Seller_Create,
    PermissaoVendedoresRead AS Seller_Read,
    PermissaoVendedoresUpdate AS Seller_Update,
    PermissaoVendedoresDelete AS Seller_Delete,
    -- Suppliers
    PermissaoFornecedoresCreate AS Supplier_Create,
    PermissaoFornecedoresRead AS Supplier_Read,
    PermissaoFornecedoresUpdate AS Supplier_Update,
    PermissaoFornecedoresDelete AS Supplier_Delete,
    -- Reports
    PermissaoRelatoriosGenerate AS Reports
FROM usuarios
WHERE Usuario = 'your_username'; -- Replace with username to check

-- ============================================================================
-- NOTES
-- ============================================================================
-- 1. After running any UPDATE statement, existing users must log out and log back in
--    for permission changes to take effect
--
-- 2. Administrator Access: Only users with ALL permissions can manage other users
--    (access the User Management module)
--
-- 3. Purchase Module: Uses Supplier permissions (if user can read suppliers,
--    they can access purchases)
--
-- 4. Cities Module: Currently has no permission checks (available to all logged-in users)
--    Consider adding permissions if needed
--
-- 5. Inventory/Stock Module: Currently uses Product permissions
--
-- 6. Permission values: 1 = Granted, 0 = Denied
--
-- 7. Always keep at least one Administrator account active to prevent lockout
--
-- ============================================================================
