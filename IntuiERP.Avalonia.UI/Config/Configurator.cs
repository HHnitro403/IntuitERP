using System;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace IntuiERP.Avalonia.UI.Config
{
    public class Configurator
    {
        private readonly string dbPath;
        private ConnectionConfig? currentConfig;

        public Configurator()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appDataPath, "IntuitERP", "Config");
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            dbPath = Path.Combine(folderPath, "ConfigsDB.db");
            
            try
            {
                if (File.Exists(dbPath))
                {
                    LoadConfiguration();
                }
            }
            catch (Exception)
            {
                // If loading fails, currentConfig remains null
            }
        }

        public IDbConnection GetNpgsqlConnection()
        {
            if (currentConfig == null)
            {
                LoadConfiguration();
            }

            if (currentConfig == null)
            {
                throw new InvalidOperationException("PostgreSQL connection configuration not found in AppData.");
            }

            string connectionString = NpgsqlConnect.GetConnectionString(
                currentConfig.Server,
                currentConfig.DataBase,
                currentConfig.User,
                currentConfig.Password);

            // NpgsqlConnection handles the actual connection
            return new NpgsqlConnection(connectionString);
        }

        private void LoadConfiguration()
        {
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Configuration database not found in AppData.", dbPath);
            }

            using (var connection = CreateSqliteConnection())
            {
                connection.Open();
                var query = "SELECT * FROM Connection ORDER BY ID DESC LIMIT 1";
                currentConfig = connection.QueryFirstOrDefault<ConnectionConfig>(query);

                if (currentConfig == null)
                {
                    throw new InvalidDataException("No connection configuration found in the database.");
                }
            }
        }

        private IDbConnection CreateSqliteConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }
    }

    public class ConnectionConfig
    {
        public int ID { get; set; }
        public string Server { get; set; } = string.Empty;
        public string DataBase { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class NpgsqlConnect
    {
        public static string GetConnectionString(string server, string database, string user, string password)
        {
            // Standard PostgreSQL connection string format for Npgsql
            // Including SSL Mode=Prefer for Supabase compatibility
            // Removed trailing semicolon from Host if user accidentally included it
            string cleanHost = server.Replace("postgresql://", "").Split(':')[0].Trim();
            
            return $"Host={cleanHost};Database={database};Username={user};Password={password};SSL Mode=Prefer;Trust Server Certificate=true;Pooling=false;Command Timeout=30;";
        }
    }
}
