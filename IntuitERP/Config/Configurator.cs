using IntuitERP.DataBase;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;  // Added for Path.Combine

namespace IntuitERP.Config
{
    public class Configurator
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private string connectionString;
        MySqlConnect mySqlConnect;

        public Configurator()
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\AccessDB.accdb");
            GetAccessDatabaseInfo(fullPath).Wait();  // Call the configuration loader

        }

        private async Task GetAccessDatabaseInfo(string dbPath)
        {
            connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";
            await GetData("Connection");
        }

        private async Task GetData(string tableName)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                string query = $"SELECT * FROM [{tableName}]";
                OleDbCommand command = new OleDbCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    using (OleDbDataReader reader = (OleDbDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            server = reader["Server"].ToString();
                            database = reader["Database"].ToString();
                            user = reader["User"].ToString();
                            password = reader["Password"].ToString();

                            // If you only need the first row, you can break here
                            break;
                        }
                    }

                    try
                    {
                        string connectionString = MySqlConnect.GetConnectionString(server, database, user, password);
                        MySqlConnect mySqlConnect = new MySqlConnect(connectionString);

                        mySqlConnect.OpenConnection();
                        Console.WriteLine("Connection opened successfully.");
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }


                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

    }
}
