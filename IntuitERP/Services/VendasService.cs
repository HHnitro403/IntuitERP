using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class VendasService
    {
        private readonly IDbConnection _connection;

        public VendasService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<VendaModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM vendas";
            return await _connection.QueryAsync<VendaModel>(query);
        }

        public async Task<VendaModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM vendas WHERE CodVenda = @Id";
            return await _connection.QueryFirstOrDefaultAsync<VendaModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(VendaModel venda)
        {
            const string query =
                @"INSERT INTO vendas 
            (data_venda, hora_venda, CodCliente, Desconto, CodVendedor, OBS, valor_total, forma_pagamento, status_venda) 
            VALUES 
            (@data_venda, @hora_venda, @CodCliente, @Desconto, @CodVendedor, @OBS, @valor_total, @forma_pagamento, @status_venda)";
            return await _connection.ExecuteAsync(query, venda);
        }

        public async Task<int> UpdateAsync(VendaModel venda)
        {
            const string query =
                @"UPDATE vendas SET 
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
            const string query = "DELETE FROM vendas WHERE CodVenda = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
