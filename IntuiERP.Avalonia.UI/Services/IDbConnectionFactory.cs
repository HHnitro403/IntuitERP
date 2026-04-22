using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Data;

namespace IntuitERP.Services
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

