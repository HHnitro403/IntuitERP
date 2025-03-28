using System;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient; // Use Microsoft.Data.Sqlite for SQLite connections

namespace IntuitERP.Config
{
    public class Configurator
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private readonly string dbPath;
        private MySqlConnect mySqlConnect;

        public Configurator()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigsDB.db");
            InitializeDatabase(); // Ensure the database and table exist
            CreateMySqlConnection(); // Load configuration
        }

        // Add this method to your Configurator class
        public IDbConnection GetMySqlConnection()
        {
            try
            {
                // Use the class-level variables already loaded by GetSQLiteDatabaseInfo()
                string connectionString = MySqlConnect.GetConnectionString(server, database, user, password);

                // Create and return the connection
                return new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating MySQL connection: {ex.Message}");
                return null;
            }
        }



        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                try
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                    using (var connection = CreateConnection())
                    {
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
        }

        private IDbConnection CreateMySqlConnection()
        {
            try
            {
                // First, get the connection info from SQLite
                using (var sqliteConnection = CreateConnection())
                {
                    var query = "SELECT * FROM Connection WHERE ID = @ID";
                    var config = sqliteConnection.QuerySingleOrDefault<ConnectionConfig>(query, new { ID = 1 });

                    if (config != null)
                    {
                        string connectionString = MySqlConnect.GetConnectionString(
                            config.Server,
                            config.DataBase,
                            config.User,
                            config.Password
                        );

                        // Assuming you have MySql.Data.MySqlClient imported
                        return new MySqlConnection(connectionString);
                    }
                    else
                    {
                        Console.WriteLine("No connection configuration found in the database.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating MySQL connection: {ex.Message}");
                return null;
            }
        }

        private IDbConnection CreateConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }
    }

    // Model class for the Connection table
    public class ConnectionConfig
    {
        public int ID { get; set; }
        public string Server { get; set; }
        public string DataBase { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    // Example MySQL connection class for demonstration purposes
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
