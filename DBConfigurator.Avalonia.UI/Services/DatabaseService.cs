using DBConfigurator.Avalonia.UI.Models;
using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace DBConfigurator.Avalonia.UI.Services
{
    public class DatabaseService
    {
        private static string DbPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string folderPath = Path.Combine(appDataPath, "IntuitERP", "Config");
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                return Path.Combine(folderPath, "ConfigsDB.db");
            }
        }

        private string ConnectionString => $"Data Source={DbPath}";

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Connection (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Server TEXT,
                        DataBase TEXT,
                        User TEXT,
                        Password TEXT
                    )");
            }
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                var result = await connection.QueryAsync<Configuration>("SELECT * FROM Connection");
                return result.ToList();
            }
        }

        public async Task<Configuration?> GetConfigurationAsync(int id)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<Configuration>(
                    "SELECT * FROM Connection WHERE ID = @Id", new { Id = id });
            }
        }

        public async Task<int> SaveConfigurationAsync(Configuration config)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                if (config.ID != 0)
                {
                    return await connection.ExecuteAsync(@"
                        UPDATE Connection 
                        SET Server = @Server, DataBase = @DataBase, User = @User, Password = @Password 
                        WHERE ID = @ID", config);
                }
                else
                {
                    return await connection.ExecuteAsync(@"
                        INSERT INTO Connection (Server, DataBase, User, Password) 
                        VALUES (@Server, @DataBase, @User, @Password)", config);
                }
            }
        }

        public async Task<int> DeleteConfigurationAsync(Configuration config)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                return await connection.ExecuteAsync("DELETE FROM Connection WHERE ID = @ID", new { ID = config.ID });
            }
        }
    }
}
