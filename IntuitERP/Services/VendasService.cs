﻿using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class VendaService
    {
        private readonly IDbConnection _connection;

        public VendaService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<VendaModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM venda Order by CodVenda";
            return await _connection.QueryAsync<VendaModel>(query);
        }

        public async Task<IEnumerable<VendaModel>> GetAllAsync(VendaFilterModel filters)
        {
            // Base query selects all columns from your table
            var sqlBuilder = new StringBuilder("SELECT * FROM venda");
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            // Filter by CodCliente
            if (filters.CodCliente.HasValue)
            {
                whereClauses.Add("CodCliente = @CodCliente");
                parameters.Add("@CodCliente", filters.CodCliente.Value, DbType.Int32);
            }

            // Filter by CodVendedor
            if (filters.CodVendedor.HasValue)
            {
                whereClauses.Add("CodVendedor = @CodVendedor");
                parameters.Add("@CodVendedor", filters.CodVendedor.Value, DbType.Int32);
            }

            // Filter by start date (>=) using the 'data_venda' column
            if (filters.DataInicial.HasValue)
            {
                whereClauses.Add("data_venda >= @DataInicial");
                parameters.Add("@DataInicial", filters.DataInicial.Value, DbType.Date);
            }

            // Filter by end date (<=) using the 'data_venda' column
            if (filters.DataFinal.HasValue)
            {
                whereClauses.Add("data_venda <= @DataFinal");
                parameters.Add("@DataFinal", filters.DataFinal.Value, DbType.Date);
            }

            // Filter by Minimum Total Value using the 'valor_total' column
            if (filters.ValorTotalMinimo.HasValue)
            {
                whereClauses.Add("valor_total >= @ValorTotalMinimo");
                parameters.Add("@ValorTotalMinimo", filters.ValorTotalMinimo.Value, DbType.Decimal);
            }

            // Filter by Maximum Total Value using the 'valor_total' column
            if (filters.ValorTotalMaximo.HasValue)
            {
                whereClauses.Add("valor_total <= @ValorTotalMaximo");
                parameters.Add("@ValorTotalMaximo", filters.ValorTotalMaximo.Value, DbType.Decimal);
            }

            // Filter by Payment Method (exact match)
            if (!string.IsNullOrWhiteSpace(filters.FormaPagamento))
            {
                whereClauses.Add("forma_pagamento = @FormaPagamento");
                parameters.Add("@FormaPagamento", filters.FormaPagamento, DbType.String);
            }

            // Filter by Status
            if (filters.StatusVenda.HasValue)
            {
                whereClauses.Add("status_venda = @StatusVenda");
                parameters.Add("@StatusVenda", filters.StatusVenda.Value, DbType.Int32);
            }


            // If any clauses were added, append them to the main query
            if (whereClauses.Any())
            {
                sqlBuilder.Append(" WHERE ");
                sqlBuilder.Append(string.Join(" AND ", whereClauses));
            }

            // Always add the ordering at the end
            sqlBuilder.Append(" ORDER BY CodVenda DESC"); // Order by most recent sales

            // Execute the final, dynamically built query
            string finalQuery = sqlBuilder.ToString();
            return await _connection.QueryAsync<VendaModel>(finalQuery, parameters);
        }

        public async Task<VendaModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM venda WHERE CodVenda = @Id";
            return await _connection.QueryFirstOrDefaultAsync<VendaModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(VendaModel venda)
        {
            const string query =
                @"INSERT INTO venda 
                (data_venda, hora_venda, CodCliente, Desconto, CodVendedor, 
                OBS, valor_total, forma_pagamento, status_venda) 
                VALUES 
                (@data_venda, @hora_venda, @CodCliente, @Desconto, @CodVendedor, 
                @OBS, @valor_total, @forma_pagamento, @status_venda);
                SELECT LAST_INSERT_ID();";

            if (venda.data_venda == null)
                venda.data_venda = DateTime.Now;
            if (venda.hora_venda == null)
                venda.hora_venda = DateTime.Now.TimeOfDay;

            return await _connection.ExecuteScalarAsync<int>(query, venda);
        }

        public async Task<int> UpdateAsync(VendaModel venda)
        {
            const string query =
                @"UPDATE venda SET 
                data_venda = @data_venda, 
                hora_venda = @hora_venda, 
                CodCliente = @CodCliente, 
                Desconto = @Desconto, 
                CodVendedor = @CodVendedor, 
                OBS = @OBS, 
                valor_total = @valor_total, 
                forma_pagamento = @forma_pagamento, 
                status_venda = @status_venda 
                WHERE CodVenda = @CodVenda";
            return await _connection.ExecuteAsync(query, venda);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM venda WHERE CodVenda = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<int> UpdateStatusAsync(int id, byte status)
        {
            const string query = "UPDATE venda SET status_venda = @Status WHERE CodVenda = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id, Status = status });
        }

        public async Task<IEnumerable<VendaModel>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            const string query =
                @"SELECT * FROM venda 
                WHERE data_venda BETWEEN @StartDate AND @EndDate";
            return await _connection.QueryAsync<VendaModel>(query,
                new { StartDate = startDate, EndDate = endDate });
        }

        public async Task<IEnumerable<VendaModel>> GetByClienteAsync(int clienteId)
        {
            const string query = "SELECT * FROM venda WHERE CodCliente = @ClienteId";
            return await _connection.QueryAsync<VendaModel>(query,
                new { ClienteId = clienteId });
        }

        public async Task<IEnumerable<VendaModel>> GetByVendedorAsync(int vendedorId)
        {
            const string query = "SELECT * FROM venda WHERE CodVendedor = @VendedorId";
            return await _connection.QueryAsync<VendaModel>(query,
                new { VendedorId = vendedorId });
        }

        public async Task AtualizarClienteUltimaCompraAsync(int clienteId)
        {
            const string query =
                @"UPDATE cliente SET 
                DataUltimaCompra = @DataUltimaCompra 
                WHERE CodCliente = @CodCliente";
            await _connection.ExecuteAsync(query,
                new { CodCliente = clienteId, DataUltimaCompra = DateTime.Now });
        }
    }
}
