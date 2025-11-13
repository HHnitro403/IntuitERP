# Security Implementation Guide

## Overview

This document outlines the security improvements implemented in IntuitERP to address critical vulnerabilities and establish a secure foundation for the application.

---

## Phase 1: Critical Security Fixes (Completed)

### 1. Password Hashing with BCrypt

**Problem**: Passwords were previously stored in plain text in the database, making them vulnerable if the database was compromised.

**Solution**: Implemented BCrypt password hashing with work factor 12.

**Implementation**:
- Added `BCrypt.Net-Next` (v4.0.3) package
- Created `PasswordHashingService` (`IntuitERP/Services/PasswordHashingService.cs`)
- Updated `UsuarioService` to hash passwords on insert/update
- Modified authentication to verify hashed passwords

**Key Features**:
- Automatic password migration: Existing plain-text passwords are hashed on first login
- Automatic rehashing if work factor changes
- Secure password verification using BCrypt.Verify()

**Code Example**:
```csharp
// In UsuarioService.cs
public async Task<UsuarioModel?> AuthenticateAsync(string usuario, string senha)
{
    var user = await GetByUsernameAsync(usuario);
    if (user == null) return null;

    // Supports both hashed and plain-text (migration)
    if (_passwordHashingService.IsPasswordHashed(user.Senha))
    {
        return _passwordHashingService.VerifyPassword(senha, user.Senha) ? user : null;
    }
    else
    {
        // Auto-migrate plain text to hashed
        if (user.Senha == senha)
        {
            user.Senha = _passwordHashingService.HashPassword(senha);
            await UpdateAsync(user);
            return user;
        }
    }
    return null;
}
```

---

### 2. Removed Hardcoded Credentials

**Problem**: DBconfigurator had hardcoded credentials (`BbAdmin`/`masterkey`) in source code, visible to anyone with repository access.

**Solution**: Implemented secure credential storage using encrypted SQLite database.

**Implementation**:
- Created `AdminCredentialsService` (`DBconfigurator/Services/AdminCredentialsService.cs`)
- Credentials stored with BCrypt hashing
- Default credentials: username `admin`, password `ChangeMe123!`
- Password change functionality built-in
- Automatic warning when using default password

**DBconfigurator Default Credentials**:
```
Username: admin
Password: ChangeMe123!
```

**IMPORTANT**: Change the default password on first run!

**Features**:
- Secure password storage in `AdminCreds.db`
- Password change interface with validation
- Default password detection and warnings
- Minimum 8 character password requirement

---

### 3. Permission Enforcement System

**Problem**: User permissions were stored in the database but never checked. Any logged-in user could perform any action.

**Solution**: Implemented comprehensive permission checking system.

**Implementation**:
- Created `UserContext` singleton for session management (`IntuitERP/Services/UserContext.cs`)
- Created `PermissionService` for authorization checks (`IntuitERP/Services/PermissionService.cs`)
- Updated `MainPage` to store authenticated user in context
- Added permission checks to CRUD operations

**Available Permissions**:
- **Products**: Create, Read, Update, Delete
- **Sales**: Create, Read, Update, Delete
- **Sellers**: Create, Read, Update, Delete
- **Suppliers**: Create, Read, Update, Delete
- **Clients**: Create, Read, Update, Delete
- **Reports**: Generate

**Usage Example** (`CadastroProduto.xaml.cs`):
```csharp
private readonly PermissionService _permissionService;

protected override async void OnAppearing()
{
    // Check read permission
    if (!_permissionService.CanReadProduct())
    {
        await DisplayAlert("Access Denied",
            _permissionService.GetPermissionDeniedMessage("view products"),
            "OK");
        await Navigation.PopAsync();
        return;
    }

    // Disable save button if no create/update permission
    if (_id != 0)
    {
        SalvarProdutoButton.IsVisible = _permissionService.CanUpdateProduct();
    }
    else
    {
        SalvarProdutoButton.IsVisible = _permissionService.CanCreateProduct();
    }
}

private async void SaveButton_Clicked(object sender, EventArgs e)
{
    try
    {
        if (_id != 0)
        {
            _permissionService.RequireProductUpdate();
        }
        else
        {
            _permissionService.RequireProductCreate();
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        await DisplayAlert("Access Denied", ex.Message, "OK");
        return;
    }

    // Proceed with save operation...
}
```

---

## Implementation Details

### UserContext (Session Management)

The `UserContext` service maintains the current logged-in user throughout the application:

