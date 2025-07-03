
using DBconfigurator.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBconfigurator.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        // The database path remains hardcoded to the C:\ drive.
        private static string DbPath
        {
            get
            {
                string rootPath = "C:\\IntuitERP\\Config\\ConfigsDB.db";
                return rootPath;
                
            }
        }

        public DatabaseService()
        {
            // The connection is initialized in an async method
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            // --- CHANGE: Check if the database file exists before proceeding. ---
            if (!File.Exists(DbPath))
            {
                // Log an error for debugging. The application will not create the file.
                Console.WriteLine($"DATABASE NOT FOUND: The database file was not found at {DbPath}");
                // Keep _isInitialized as false, so methods will return empty/null.
                return;
            }

            try
            {
                // --- CHANGE: Removed the 'Create' flag. This will now only open an existing file. ---
                var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache;
                _database = new SQLiteAsyncConnection(DbPath, flags);

                // This will create the table *if it doesn't exist* inside the DB file, but it will not create the file itself.
                await _database.CreateTableAsync<Configuration>();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to database: {ex.Message}");
                // Ensure we remain uninitialized if an error occurs.
                _isInitialized = false;
            }
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            await InitializeAsync();
            if (!_isInitialized) return new List<Configuration>();
            return await _database.Table<Configuration>().ToListAsync();
        }

        public async Task<Configuration> GetConfigurationAsync(int id)
        {
            await InitializeAsync();
            if (!_isInitialized) return null;
            return await _database.Table<Configuration>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveConfigurationAsync(Configuration config)
        {
            await InitializeAsync();
            if (!_isInitialized) return -1;
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
            if (!_isInitialized) return -1;
            return await _database.DeleteAsync(config);
        }
    }
}