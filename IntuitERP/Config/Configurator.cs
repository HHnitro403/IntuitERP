using System;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite; // Use Microsoft.Data.Sqlite for SQLite connections

namespace IntuitERP.Config
{
    public class Configurator
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private readonly string dbPath;

        public Configurator()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigsDB.db");
            InitializeDatabase(); // Ensure the database and table exist
            GetSQLiteDatabaseInfo(); // Load configuration
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

        private void GetSQLiteDatabaseInfo()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var query = "SELECT * FROM Connection WHERE ID = @ID";
                    var config = connection.QuerySingleOrDefault<ConnectionConfig>(query, new { ID = 1 });

                    if (config != null)
                    {
                        server = config.Server;
                        database = config.Database;
                        user = config.User;
                        password = config.Password;

                        Console.WriteLine($"Loaded configuration: Server={server}, Database={database}, User={user}");

                        try
                        {
                            string mysqlConnectionString = MySqlConnect.GetConnectionString(server, database, user, password);
                            MySqlConnect mySqlConnect = new MySqlConnect(mysqlConnectionString);

                            mySqlConnect.OpenConnection();
                            Console.WriteLine("MySQL connection opened successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred when connecting to MySQL: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No connection configuration found in the database.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQLite Error: {ex.Message}");
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
        public string Database { get; set; }
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
