using BDConfigurator.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDConfigurator.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        // The database path provided in your request
        private static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigsDB.db");

        public DatabaseService()
        {
            // The connection is initialized in an async method
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            // Ensure the directory exists
            var dbDir = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            // Create the connection and the table
            _database = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _database.CreateTableAsync<Configuration>();
            _isInitialized = true;
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            await InitializeAsync();
            return await _database.Table<Configuration>().ToListAsync();
        }

        public async Task<Configuration> GetConfigurationAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<Configuration>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveConfigurationAsync(Configuration config)
        {
            await InitializeAsync();
            if (config.ID != 0)
            {
                // Update an existing configuration
                return await _database.UpdateAsync(config);
            }
            else
            {
                // Save a new configuration
                return await _database.InsertAsync(config);
            }
        }

        public async Task<int> DeleteConfigurationAsync(Configuration config)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(config);
        }
    }
}
