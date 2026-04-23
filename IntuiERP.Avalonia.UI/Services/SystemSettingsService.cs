using IntuiERP.Avalonia.UI.models;
using Dapper;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace IntuiERP.Avalonia.UI.Services
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