```csharp
// After successful login (in MainPage.xaml.cs)
var usuario = await _usuarioService.AuthenticateAsync(username, password);
if (usuario != null)
{
    UserContext.Instance.SetCurrentUser(usuario);
    // Navigate to main menu
}

// Anywhere in the app
var currentUser = UserContext.Instance.CurrentUser;
var userId = UserContext.Instance.GetCurrentUserId();
bool canEdit = UserContext.Instance.HasPermission("PermissaoProdutosUpdate");
```

### PermissionService API

The `PermissionService` provides both query and enforcement methods:

**Query Methods** (return bool):
- `CanCreateProduct()`, `CanReadProduct()`, `CanUpdateProduct()`, `CanDeleteProduct()`
- `CanCreateSale()`, `CanReadSale()`, `CanUpdateSale()`, `CanDeleteSale()`
- `CanCreateClient()`, `CanReadClient()`, `CanUpdateClient()`, `CanDeleteClient()`
- Similar methods for Sellers, Suppliers
- `CanGenerateReports()`

**Enforcement Methods** (throw UnauthorizedAccessException):
- `RequireProductCreate()`, `RequireProductRead()`, `RequireProductUpdate()`, `RequireProductDelete()`
- Similar methods for all entities
- `RequireAuthentication()`

---

## Migration Guide for Existing Data

### Password Migration

The password migration is **automatic** and transparent:

1. User logs in with their current plain-text password
2. System detects password is not hashed
3. System verifies plain-text password
4. System hashes the password using BCrypt
5. System updates the database with hashed password
6. Future logins use BCrypt verification

**No manual intervention required!**

### User Permission Setup

For existing users in the database, you'll need to manually set permissions:

```sql
-- Example: Grant all permissions to admin user
UPDATE usuarios
SET
    PermissaoProdutosCreate = 1,
    PermissaoProdutosRead = 1,
    PermissaoProdutosUpdate = 1,
    PermissaoProdutosDelete = 1,
    PermissaoVendasCreate = 1,
    PermissaoVendasRead = 1,
    PermissaoVendasUpdate = 1,
    PermissaoVendasDelete = 1,
    PermissaoClientesCreate = 1,
    PermissaoClientesRead = 1,
    PermissaoClientesUpdate = 1,
    PermissaoClientesDelete = 1,
    PermissaoVendedoresCreate = 1,
    PermissaoVendedoresRead = 1,
    PermissaoVendedoresUpdate = 1,
    PermissaoVendedoresDelete = 1,
    PermissaoFornecedoresCreate = 1,
    PermissaoFornecedoresRead = 1,
    PermissaoFornecedoresUpdate = 1,
    PermissaoFornecedoresDelete = 1,
    PermissaoRelatoriosGenerate = 1
WHERE Usuario = 'admin';

-- Example: Grant read-only access to a user
UPDATE usuarios
SET
    PermissaoProdutosRead = 1,
    PermissaoVendasRead = 1,
    PermissaoClientesRead = 1
WHERE Usuario = 'viewer';
```

---

## Applying Permission Checks to Other Pages

To add permission enforcement to other CRUD pages, follow this pattern:

**1. Add PermissionService to the page:**
```csharp
public partial class YourPage : ContentPage
{
    private readonly PermissionService _permissionService;

    public YourPage()
    {
        InitializeComponent();
        _permissionService = new PermissionService();
    }
}
```

**2. Check permissions on page load:**
```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();

    // Check read permission first
    if (!_permissionService.CanReadXXX())
    {
        await DisplayAlert("Access Denied",
            _permissionService.GetPermissionDeniedMessage("view XXX"),
            "OK");
        await Navigation.PopAsync();
        return;
    }

    // Control button visibility based on permissions
    CreateButton.IsVisible = _permissionService.CanCreateXXX();
    UpdateButton.IsVisible = _permissionService.CanUpdateXXX();
    DeleteButton.IsVisible = _permissionService.CanDeleteXXX();
}
```

**3. Check permissions before operations:**
```csharp
private async void SaveButton_Clicked(object sender, EventArgs e)
{
    try
    {
        _permissionService.RequireXXXCreate(); // or RequireXXXUpdate()
    }
    catch (UnauthorizedAccessException ex)
    {
        await DisplayAlert("Access Denied", ex.Message, "OK");
        return;
    }

    // Proceed with save...
}
```

---

## Security Best Practices

### 1. Password Requirements
- Minimum 6 characters (enforced in `MainPage.xaml.cs`)
- Consider implementing:
  - Uppercase/lowercase requirements
  - Special character requirements
  - Password expiration policy
  - Password history (prevent reuse)

