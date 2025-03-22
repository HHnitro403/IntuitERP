using Dapper;
using IntuitERP.models;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class ComprasService
    {
        private readonly IDbConnection _connection;

        public ComprasService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<CompraModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM compras";
            return await _connection.QueryAsync<CompraModel>(query);
        }

        public async Task<CompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM compras WHERE CodCompra = @Id";
            return await _connection.QueryFirstOrDefaultAsync<CompraModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(CompraModel compra)
        {
            const string query =
                @"INSERT INTO compras 
            (data_compra, hora_compra, CodFornec, Desconto, CodVendedor, OBS, valor_total, forma_pagamento, status_compra) 
            VALUES 
            (@data_compra, @hora_compra, @CodFornec, @Desconto, @CodVendedor, @OBS, @valor_total, @forma_pagamento, @status_compra)";
            return await _connection.ExecuteAsync(query, compra);
        }

        public async Task<int> UpdateAsync(CompraModel compra)
        {
            const string query =
                @"UPDATE compras SET 
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
            const string query = "DELETE FROM compras WHERE CodCompra = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
