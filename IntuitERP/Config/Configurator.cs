using System;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace IntuitERP.Config
{
    public class Configurator
    {
        private readonly string dbPath;
        private ConnectionConfig currentConfig;

        public Configurator()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigsDB.db");
            LoadConfiguration(); // Load configuration from SQLite
        }

        public IDbConnection GetMySqlConnection()
        {
            if (currentConfig == null)
            {
                LoadConfiguration();
            }

            if (currentConfig == null)
            {
                throw new InvalidOperationException("MySQL connection configuration not found.");
            }

            string connectionString = MySqlConnect.GetConnectionString(
                currentConfig.Server,
                currentConfig.Database,
                currentConfig.User,
                currentConfig.Password);

            var connection = new MySqlConnection(connectionString);

            // **Attempt to connect to the database to validate credentials.**
            // This will throw a MySqlException if the connection fails,
            // which must be handled by the method's caller.
            connection.Open();
            connection.Close();

            // Return the connection object (in a closed state) for the caller to use.
            return connection;
        }

        private void LoadConfiguration()
        {
            // Throw an exception if the database file does not exist.
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Configuration database not found.", dbPath);
            }

            using (var connection = CreateSqliteConnection())
            {
                connection.Open();
                var query = "SELECT * FROM Connection WHERE ID = @ID";
                currentConfig = connection.QuerySingleOrDefault<ConnectionConfig>(query, new { ID = 1 });

                if (currentConfig == null)
                {
                    throw new InvalidDataException("No connection configuration with ID = 1 found in the database.");
                }
            }
        }

        private IDbConnection CreateSqliteConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }

        public void SaveConfiguration(ConnectionConfig config)
        {
            using (var connection = CreateSqliteConnection())
            {
                connection.Open();
                var updateQuery = @"
                UPDATE Connection 
                SET Server = @Server, Database = @Database, User = @User, Password = @Password
                WHERE ID = @ID";

                // Pass the config object directly, ensuring the ID is set for the WHERE clause.
                config.ID = 1;
                connection.Execute(updateQuery, config);

                // Update the in-memory configuration
                currentConfig = config;
            }
        }
    }

    // Model class for the Connection table
    public class ConnectionConfig
    {
        public int ID { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    // Helper class for MySQL connections
    public class MySqlConnect
    {
        private readonly string connectionString;

        public MySqlConnect(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public static string GetConnectionString(string server, string database, string user, string password)
        {
            return $"Server={server};Database={database};User={user};Password={password};";
        }

    }
}