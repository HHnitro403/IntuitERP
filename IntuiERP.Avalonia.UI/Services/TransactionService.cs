using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Data;

namespace IntuitERP.Services
{
    /// <summary>
    /// Service for managing database transactions
    /// Provides safe transaction execution with automatic rollback on errors
    /// </summary>
    public class TransactionService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TransactionService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Executes an action within a database transaction
        /// Automatically commits on success or rolls back on exception
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute within transaction</param>
        /// <returns>Result of the operation</returns>
        /// <exception cref="Exception">Re-throws any exception after rollback</exception>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                IDbTransaction? transaction = null;

                try
                {
                    // Begin transaction
                    transaction = connection.BeginTransaction();

                    // Execute the operation
                    var result = await operation(connection, transaction);

                    // Commit transaction
                    transaction.Commit();

                    return result;
                }
                catch (Exception)
                {
                    // Rollback on any error
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes an action within a database transaction (void return)
        /// Automatically commits on success or rolls back on exception
        /// </summary>
        /// <param name="operation">The operation to execute within transaction</param>
        /// <exception cref="Exception">Re-throws any exception after rollback</exception>
        public async Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operation)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                IDbTransaction? transaction = null;

                try
                {
                    // Begin transaction
                    transaction = connection.BeginTransaction();

                    // Execute the operation
                    await operation(connection, transaction);

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Rollback on any error
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes multiple operations within a single transaction
        /// All operations must succeed or all will be rolled back
        /// </summary>
        /// <param name="operations">List of operations to execute</param>
        /// <exception cref="Exception">Re-throws any exception after rollback</exception>
        public async Task ExecuteMultipleInTransactionAsync(params Func<IDbConnection, IDbTransaction, Task>[] operations)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                IDbTransaction? transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    foreach (var operation in operations)
                    {
                        await operation(connection, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
        }
    }
}

