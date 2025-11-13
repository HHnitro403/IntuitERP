# Next Steps: Complete Settings System Implementation

This document outlines the remaining work to complete the settings system implementation.

## Files Already Created ✅
1. ✅ `SETTINGS_DATABASE_SCHEMA.sql` - Database tables with 28 default settings
2. ✅ `IntuitERP/models/SystemSettingModel.cs` - System settings model
3. ✅ `IntuitERP/models/UserSettingModel.cs` - User settings model

## Files to Create

### 1. SystemSettingsService.cs
Location: `IntuitERP/Services/SystemSettingsService.cs`

```csharp
using IntuitERP.models;
using Dapper;
using System.Data;

namespace IntuitERP.Services
{
    public class SystemSettingsService
    {
        private readonly IDbConnection _connection;
        private static Dictionary<string, SystemSettingModel> _cache;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private const int CACHE_MINUTES = 5;

        public SystemSettingsService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default(T))
        {
            var setting = await GetSettingModelAsync(key);
            if (setting == null) return defaultValue;

            try
            {
                return setting.GetValue<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<SystemSettingModel> GetSettingModelAsync(string key)
        {
            // Check cache first
            if (_cache != null && DateTime.Now < _cacheExpiry && _cache.ContainsKey(key))
            {
                return _cache[key];
            }

            // Load from database
            var sql = "SELECT * FROM system_settings WHERE setting_key = @Key";
            var setting = await _connection.QueryFirstOrDefaultAsync<SystemSettingModel>(sql, new { Key = key });

            if (setting != null)
            {
                // Update cache
                if (_cache == null || DateTime.Now >= _cacheExpiry)
                {
                    await RefreshCacheAsync();
                }
                else
                {
                    _cache[key] = setting;
                }
            }

            return setting;
        }

        public async Task SetSettingAsync<T>(string key, T value, int updatedBy)
        {
            var setting = await GetSettingModelAsync(key);
            if (setting == null)
            {
                throw new Exception($"Setting '{key}' not found");
            }

            setting.SetValue(value);
            setting.UpdatedBy = updatedBy;

            var sql = @"UPDATE system_settings
                       SET setting_value = @SettingValue,
                           updated_at = @UpdatedAt,
                           updated_by = @UpdatedBy
                       WHERE setting_key = @SettingKey";

            await _connection.ExecuteAsync(sql, setting);

            // Invalidate cache
            _cacheExpiry = DateTime.MinValue;
        }

        public async Task<List<SystemSettingModel>> GetAllSettingsAsync()
        {
            var sql = "SELECT * FROM system_settings ORDER BY category, setting_key";
            var settings = await _connection.QueryAsync<SystemSettingModel>(sql);
            return settings.ToList();
        }

        public async Task<List<SystemSettingModel>> GetSettingsByCategoryAsync(string category)
        {
            var sql = "SELECT * FROM system_settings WHERE category = @Category ORDER BY setting_key";
            var settings = await _connection.QueryAsync<SystemSettingModel>(sql, new { Category = category });
            return settings.ToList();
        }

        public async Task ResetToDefaultAsync(string key, int updatedBy)
        {
            var sql = @"UPDATE system_settings
                       SET setting_value = default_value,
                           updated_at = @UpdatedAt,
                           updated_by = @UpdatedBy
                       WHERE setting_key = @Key";

            await _connection.ExecuteAsync(sql, new
            {
                Key = key,
                UpdatedAt = DateTime.Now,
                UpdatedBy = updatedBy
            });

            _cacheExpiry = DateTime.MinValue;
        }

        private async Task RefreshCacheAsync()
        {
            var settings = await GetAllSettingsAsync();
            _cache = settings.ToDictionary(s => s.SettingKey);
            _cacheExpiry = DateTime.Now.AddMinutes(CACHE_MINUTES);
        }

        // Convenience methods for high-priority settings
        public async Task<List<string>> GetPaymentMethodsAsync()
        {
            return await GetSettingAsync<List<string>>("payment_methods",
                new List<string> { "Dinheiro", "Cartão de Crédito", "PIX" });
        }

        public async Task<int> GetSessionTimeoutAsync()
        {
            return await GetSettingAsync<int>("session_timeout_minutes", 30);
        }

        public async Task<decimal> GetMaxSaleValueAsync()
        {
            return await GetSettingAsync<decimal>("max_sale_value", 9999999.99m);
        }

        public async Task<string> GetDateFormatAsync()
        {
            return await GetSettingAsync<string>("date_format", "dd/MM/yyyy");
        }

        public async Task<int> GetRecentOrdersLimitAsync()
        {
            return await GetSettingAsync<int>("recent_orders_limit", 5);
        }
    }
}
```

