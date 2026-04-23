using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBConfigurator.Avalonia.UI.Services
{
    public static class PostgresConfigReader
    {
        public static string GetConnectionString()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbPath = Path.Combine(appDataPath, "IntuitERP", "Config", "ConfigsDB.db");

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Configuration database not found.", dbPath);
            }

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                
                // Read the active configuration
                var config = connection.QueryFirstOrDefault<dynamic>("SELECT * FROM Connection ORDER BY ID DESC LIMIT 1");

                if (config == null)
                {
                    throw new InvalidDataException("No connection configuration found in the database.");
                }

                // Parse the dynamic results safely
                string server = config.Server ?? "";
                string database = config.DataBase ?? "";
                string user = config.User ?? "";
                string password = config.Password ?? "";
                
                // Read Port safely since it might be a new or missing column in some old tables
                int port = 5432;
                if (((IDictionary<string, object>)config).ContainsKey("Port") && config.Port != null)
                {
                    try { port = Convert.ToInt32(config.Port); } catch { }
                }

                string cleanHost = server.Replace("postgresql://", "").Split(':')[0].Trim();
                
                // Build and return the Npgsql connection string
                return $"Host={cleanHost};Port={port};Database={database};Username={user};Password={password};SSL Mode=Prefer;Trust Server Certificate=true;";
            }
        }
    }
}
