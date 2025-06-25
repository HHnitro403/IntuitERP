using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
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
            const string query = "SELECT * FROM compra Order by CodCompra";
            return await _connection.QueryAsync<CompraModel>(query);
        }

        public async Task<CompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM compra WHERE CodCompra = @Id";
            return await _connection.QueryFirstOrDefaultAsync<CompraModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(CompraModel compra)
        {
            const string query = 
                @"INSERT INTO compra 
                (data_compra, hora_compra, CodFornec, Desconto, CodVendedor, 
                OBS, valor_total, forma_pagamento, status_compra) 
                VALUES 
                (@data_compra, @hora_compra, @CodFornec, @Desconto, @CodVendedor, 
                @OBS, @valor_total, @forma_pagamento, @status_compra);
                SELECT LAST_INSERT_ID();";
            
            if (compra.data_compra == null)
                compra.data_compra = DateTime.Now;
            if (compra.hora_compra == null)
                compra.hora_compra = DateTime.Now.TimeOfDay;
                
            return await _connection.ExecuteScalarAsync<int>(query, compra);
        }

        public async Task<int> UpdateAsync(CompraModel compra)
        {
            const string query = 
                @"UPDATE compra SET 
                data_compra = @data_compra, 
                hora_compra = @hora_compra, 
                CodFornec = @CodFornec, 
                Desconto = @Desconto, 
                CodVendedor = @CodVendedor, 
                OBS = @OBS, 
                valor_total = @valor_total, 
                forma_pagamento = @forma_pagamento, 
                status_compra = @status_compra 
                WHERE CodCompra = @CodCompra";
            return await _connection.ExecuteAsync(query, compra);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM compra WHERE CodCompra = @Id";
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
            const string query = "SELECT * FROM compra WHERE CodFornec = @FornecedorId";
            return await _connection.QueryAsync<CompraModel>(query, 
                new { FornecedorId = fornecedorId });
        }
    }
}
