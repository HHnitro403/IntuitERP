using DBConfigurator.Avalonia.UI.Models;
using Npgsql;
using System;
using System.Threading.Tasks;
using Dapper;

namespace DBConfigurator.Avalonia.UI.Services
{
    public class ConnectionTesterService
    {
        private readonly DatabaseService _databaseService;

        public ConnectionTesterService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(Configuration config)
        {
            try
            {
                string connectionString = _databaseService.BuildConnectionString(config);
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteScalarAsync<int>("SELECT 1");
                    return (true, "Connection successful!");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }
    }
}
