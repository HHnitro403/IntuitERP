# Settings System Usage Guide

## Table of Contents
1. [Overview](#overview)
2. [Database Setup](#database-setup)
3. [System Settings](#system-settings)
4. [User Settings](#user-settings)
5. [Using Settings in Your Code](#using-settings-in-your-code)
6. [High-Priority Settings](#high-priority-settings)
7. [Session Timeout](#session-timeout)
8. [Testing & Validation](#testing--validation)

---

## Overview

The IntuitERP settings system provides a flexible, type-safe way to manage both system-wide and user-specific configuration. It includes:

- **System Settings**: Global configuration shared across all users
- **User Settings**: Per-user customization and preferences
- **Session Timeout**: Automatic logout for security
- **Audit Trail**: Complete history of setting changes
- **Caching**: 5-minute cache for optimal performance

### Architecture

```
┌─────────────────────────────────────┐
│     Application Layer               │
│  (Pages, ViewModels, Services)      │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│   Settings Services Layer           │
│  - SystemSettingsService (cached)   │
│  - UserSettingsService              │
│  - SessionTimeoutService            │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│   Data Models Layer                 │
│  - SystemSettingModel               │
│  - UserSettingModel                 │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│   Database Layer (MySQL)            │
│  - system_settings                  │
│  - user_settings                    │
│  - settings_audit_log               │
└─────────────────────────────────────┘
```

---

## Database Setup

### Step 1: Apply the Schema

Run the database schema to create the required tables:

```bash
mysql -u your_user -p your_database < SETTINGS_DATABASE_SCHEMA.sql
```

This creates:
- `system_settings` table with 28 default settings
- `user_settings` table for user preferences
- `settings_audit_log` table for compliance tracking

### Step 2: Verify Installation

```sql
-- Check that settings were created
SELECT COUNT(*) FROM system_settings;
-- Should return 28

-- View all settings by category
SELECT category, COUNT(*) as count
FROM system_settings
GROUP BY category
ORDER BY category;
```

Expected output:
```
+-----------------+-------+
| category        | count |
+-----------------+-------+
| business_rules  |     9 |
| formatting      |     6 |
| performance     |     4 |
| reports         |     6 |
| security        |     3 |
+-----------------+-------+
```

---

## System Settings

### Available System Settings

#### Security Settings
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `session_timeout_minutes` | int | 30 | Auto-logout after N minutes of inactivity (0 = disabled) |
| `idle_timeout_minutes` | int | 15 | Show warning after N minutes of inactivity |
| `password_min_length` | int | 8 | Minimum password length for new users |

#### Business Rules
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `payment_methods` | json | ["Dinheiro", "Cartão de Crédito", ...] | Available payment methods |
| `max_sale_value` | decimal | 9999999.99 | Maximum allowed sale value |
| `max_purchase_value` | decimal | 9999999.99 | Maximum allowed purchase value |
| `allow_negative_stock` | boolean | false | Allow sales when stock is insufficient |
| `require_client_on_sale` | boolean | true | Make client selection mandatory |
| `auto_finalize_sale` | boolean | false | Automatically mark new sales as invoiced |
| `min_stock_alert_enabled` | boolean | true | Show alerts for low stock |
| `sale_discount_max_percent` | decimal | 20.0 | Maximum discount percentage |
| `purchase_discount_max_percent` | decimal | 30.0 | Maximum purchase discount |

#### Formatting
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `date_format` | string | dd/MM/yyyy | Date display format |
| `time_format` | string | HH:mm:ss | Time display format |
| `currency_symbol` | string | R$ | Currency symbol |
| `currency_decimal_places` | int | 2 | Decimal places for currency |
| `number_decimal_separator` | string | , | Decimal separator |
| `number_thousand_separator` | string | . | Thousands separator |

#### Performance
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `recent_orders_limit` | int | 5 | Number of recent orders to show |
| `default_page_size` | int | 20 | Default items per page |
| `search_min_chars` | int | 2 | Minimum characters to trigger search |
| `cache_duration_minutes` | int | 5 | Cache duration for settings |

#### Reports
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `report_page_size` | string | A4 | Default report page size |
| `report_orientation` | string | Portrait | Default report orientation |
| `report_show_logo` | boolean | true | Show company logo on reports |
| `report_header_font_size` | int | 16 | Report header font size |
| `report_body_font_size` | int | 12 | Report body font size |
| `report_footer_text` | string | IntuitERP - Sistema... | Report footer text |

---

## User Settings

User settings are specific to each user and override system defaults.

### Common User Settings

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `theme` | string | Auto | UI theme (Light/Dark/Auto) |
| `items_per_page` | int | 20 | Preferred items per page |
| `default_view` | string | Grid | Preferred view mode (Grid/List) |
| `show_tooltips` | boolean | true | Show help tooltips |
| `notifications_enabled` | boolean | true | Enable notifications |

---

## Using Settings in Your Code

### Dependency Injection

The settings services are registered as singletons in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<SystemSettingsService>();
builder.Services.AddSingleton<UserSettingsService>();
builder.Services.AddSingleton<SessionTimeoutService>();
```

### Injecting in a Page/Service

```csharp
public class MyPage : ContentPage
{
    private readonly SystemSettingsService _systemSettings;
    private readonly UserSettingsService _userSettings;

    public MyPage(SystemSettingsService systemSettings, UserSettingsService userSettings)
    {
        InitializeComponent();
        _systemSettings = systemSettings;
        _userSettings = userSettings;
    }
}
```

### Reading System Settings

```csharp
// Generic method with type conversion
var timeout = await _systemSettings.GetSettingAsync<int>("session_timeout_minutes", 30);
var paymentMethods = await _systemSettings.GetSettingAsync<List<string>>("payment_methods", new List<string>());
var maxValue = await _systemSettings.GetSettingAsync<decimal>("max_sale_value", 9999999.99m);
var dateFormat = await _systemSettings.GetSettingAsync<string>("date_format", "dd/MM/yyyy");

// Convenience methods (recommended for high-priority settings)
var paymentMethods = await _systemSettings.GetPaymentMethodsAsync();
var timeout = await _systemSettings.GetSessionTimeoutAsync();
var maxValue = await _systemSettings.GetMaxSaleValueAsync();
var dateFormat = await _systemSettings.GetDateFormatAsync();
var recentLimit = await _systemSettings.GetRecentOrdersLimitAsync();
```

### Writing System Settings

```csharp
// Update a setting (requires user ID for audit trail)
int currentUserId = UserContext.Instance.GetCurrentUserId() ?? 0;

await _systemSettings.SetSettingAsync("session_timeout_minutes", 45, currentUserId);

// Update complex JSON setting
var newPaymentMethods = new List<string> { "Dinheiro", "PIX", "Cartão de Crédito" };
await _systemSettings.SetSettingAsync("payment_methods", newPaymentMethods, currentUserId);

// Reset to default value
await _systemSettings.ResetToDefaultAsync("session_timeout_minutes", currentUserId);
```

### Reading User Settings

```csharp
int userId = UserContext.Instance.GetCurrentUserId() ?? 0;

// Generic method
var theme = await _userSettings.GetUserSettingAsync<string>(userId, "theme", "Auto");
var itemsPerPage = await _userSettings.GetUserSettingAsync<int>(userId, "items_per_page", 20);

// Convenience methods
var theme = await _userSettings.GetUserThemeAsync(userId);
var itemsPerPage = await _userSettings.GetUserItemsPerPageAsync(userId);
```

### Writing User Settings

```csharp
int userId = UserContext.Instance.GetCurrentUserId() ?? 0;

// Create or update a user setting
await _userSettings.SetUserSettingAsync(
    userId,
    "theme",
    "Dark",
    "string",      // setting_type
    "appearance"   // category
);

// Delete a user setting (revert to system default)
await _userSettings.DeleteUserSettingAsync(userId, "theme");
```

---

## High-Priority Settings

### 1. Payment Methods

**Problem Solved**: Different businesses accept different payment methods.

**Implementation**:

```csharp
// In your sales page
protected override async void OnAppearing()
{
    base.OnAppearing();

    // Load payment methods from settings
    var paymentMethods = await _systemSettings.GetPaymentMethodsAsync();

    // Populate picker
    PaymentMethodPicker.ItemsSource = paymentMethods;
}
```

**Customization**:

```csharp
// Admin page to update payment methods
var currentMethods = await _systemSettings.GetPaymentMethodsAsync();
// Show UI to add/remove methods
// ...
var updatedMethods = new List<string> { "PIX", "Dinheiro", "Crediário" };
await _systemSettings.SetSettingAsync("payment_methods", updatedMethods, currentUserId);
```

### 2. Transaction Limits

**Problem Solved**: Prevent accidental large transactions, enforce business rules.

**Implementation**:

```csharp
// In sale validation
decimal saleTotal = CalculateSaleTotal();
decimal maxAllowed = await _systemSettings.GetMaxSaleValueAsync();

if (saleTotal > maxAllowed)
{
    await DisplayAlert("Valor Excedido",
        $"O valor total (R$ {saleTotal:N2}) excede o limite máximo permitido (R$ {maxAllowed:N2}). Consulte o supervisor.",
        "OK");
    return;
}
```

### 3. Date & Currency Formatting

**Problem Solved**: Regional differences in date/number formatting.

**Implementation**:

```csharp
// Format date according to settings
var dateFormat = await _systemSettings.GetDateFormatAsync();
var formattedDate = DateTime.Now.ToString(dateFormat); // "13/11/2025"

// Format currency
var currencySymbol = await _systemSettings.GetSettingAsync<string>("currency_symbol", "R$");
var decimalPlaces = await _systemSettings.GetSettingAsync<int>("currency_decimal_places", 2);
var formattedValue = $"{currencySymbol} {value.ToString($"N{decimalPlaces}")}"; // "R$ 1.234,56"
```

### 4. Pagination Limits

**Problem Solved**: Optimize performance for large datasets.

**Implementation**:

```csharp
// In a search/list page
int pageSize = await _systemSettings.GetSettingAsync<int>("default_page_size", 20);
int recentLimit = await _systemSettings.GetRecentOrdersLimitAsync();

// Use for query
var recentSales = await _vendaService.GetRecentAsync(recentLimit);
var pagedProducts = await _produtoService.GetPagedAsync(pageNumber, pageSize);
```

---

## Session Timeout

### How It Works

The `SessionTimeoutService` monitors user activity and triggers automatic logout after the configured timeout period.

### Setup

1. **Start Monitoring** (typically in App.xaml.cs or after login):

```csharp
public partial class App : Application
{
    private readonly SessionTimeoutService _sessionTimeout;

    public App(SessionTimeoutService sessionTimeout)
    {
        InitializeComponent();
        _sessionTimeout = sessionTimeout;

        // Subscribe to session expired event
        _sessionTimeout.SessionExpired += OnSessionExpired;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Start monitoring after user logs in
        if (UserContext.Instance.IsAuthenticated)
        {
            await _sessionTimeout.StartMonitoringAsync();
        }
    }

    private async void OnSessionExpired(object sender, EventArgs e)
    {
        // Show alert
        await MainPage.DisplayAlert("Sessão Expirada",
            "Sua sessão expirou devido à inatividade. Por favor, faça login novamente.",
            "OK");

        // Clear user context
        UserContext.Instance.ClearCurrentUser();

        // Navigate to login page
        MainPage = new LoginPage();
    }
}
```

2. **Record Activity** (in pages that indicate user activity):

```csharp
private async void SaveButton_Clicked(object sender, EventArgs e)
{
    // Record activity before processing
    _sessionTimeout?.RecordActivity();
    UserContext.Instance.RecordActivity();

    // Process the action
    await SaveData();
}
```

3. **Configure Timeout** (admin settings page):

```csharp
// Get current timeout
var currentTimeout = await _systemSettings.GetSessionTimeoutAsync();

// Update timeout (e.g., change to 60 minutes)
await _systemSettings.SetSettingAsync("session_timeout_minutes", 60, currentUserId);

// Restart monitoring with new timeout
_sessionTimeout.StopMonitoring();
await _sessionTimeout.StartMonitoringAsync();
```

4. **Disable Timeout** (set to 0):

```csharp
await _systemSettings.SetSettingAsync("session_timeout_minutes", 0, currentUserId);
// Timeout is now disabled
```

---

## Testing & Validation

### Step 1: Verify Database Installation

```sql
-- Check settings count
SELECT COUNT(*) FROM system_settings; -- Should be 28

-- Check categories
SELECT category, COUNT(*) FROM system_settings GROUP BY category;

-- View payment methods
SELECT setting_value FROM system_settings WHERE setting_key = 'payment_methods';
```

### Step 2: Test Reading Settings

Create a test page:

```csharp
public partial class SettingsTestPage : ContentPage
{
    private readonly SystemSettingsService _settings;

    public SettingsTestPage(SystemSettingsService settings)
    {
        InitializeComponent();
        _settings = settings;
    }

    private async void TestButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Test payment methods
            var methods = await _settings.GetPaymentMethodsAsync();
            await DisplayAlert("Payment Methods", string.Join(", ", methods), "OK");

            // Test timeout
            var timeout = await _settings.GetSessionTimeoutAsync();
            await DisplayAlert("Session Timeout", $"{timeout} minutes", "OK");

            // Test max value
            var maxValue = await _settings.GetMaxSaleValueAsync();
            await DisplayAlert("Max Sale Value", $"R$ {maxValue:N2}", "OK");

            // Test date format
            var dateFormat = await _settings.GetDateFormatAsync();
            var formattedDate = DateTime.Now.ToString(dateFormat);
            await DisplayAlert("Date Format", formattedDate, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
```

### Step 3: Test Writing Settings

```csharp
private async void UpdateSettingsButton_Clicked(object sender, EventArgs e)
{
    try
    {
        int userId = UserContext.Instance.GetCurrentUserId() ?? 1;

        // Update session timeout
        await _settings.SetSettingAsync("session_timeout_minutes", 45, userId);

        // Verify update
        var newTimeout = await _settings.GetSessionTimeoutAsync();
        await DisplayAlert("Success", $"Timeout updated to {newTimeout} minutes", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", ex.Message, "OK");
    }
}
```

### Step 4: Test Session Timeout

1. Set a short timeout for testing:
```csharp
await _systemSettings.SetSettingAsync("session_timeout_minutes", 1, currentUserId);
await _sessionTimeout.StartMonitoringAsync();
```

2. Wait 1 minute without activity

3. Verify that SessionExpired event fires and user is logged out

4. Restore normal timeout:
```csharp
await _systemSettings.SetSettingAsync("session_timeout_minutes", 30, currentUserId);
```

### Step 5: Test User Settings

```csharp
private async void TestUserSettingsButton_Clicked(object sender, EventArgs e)
{
    int userId = UserContext.Instance.GetCurrentUserId() ?? 1;

    // Set user theme
    await _userSettings.SetUserSettingAsync(userId, "theme", "Dark", "string", "appearance");

    // Read user theme
    var theme = await _userSettings.GetUserThemeAsync(userId);
    await DisplayAlert("User Theme", theme, "OK");

    // Delete user setting
    await _userSettings.DeleteUserSettingAsync(userId, "theme");

    // Should return default now
    theme = await _userSettings.GetUserThemeAsync(userId); // Returns "Auto"
    await DisplayAlert("Theme After Delete", theme, "OK");
}
```

---

## Common Use Cases

### Use Case 1: Customizing Payment Methods for Your Business

```csharp
// Coffee shop only accepts cash and PIX
var coffeeShopMethods = new List<string> { "Dinheiro", "PIX" };
await _systemSettings.SetSettingAsync("payment_methods", coffeeShopMethods, currentUserId);

// Tech store accepts all methods including installments
var techStoreMethods = new List<string> {
    "Dinheiro", "PIX", "Cartão de Crédito",
    "Cartão de Débito", "Crediário", "Boleto Bancário"
};
await _systemSettings.SetSettingAsync("payment_methods", techStoreMethods, currentUserId);
```

### Use Case 2: Regional Date Formatting

```csharp
// Brazilian format (default)
await _systemSettings.SetSettingAsync("date_format", "dd/MM/yyyy", currentUserId);

// US format
await _systemSettings.SetSettingAsync("date_format", "MM/dd/yyyy", currentUserId);

// ISO format
await _systemSettings.SetSettingAsync("date_format", "yyyy-MM-dd", currentUserId);
```

### Use Case 3: Implementing Per-User Themes

```csharp
// User preferences page
private async void SavePreferencesButton_Clicked(object sender, EventArgs e)
{
    int userId = UserContext.Instance.GetCurrentUserId() ?? 0;
    string selectedTheme = ThemePicker.SelectedItem.ToString(); // "Light", "Dark", or "Auto"

    await _userSettings.SetUserSettingAsync(userId, "theme", selectedTheme, "string", "appearance");

    // Apply theme immediately
    ApplyTheme(selectedTheme);

    await DisplayAlert("Sucesso", "Preferências salvas!", "OK");
}

private void ApplyTheme(string theme)
{
    Application.Current.UserAppTheme = theme switch
    {
        "Light" => AppTheme.Light,
        "Dark" => AppTheme.Dark,
        _ => AppTheme.Unspecified
    };
}
```

### Use Case 4: Enforcing Business Rules with Settings

```csharp
// Before saving a sale
private async Task<bool> ValidateSale(VendaModel venda)
{
    // Check max sale value
    var maxValue = await _systemSettings.GetMaxSaleValueAsync();
    if (venda.valor_total > maxValue)
    {
        await DisplayAlert("Erro",
            $"Valor total excede o limite de R$ {maxValue:N2}",
            "OK");
        return false;
    }

    // Check if client is required
    var requireClient = await _systemSettings.GetSettingAsync<bool>("require_client_on_sale", true);
    if (requireClient && venda.CodCliente == 0)
    {
        await DisplayAlert("Erro", "Cliente é obrigatório", "OK");
        return false;
    }

    // Check if negative stock is allowed
    var allowNegative = await _systemSettings.GetSettingAsync<bool>("allow_negative_stock", false);
    if (!allowNegative)
    {
        // Validate stock for each item...
    }

    return true;
}
```

---

## Troubleshooting

### Problem: Settings not loading

**Solution**:
```sql
-- Verify database connection
SELECT COUNT(*) FROM system_settings;

-- Check for missing settings
SELECT setting_key FROM system_settings WHERE setting_key IN
('payment_methods', 'session_timeout_minutes', 'max_sale_value');
```

### Problem: Cache not updating

**Solution**:
```csharp
// Force cache refresh by setting expiry to past
await _systemSettings.SetSettingAsync("cache_duration_minutes", 5, currentUserId);
// This invalidates the cache automatically
```

### Problem: Session timeout not working

**Solution**:
```csharp
// Verify timeout is not disabled
var timeout = await _systemSettings.GetSessionTimeoutAsync();
if (timeout == 0)
{
    await DisplayAlert("Info", "Session timeout is disabled", "OK");
}

// Ensure monitoring started
await _sessionTimeout.StartMonitoringAsync();

// Check that activity is being recorded
UserContext.Instance.RecordActivity(); // Call on user actions
```

---

## Best Practices

1. **Always provide default values** when reading settings
```csharp
// Good
var timeout = await _settings.GetSettingAsync<int>("session_timeout_minutes", 30);

// Bad - may return 0 if setting is missing
var timeout = await _settings.GetSettingAsync<int>("session_timeout_minutes");
```

2. **Use convenience methods** for frequently accessed settings
```csharp
// Good - uses cached convenience method
var methods = await _settings.GetPaymentMethodsAsync();

// Less efficient - bypasses convenience method
var methods = await _settings.GetSettingAsync<List<string>>("payment_methods", new List<string>());
```

3. **Record user ID for audit trail** when updating settings
```csharp
// Good - audit trail preserved
int userId = UserContext.Instance.GetCurrentUserId() ?? 0;
await _settings.SetSettingAsync("key", value, userId);

// Bad - no audit trail
await _settings.SetSettingAsync("key", value, 0);
```

4. **Validate settings before saving**
```csharp
// Good
if (newTimeout >= 5 && newTimeout <= 480) // 5 minutes to 8 hours
{
    await _settings.SetSettingAsync("session_timeout_minutes", newTimeout, userId);
}
else
{
    await DisplayAlert("Erro", "Timeout deve estar entre 5 e 480 minutos", "OK");
}
```

5. **Handle exceptions gracefully**
```csharp
try
{
    var setting = await _settings.GetSettingAsync<int>("some_setting", 10);
}
catch (Exception ex)
{
    // Log error
    Console.WriteLine($"Error loading setting: {ex.Message}");
    // Use default
    var setting = 10;
}
```

---

## Next Steps

1. Apply the database schema: `mysql -u user -p database < SETTINGS_DATABASE_SCHEMA.sql`
2. Load mock data: `mysql -u user -p database < MOCK_DATA.sql`
3. Test settings in your application using the examples above
4. Create admin pages for managing system settings
5. Implement user preferences pages for user-specific settings
6. Set up session timeout monitoring
7. Review audit logs periodically: `SELECT * FROM settings_audit_log ORDER BY changed_at DESC LIMIT 50;`

---

## Support

For questions or issues:
- Check the `NEXT_STEPS_SETTINGS.md` file for implementation details
- Review the model files: `SystemSettingModel.cs` and `UserSettingModel.cs`
- Review the service files: `SystemSettingsService.cs` and `UserSettingsService.cs`
- Check the database schema: `SETTINGS_DATABASE_SCHEMA.sql`

---

**Last Updated**: 2025-11-13
**Version**: 1.0
