using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace IntuiERP.Avalonia.UI.DataBase
{
    public class NpgsqlConnect
    {
        private readonly NpgsqlConnection _connection;

        // Constructor to initialize the PostgreSQL connection
        public NpgsqlConnect(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            _connection = new NpgsqlConnection(connectionString);
        }

        // Static method to build a connection string
        public static string GetConnectionString(string server, string database, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Server, database, user, and password must all be provided.");
            }

            return $"Server={server};Database={database};User={user};Password={password};";
        }

        // Method to open the connection
        public void OpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        // Method to close the connection
        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        // Method to retrieve the underlying NpgsqlConnection object (if needed)
        public NpgsqlConnection GetConnection()
        {
            return _connection;
        }
    }
}