### 2. Session Management
- User session is stored in memory (cleared on app restart)
- Consider adding:
  - Session timeout (auto-logout after inactivity)
  - Remember me functionality (secure token storage)
  - Concurrent session limits

### 3. Audit Logging
- Current implementation lacks audit trails
- Recommend adding:
  - Login/logout logging
  - Failed login attempt tracking
  - CRUD operation logging with user ID
  - Permission change logging

### 4. Additional Recommendations
- **Database Encryption**: Consider encrypting the MySQL database at rest
- **Connection String Security**: Move MySQL connection string to secure platform storage
- **HTTPS**: If exposing APIs, use HTTPS only
- **Input Validation**: Already implemented in validators, ensure all inputs are validated
- **SQL Injection**: Already protected via Dapper parameterization
- **Rate Limiting**: Add rate limiting for login attempts

---

## Testing the Security Implementation

### Test Password Hashing

1. Create a new user through the UI
2. Check the database: `SELECT Usuario, Senha FROM usuarios WHERE Usuario = 'newuser';`
3. Verify the password is hashed (starts with `$2a$`, `$2b$`, or `$2y$` and is 60 characters)

### Test Password Migration

1. Insert a user with plain-text password:
   ```sql
   INSERT INTO usuarios (Usuario, Senha, ...) VALUES ('testuser', 'plaintext123', ...);
   ```
2. Log in with username `testuser` and password `plaintext123`
3. Check database again - password should now be hashed

### Test Permission Enforcement

1. Create a user with limited permissions (e.g., only Products Read)
2. Log in with that user
3. Try to access Product registration page - should show read-only mode
4. Verify save button is hidden/disabled
5. Try to access other modules - should be denied if no permission

### Test DBconfigurator Security

1. Run DBconfigurator
2. Try logging in with wrong credentials - should fail
3. Log in with default credentials: `admin` / `ChangeMe123!`
4. Should see warning about default password
5. Change the password
6. Verify new password works and warning no longer appears

---

## Troubleshooting

### "Invalid username or password" after implementing changes

**Cause**: The authentication logic changed.

**Solution**: This is expected for the first login with plain-text passwords. The system will automatically hash them on successful login.

### "User does not have permission" error

**Cause**: User permissions are not set in the database.

**Solution**: Run the SQL script in the "User Permission Setup" section above to grant permissions.

### DBconfigurator won't accept default password

**Cause**: Admin credentials database file may be corrupted or missing.

**Solution**: Delete the `AdminCreds.db` file (located in `LocalApplicationData` folder) and restart the application. It will be recreated with default credentials.

---

## Files Changed/Added

### New Files
- `IntuitERP/Services/PasswordHashingService.cs` - Password hashing service
- `IntuitERP/Services/UserContext.cs` - Session management
- `IntuitERP/Services/PermissionService.cs` - Authorization service
- `DBconfigurator/Services/AdminCredentialsService.cs` - Secure credentials management
- `SECURITY.md` - This documentation

### Modified Files
- `IntuitERP/IntuitERP.csproj` - Added BCrypt.Net-Next package
- `IntuitERP/Services/UsuarioService.cs` - Password hashing and verification
- `IntuitERP/MainPage.xaml.cs` - UserContext integration
- `IntuitERP/Viwes/CadastroProduto.xaml.cs` - Permission enforcement example
- `DBconfigurator/DBconfigurator.csproj` - Added BCrypt.Net-Next package
- `DBconfigurator/MainPage.xaml.cs` - Secure authentication

---

## Next Steps (Phase 2 & 3)

While Phase 1 addressed critical security vulnerabilities, the following improvements are recommended:

### Phase 2: Stability (Urgent)
- [ ] Implement proper database connection disposal with `using` statements
- [ ] Add transaction support for multi-step operations
- [ ] Set up dependency injection in MauiProgram

### Phase 3: Quality (Important)
- [ ] Add structured logging (Serilog or Microsoft.Extensions.Logging)
- [ ] Implement comprehensive error handling strategy
- [ ] Add unit tests for security services
- [ ] Apply permission checks to all remaining CRUD pages
- [ ] Implement audit logging

---

## Support and Questions

For questions or issues related to security implementation:

1. Check this documentation first
2. Review the code examples in the "Usage Example" sections
3. Check the "Troubleshooting" section
4. Consult the inline code comments in the service files

---

**Last Updated**: 2025-11-13
**Version**: 1.0.0
**Author**: Security Implementation Phase 1