### 2. UserSettingsService.cs
Location: `IntuitERP/Services/UserSettingsService.cs`

```csharp
using IntuitERP.models;
using Dapper;
using System.Data;

namespace IntuitERP.Services
{
    public class UserSettingsService
    {
        private readonly IDbConnection _connection;

        public UserSettingsService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<T> GetUserSettingAsync<T>(int userId, string key, T defaultValue = default(T))
        {
            var setting = await GetUserSettingModelAsync(userId, key);
            if (setting == null) return defaultValue;

            try
            {
                return setting.GetValue<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<UserSettingModel> GetUserSettingModelAsync(int userId, string key)
        {
            var sql = "SELECT * FROM user_settings WHERE cod_usuario = @UserId AND setting_key = @Key";
            return await _connection.QueryFirstOrDefaultAsync<UserSettingModel>(sql,
                new { UserId = userId, Key = key });
        }

        public async Task SetUserSettingAsync<T>(int userId, string key, T value, string settingType, string category)
        {
            var existing = await GetUserSettingModelAsync(userId, key);

            if (existing != null)
            {
                // Update existing
                existing.SetValue(value);

                var sql = @"UPDATE user_settings
                           SET setting_value = @SettingValue,
                               updated_at = @UpdatedAt
                           WHERE id = @Id";

                await _connection.ExecuteAsync(sql, existing);
            }
            else
            {
                // Insert new
                var newSetting = new UserSettingModel
                {
                    CodUsuario = userId,
                    SettingKey = key,
                    SettingType = settingType,
                    Category = category
                };
                newSetting.SetValue(value);

                var sql = @"INSERT INTO user_settings
                           (cod_usuario, setting_key, setting_value, setting_type, category)
                           VALUES (@CodUsuario, @SettingKey, @SettingValue, @SettingType, @Category)";

                await _connection.ExecuteAsync(sql, newSetting);
            }
        }

        public async Task<List<UserSettingModel>> GetAllUserSettingsAsync(int userId)
        {
            var sql = "SELECT * FROM user_settings WHERE cod_usuario = @UserId ORDER BY category, setting_key";
            var settings = await _connection.QueryAsync<UserSettingModel>(sql, new { UserId = userId });
            return settings.ToList();
        }

        public async Task DeleteUserSettingAsync(int userId, string key)
        {
            var sql = "DELETE FROM user_settings WHERE cod_usuario = @UserId AND setting_key = @Key";
            await _connection.ExecuteAsync(sql, new { UserId = userId, Key = key });
        }

        // Convenience methods for common user settings
        public async Task<string> GetUserThemeAsync(int userId)
        {
            return await GetUserSettingAsync<string>(userId, "theme", "Auto");
        }

        public async Task<int> GetUserItemsPerPageAsync(int userId)
        {
            return await GetUserSettingAsync<int>(userId, "items_per_page", 20);
        }
    }
}
```

### 3. Session Timeout Implementation

#### SessionTimeoutService.cs
Location: `IntuitERP/Services/SessionTimeoutService.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class SessionTimeoutService
    {
        private DateTime _lastActivity;
        private Timer _timer;
        private readonly SystemSettingsService _settingsService;
        private readonly UserContext _userContext;
        private int _timeoutMinutes = 30;
        public event EventHandler SessionExpired;

        public SessionTimeoutService(SystemSettingsService settingsService, UserContext userContext)
        {
            _settingsService = settingsService;
            _userContext = userContext;
            _lastActivity = DateTime.Now;
        }

        public async Task StartMonitoringAsync()
        {
            _timeoutMinutes = await _settingsService.GetSessionTimeoutAsync();

            if (_timeoutMinutes <= 0) return; // Timeout disabled

            _timer = new Timer(CheckTimeout, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void RecordActivity()
        {
            _lastActivity = DateTime.Now;
        }

        private void CheckTimeout(object state)
        {
            var idle = (DateTime.Now - _lastActivity).TotalMinutes;

            if (idle >= _timeoutMinutes)
            {
                _timer?.Dispose();
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StopMonitoring()
        {
            _timer?.Dispose();
        }
    }
}
```

