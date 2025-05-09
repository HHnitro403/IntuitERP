using Dapper;
using IntuitERP.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
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
            const string query = "SELECT * FROM itenscompra";
            return await _connection.QueryAsync<ItemCompraModel>(query);
        }

        public async Task<ItemCompraModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM itenscompra WHERE CodItem = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemCompraModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemCompraModel item)
        {
            const string query =
                @"INSERT INTO itenscompra 
                (CodCompra, CodProduto, Descricao, quantidade, valor_unitario, valor_total, desconto) 
                VALUES 
                (@CodCompra, @CodProduto, @Descricao, @quantidade, @valor_unitario, @valor_total, @desconto);
                SELECT LAST_INSERT_ID();";

            // Calculate total value if not provided
            if (!item.valor_total.HasValue && item.quantidade.HasValue && item.valor_unitario.HasValue)
            {
                decimal totalBeforeDiscount = item.quantidade.Value * item.valor_unitario.Value;
                item.valor_total = item.desconto.HasValue ?
                    totalBeforeDiscount - item.desconto.Value : totalBeforeDiscount;
            }

            return await _connection.ExecuteScalarAsync<int>(query, item);
        }

        public async Task<int> UpdateAsync(ItemCompraModel item)
        {
            const string query =
                @"UPDATE itenscompra SET 
                CodCompra = @CodCompra, 
                CodProduto = @CodProduto, 
                Descricao = @Descricao, 
                quantidade = @quantidade, 
                valor_unitario = @valor_unitario, 
                valor_total = @valor_total, 
                desconto = @desconto 
                WHERE CodItem = @CodItem";

            // Calculate total value if not provided
            if (!item.valor_total.HasValue && item.quantidade.HasValue && item.valor_unitario.HasValue)
            {
                decimal totalBeforeDiscount = item.quantidade.Value * item.valor_unitario.Value;
                item.valor_total = item.desconto.HasValue ?
                    totalBeforeDiscount - item.desconto.Value : totalBeforeDiscount;
            }

            return await _connection.ExecuteAsync(query, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM itenscompra WHERE CodItem = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ItemCompraModel>> GetByCompraAsync(int compraId)
        {
            const string query = "SELECT * FROM itenscompra WHERE CodCompra = @CompraId";
            return await _connection.QueryAsync<ItemCompraModel>(query,
                new { CompraId = compraId });
        }

        public async Task<int> DeleteByCompraAsync(int compraId)
        {
            const string query = "DELETE FROM itenscompra WHERE CodCompra = @CompraId";
            return await _connection.ExecuteAsync(query, new { CompraId = compraId });
        }
    }
}
