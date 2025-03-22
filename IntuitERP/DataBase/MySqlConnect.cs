using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;




namespace IntuitERP.DataBase
{
    public class MySqlConnect
    {
        private readonly MySqlConnection _connection;

        // Constructor to initialize the MySQL connection
        public MySqlConnect(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            _connection = new MySqlConnection(connectionString);
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

        // Method to retrieve the underlying MySqlConnection object (if needed)
        public MySqlConnection GetConnection()
        {
            return _connection;
        }
    }
}
