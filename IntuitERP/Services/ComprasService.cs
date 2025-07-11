﻿using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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

        public async Task<IEnumerable<CompraModel>> GetAllComprasAsync(CompraFilterModel filters)
        {
            var sqlBuilder = new StringBuilder("SELECT * FROM compra");
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            // Filter by Supplier (CodFornec)
            if (filters.CodFornec.HasValue)
            {
                whereClauses.Add("CodFornec = @CodFornec");
                parameters.Add("@CodFornec", filters.CodFornec.Value);
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

            sqlBuilder.Append(" ORDER BY CodCompra DESC");

            return await _connection.QueryAsync<CompraModel>(sqlBuilder.ToString(), parameters);
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
