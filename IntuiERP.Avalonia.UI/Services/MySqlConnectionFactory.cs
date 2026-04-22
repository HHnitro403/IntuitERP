using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using IntuitERP.Config;
using System.Data;

namespace IntuitERP.Services
{
    /// <summary>
    /// Factory for creating MySQL database connections
    /// Manages connection creation from configuration
    /// </summary>
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly Configurator _configurator;

        public MySqlConnectionFactory()
        {
            _configurator = new Configurator();
        }

        /// <summary>
        /// Creates a new MySQL connection from configuration
        /// Connection is opened and ready to use
        /// IMPORTANT: Caller must dispose the connection using 'using' statement
        /// </summary>
        /// <returns>An open MySQL connection</returns>
        public IDbConnection CreateConnection()
        {
            var connection = _configurator.GetMySqlConnection();

            // Ensure connection is open
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }
    }
}

