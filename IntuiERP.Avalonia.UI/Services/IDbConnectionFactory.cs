using System.Data;

namespace IntuiERP.Avalonia.UI.Services
{
    /// <summary>
    /// Factory interface for creating database connections
    /// Enables proper connection lifecycle management and dependency injection
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a new database connection
        /// Caller is responsible for disposing the connection
        /// </summary>
        /// <returns>A new IDbConnection instance</returns>
        IDbConnection CreateConnection();
    }
}
