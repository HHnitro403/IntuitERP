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
