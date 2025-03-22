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
    public class ItensCompraService
    {
        private readonly IDbConnection _connection;

        public ItensCompraService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ItemCompraModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM itenscompra";
            return await _connection.QueryAsync<ItemCompraModel>(query);
        }

        public async Task<ItemCompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM itenscompra WHERE CodItemCompra = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemCompraModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemCompraModel itemCompra)
        {
            const string query =
                @"INSERT INTO itenscompra 
            (CodCompra, CodProduto, Quantidade, ValorUnitario) 
            VALUES 
            (@CodCompra, @CodProduto, @Quantidade, @ValorUnitario)";
            return await _connection.ExecuteAsync(query, itemCompra);
        }

        public async Task<int> UpdateAsync(ItemCompraModel itemCompra)
        {
            const string query =
                @"UPDATE itenscompra SET 
            CodCompra = @CodCompra,
            CodProduto = @CodProduto,
            Quantidade = @Quantidade,
            ValorUnitario = @ValorUnitario
            WHERE CodItemCompra = @CodItemCompra";
            return await _connection.ExecuteAsync(query, itemCompra);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM itenscompra WHERE CodItemCompra = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
