using IntuiERP.Avalonia.UI.Config;
using System.Data;

namespace IntuiERP.Avalonia.UI.Services
{
    /// <summary>
    /// Factory for creating PostgreSQL database connections
    /// Manages connection creation from configuration
    /// </summary>
    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly Configurator _configurator;

        public NpgsqlConnectionFactory()
        {
            _configurator = new Configurator();
        }

        /// <summary>
        /// Creates a new PostgreSQL connection from configuration
        /// Connection is opened and ready to use
        /// IMPORTANT: Caller must dispose the connection using 'using' statement
        /// </summary>
        /// <returns>An open PostgreSQL connection</returns>
        public IDbConnection CreateConnection()
        {
            var connection = _configurator.GetNpgsqlConnection();

            // Ensure connection is open
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }
    }
}
