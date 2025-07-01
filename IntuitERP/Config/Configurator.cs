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
            InitializeDatabase(); // Ensure the database and table exist
            LoadConfiguration(); // Load configuration from SQLite
        }

        public IDbConnection GetMySqlConnection()
        {
            try
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

                return new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating MySQL connection: {ex.Message}");
                throw; // Rethrow to let caller handle the error
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                bool databaseExists = File.Exists(dbPath);

                using (var connection = CreateSqliteConnection())
                {
                    connection.Open();

                    // Create the Connection table if it doesn't exist
                    var createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Connection (
                        ID INTEGER PRIMARY KEY,
                        Server TEXT,
                        Database TEXT,
                        User TEXT,
                        Password TEXT
                    )";
                    connection.Execute(createTableQuery);

                    // Insert default configuration if no data exists
                    var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM Connection");
                    if (count == 0)
                    {
                        var insertQuery = @"
                        INSERT INTO Connection (ID, Server, Database, User, Password)
                        VALUES (@ID, @Server, @Database, @User, @Password)";
                        connection.Execute(insertQuery, new
                        {
                            ID = 1,
                            Server = "localhost",
                            Database = "default_db",
                            User = "root",
                            Password = "password"
                        });
                    }
                }

                Console.WriteLine("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                using (var connection = CreateSqliteConnection())
                {
                    connection.Open();
                    var query = "SELECT * FROM Connection WHERE ID = @ID";
                    currentConfig = connection.QuerySingleOrDefault<ConnectionConfig>(query, new { ID = 1 });

                    if (currentConfig == null)
                    {
                        Console.WriteLine("No connection configuration found in the database.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                throw;
            }
        }

        private IDbConnection CreateSqliteConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }

        // Added method to update configuration
        public void SaveConfiguration(ConnectionConfig config)
        {
            try
            {
                using (var connection = CreateSqliteConnection())
                {
                    connection.Open();
                    var updateQuery = @"
                    UPDATE Connection 
                    SET Server = @Server, Database = @Database, User = @User, Password = @Password
                    WHERE ID = @ID";

                    connection.Execute(updateQuery, new
                    {
                        ID = 1,
                        config.Server,
                        config.Database,
                        config.User,
                        config.Password
                    });

                    currentConfig = config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                throw;
            }
        }
    }

    // Model class for the Connection table
    public class ConnectionConfig
    {
        public int ID { get; set; }
        public string Server { get; set; }
        public string Database { get; set; } // Changed from DataBase to match DB column
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

        public void OpenConnection()
        {
            // Simulate opening a connection (replace with actual MySQL logic)
            Console.WriteLine($"Connecting to MySQL with: {connectionString}");
        }
    }
}