#### Update UserContext.cs to track activity
Add to existing `UserContext.cs`:

```csharp
private DateTime _lastActivity = DateTime.Now;

public void RecordActivity()
{
    _lastActivity = DateTime.Now;
}

public TimeSpan GetIdleTime()
{
    return DateTime.Now - _lastActivity;
}
```

### 4. Register Services in MauiProgram.cs

Add to existing `MauiProgram.cs`:

```csharp
// Settings Services (Singleton for caching)
builder.Services.AddSingleton<SystemSettingsService>();
builder.Services.AddSingleton<UserSettingsService>();
builder.Services.AddSingleton<SessionTimeoutService>();
```

### 5. Create Mock Data SQL

Location: `MOCK_DATA.sql`

```sql
-- ========================================
-- Mock Data for IntuitERP Management System
-- ========================================

-- Products (20 sample products)
INSERT INTO produtos (nome, Descricao, preco_unit, SaldoEst, EstMinimo, categoria, tipo, FornecedorP_ID, Ativo) VALUES
('Notebook Dell Inspiron 15', 'Notebook Dell Inspiron 15 i5 8GB 256GB SSD', 3299.00, 25, 5, 'Eletrônicos', 'Revenda', 1, 1),
('Mouse Logitech MX Master 3', 'Mouse sem fio ergonômico', 499.00, 120, 20, 'Periféricos', 'Revenda', 1, 1),
('Teclado Mecânico Keychron K2', 'Teclado mecânico wireless', 699.00, 45, 10, 'Periféricos', 'Revenda', 1, 1),
('Monitor LG 27" 4K', 'Monitor LG 27" UHD 4K IPS', 1899.00, 15, 3, 'Eletrônicos', 'Revenda', 2, 1),
('Cadeira Gamer DXRacer', 'Cadeira ergonômica para escritório', 1499.00, 8, 2, 'Móveis', 'Revenda', 2, 1);
-- Add 15 more products here...

-- Clients (10 sample clients)
INSERT INTO clientes (nome, email, telefone, cpf, data_nasc, endereco, numero, bairro, CodCidade, CEP) VALUES
('TechCorp Ltda', 'compras@techcorp.com', '(11) 98765-4321', '123.456.789-00', '1990-05-15', 'Av. Paulista', '1000', 'Bela Vista', 1, '01310-100'),
('InnovaNet Soluções', 'contato@innovanet.com', '(21) 97654-3210', '234.567.890-11', '1985-08-22', 'Rua da Assembleia', '10', 'Centro', 2, '20011-000');
-- Add 8 more clients here...

-- Sellers (5 sample sellers)
INSERT INTO vendedores (NomeVendedor, QtdVendas, QtdVendasFinalizadas) VALUES
('João Silva', 45, 40),
('Maria Santos', 62, 58),
('Pedro Oliveira', 38, 35),
('Ana Costa', 51, 48),
('Carlos Souza', 29, 27);

-- Sample sales
-- Add sample sales with various statuses here...

-- Sample purchases
-- Add sample purchases here...
```

### 6. Usage Documentation

Create comprehensive `SETTINGS_USAGE_GUIDE.md` showing:
- How to access settings
- How to modify payment methods
- How to change session timeout
- How to format dates/currency
- How to adjust pagination limits

## Implementation Priority

1. ✅ Database schema (DONE)
2. ✅ Models (DONE)
3. **SystemSettingsService** (code provided above)
4. **UserSettingsService** (code provided above)
5. **Register services in DI**
6. **Session timeout** (code provided above)
7. **Mock data SQL** (template provided above)
8. **Documentation**

## Testing Checklist

- [ ] Apply database schema
- [ ] Verify 28 default settings inserted
- [ ] Test retrieving payment methods
- [ ] Test changing session timeout
- [ ] Test date format changes reflected in UI
- [ ] Test pagination limit changes
- [ ] Test user-specific theme settings
- [ ] Test session timeout auto-logout

## Estimated Remaining Time

- Services implementation: 1 hour
- Session timeout integration: 30 minutes
- Mock data creation: 30 minutes
- Testing: 1 hour
- Documentation: 30 minutes

**Total: ~3.5 hours**
