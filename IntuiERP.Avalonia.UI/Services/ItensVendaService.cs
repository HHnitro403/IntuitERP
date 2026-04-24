using Dapper;
using IntuiERP.Avalonia.UI.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
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
            const string query = "SELECT * FROM itens_venda";
            return await _connection.QueryAsync<ItemVendaModel>(query);
        }

        public async Task<ItemVendaModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM itens_venda WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemVendaModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemVendaModel item)
        {
            const string query =
                @"INSERT INTO itens_venda 
                (cod_venda, cod_produto, descricao, quantidade, preco_unitario) 
                VALUES 
                (@CodVenda, @CodProduto, @Descricao, @quantidade, @valor_unitario) RETURNING id;";

            return await _connection.ExecuteScalarAsync<int>(query, item);
        }

        public async Task<int> UpdateAsync(ItemVendaModel item)
        {
            const string query =
                @"UPDATE itens_venda SET 
                cod_venda = @CodVenda, 
                cod_produto = @CodProduto, 
                descricao = @Descricao, 
                quantidade = @quantidade, 
                preco_unitario = @valor_unitario 
                WHERE id = @CodItem";

            return await _connection.ExecuteAsync(query, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM itens_venda WHERE id = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ItemVendaModel>> GetByVendaAsync(int vendaId)
        {
            const string query = "SELECT * FROM itens_venda WHERE cod_venda = @VendaId";
            return await _connection.QueryAsync<ItemVendaModel>(query,
                new { VendaId = vendaId });
        }

        public async Task<int> DeleteByVendaAsync(int vendaId)
        {
            const string query = "DELETE FROM itens_venda WHERE cod_venda = @VendaId";
            return await _connection.ExecuteAsync(query, new { VendaId = vendaId });
        }
    }
}
