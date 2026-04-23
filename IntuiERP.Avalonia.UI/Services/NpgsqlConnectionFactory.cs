using IntuiERP.Avalonia.UI.Config;
using System.Data;
using Npgsql;

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
        /// Creates a new PostgreSQL connection from configuration.
        /// The connection is NOT opened here to allow for asynchronous opening in the services.
        /// </summary>
        /// <returns>A new IDbConnection instance (NpgsqlConnection)</returns>
        public IDbConnection CreateConnection()
        {
            // Just return the connection object, don't open it yet.
            // This avoids mixing synchronous Open() with asynchronous Dapper calls.
            return _configurator.GetNpgsqlConnection();
        }
    }
}
