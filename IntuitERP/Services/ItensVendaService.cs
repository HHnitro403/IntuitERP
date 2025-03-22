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
    public class ItensVendaService
    {
        private readonly IDbConnection _connection;

        public ItensVendaService(IDbConnection connection)
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
            const string query = "SELECT * FROM itensvenda WHERE CodItemVenda = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ItemVendaModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ItemVendaModel itemVenda)
        {
            const string query =
                @"INSERT INTO itensvenda 
            (CodVenda, CodProduto, Quantidade, ValorUnitario) 
            VALUES 
            (@CodVenda, @CodProduto, @Quantidade, @ValorUnitario)";
            return await _connection.ExecuteAsync(query, itemVenda);
        }

        public async Task<int> UpdateAsync(ItemVendaModel itemVenda)
        {
            const string query =
                @"UPDATE itensvenda SET 
            CodVenda = @CodVenda,
            CodProduto = @CodProduto,
            Quantidade = @Quantidade,
            ValorUnitario = @ValorUnitario
            WHERE CodItemVenda = @CodItemVenda";
            return await _connection.ExecuteAsync(query, itemVenda);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM itensvenda WHERE CodItemVenda = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
