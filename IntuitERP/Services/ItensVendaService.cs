using Dapper;
using IntuitERP.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class ItemVendaService
    {
        private readonly IDbConnection _connection;

        public ItemVendaService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ItemVendaModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM itensvenda";
            return await _connection.QueryAsync<ItemVendaModel>(query);
        }

        public async Task<ItemVendaModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM itensvenda WHERE CodItem = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemVendaModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemVendaModel item)
        {
            const string query =
                @"INSERT INTO itensvenda 
                (CodVenda, CodProduto, Descricao, quantidade, valor_unitario, valor_total, desconto) 
                VALUES 
                (@CodVenda, @CodProduto, @Descricao, @quantidade, @valor_unitario, @valor_total, @desconto);
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

        public async Task<int> UpdateAsync(ItemVendaModel item)
        {
            const string query =
                @"UPDATE itensvenda SET 
                CodVenda = @CodVenda, 
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
            const string query = "DELETE FROM itensvenda WHERE CodItem = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ItemVendaModel>> GetByVendaAsync(int vendaId)
        {
            const string query = "SELECT * FROM itensvenda WHERE CodVenda = @VendaId";
            return await _connection.QueryAsync<ItemVendaModel>(query,
                new { VendaId = vendaId });
        }

        public async Task<int> DeleteByVendaAsync(int vendaId)
        {
            const string query = "DELETE FROM itensvenda WHERE CodVenda = @VendaId";
            return await _connection.ExecuteAsync(query, new { VendaId = vendaId });
        }
    }
}
