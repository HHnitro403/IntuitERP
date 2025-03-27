using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using IntuitERP.DataBase;
using Microsoft.Data.Sqlite;

namespace IntuitERP.Config
{
    public class Configurator
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private string dbPath;
        private string connectionString;
        MySqlConnect mySqlConnect;

        public Configurator()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigDB.sqlite");
            connectionString = $"Data Source={dbPath};Version=3;";
            GetSQLiteDatabaseInfo();
        }

        private void GetSQLiteDatabaseInfo()
        {
            GetData("Connection");
        }

        private void GetData(string tableName)
        {
            if (!File.Exists(dbPath))
            {
                InitializeDatabase();
            }

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName} WHERE ID = 1";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                server = reader["Server"].ToString();
                                database = reader["Database"].ToString();
                                user = reader["User"].ToString();
                                password = reader["Password"].ToString();

                                try
                                {
                                    string mysqlConnectionString = MySqlConnect.GetConnectionString(server, database, user, password);
                                    MySqlConnect mySqlConnect = new MySqlConnect(mysqlConnectionString);

                                    mySqlConnect.OpenConnection();
                                    Console.WriteLine("Connection opened successfully.");
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQLite Error: {ex.Message}");
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string createTableQuery = @"
                        CREATE TABLE Connection (
                            ID INTEGER PRIMARY KEY,
                            Server TEXT,
                            Database TEXT,
                            User TEXT,
                            Password TEXT
                        )"
                    ;

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string insertDefaultConfigQuery = @"
                        INSERT INTO Connection (ID, Server, Database, User, Password)
                        VALUES (1, 'localhost', 'default_db', 'root', 'password')"
                    ;

                    using (var command = new SqliteCommand(insertDefaultConfigQuery, connection))
                    {
                        command.ExecuteNonQuery();
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
}
