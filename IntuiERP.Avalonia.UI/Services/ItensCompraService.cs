using Dapper;
using IntuiERP.Avalonia.UI.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
{
    public class ItemCompraService
    {
        private readonly IDbConnection _connection;

        public ItemCompraService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ItemCompraModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM itens_compra";
            return await _connection.QueryAsync<ItemCompraModel>(query);
        }

        public async Task<ItemCompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM itens_compra WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemCompraModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemCompraModel item)
        {
            const string query =
                @"INSERT INTO itens_compra 
                (cod_compra, cod_produto, quantidade, preco_unitario) 
                VALUES 
                (@CodCompra, @CodProduto, @quantidade, @preco_unitario) RETURNING id;";

            return await _connection.ExecuteScalarAsync<int>(query, item);
        }

        public async Task<int> UpdateAsync(ItemCompraModel item)
        {
            const string query =
                @"UPDATE itens_compra SET 
                cod_compra = @CodCompra, 
                cod_produto = @CodProduto, 
                quantidade = @quantidade, 
                preco_unitario = @preco_unitario 
                WHERE id = @Id";

            return await _connection.ExecuteAsync(query, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM itens_compra WHERE id = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ItemCompraModel>> GetByCompraAsync(int compraId)
        {
            const string query = "SELECT * FROM itens_compra WHERE cod_compra = @CompraId";
            return await _connection.QueryAsync<ItemCompraModel>(query,
                new { CompraId = compraId });
        }

        public async Task<int> DeleteByCompraAsync(int compraId)
        {
            const string query = "DELETE FROM itens_compra WHERE cod_compra = @CompraId";
            return await _connection.ExecuteAsync(query, new { CompraId = compraId });
        }
    }
}
