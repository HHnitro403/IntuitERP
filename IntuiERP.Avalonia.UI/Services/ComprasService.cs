using Dapper;
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
{
    public class CompraService
    {
        private readonly IDbConnection _connection;

        public CompraService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<CompraModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM compra ORDER BY cod_compra";
            return await _connection.QueryAsync<CompraModel>(query);
        }

        public async Task<CompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM compra WHERE cod_compra = @Id";
            return await _connection.QueryFirstOrDefaultAsync<CompraModel>(query, new { Id = id });
        }

        public async Task<IEnumerable<CompraModel>> GetAllComprasAsync(CompraFilterModel filters)
        {
            var sqlBuilder = new StringBuilder("SELECT * FROM compra");
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            // Filter by Supplier (cod_fornecedor)
            if (filters.CodFornecedor.HasValue)
            {
                whereClauses.Add("cod_fornecedor = @CodFornecedor");
                parameters.Add("@CodFornecedor", filters.CodFornecedor.Value);
            }

            // Filter by Start Date
            if (filters.DataInicial.HasValue)
            {
                whereClauses.Add("data_compra >= @DataInicial");
                parameters.Add("@DataInicial", filters.DataInicial.Value);
            }

            // Filter by End Date
            if (filters.DataFinal.HasValue)
            {
                whereClauses.Add("data_compra <= @DataFinal");
                parameters.Add("@DataFinal", filters.DataFinal.Value);
            }

            // Filter by Status
            if (filters.StatusCompra.HasValue)
            {
                whereClauses.Add("status_compra = @StatusCompra");
                parameters.Add("@StatusCompra", filters.StatusCompra.Value);
            }

            if (whereClauses.Any())
            {
                sqlBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            sqlBuilder.Append(" ORDER BY cod_compra DESC");

            return await _connection.QueryAsync<CompraModel>(sqlBuilder.ToString(), parameters);
        }

        public async Task<int> InsertAsync(CompraModel compra)
        {
            const string query = 
                @"INSERT INTO compra 
                (data_compra, cod_fornecedor, valor_total, status_compra) 
                VALUES 
                (@data_compra, @CodFornecedor, @valor_total, @status_compra) RETURNING cod_compra;";
            
            if (compra.data_compra == null)
                compra.data_compra = DateTime.Now;
                
            return await _connection.ExecuteScalarAsync<int>(query, compra);
        }

        public async Task<int> UpdateAsync(CompraModel compra)
        {
            const string query = 
                @"UPDATE compra SET 
                data_compra = @data_compra, 
                cod_fornecedor = @CodFornecedor, 
                valor_total = @valor_total, 
                status_compra = @status_compra 
                WHERE cod_compra = @CodCompra";
            return await _connection.ExecuteAsync(query, compra);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM compra WHERE cod_compra = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
        
        public async Task<IEnumerable<CompraModel>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            const string query = 
                @"SELECT * FROM compra 
                WHERE data_compra BETWEEN @StartDate AND @EndDate";
            return await _connection.QueryAsync<CompraModel>(query, 
                new { StartDate = startDate, EndDate = endDate });
        }
        
        public async Task<IEnumerable<CompraModel>> GetByFornecedorAsync(int fornecedorId)
        {
            const string query = "SELECT * FROM compra WHERE cod_fornecedor = @FornecedorId";
            return await _connection.QueryAsync<CompraModel>(query, 
                new { FornecedorId = fornecedorId });
        }
    }
}
