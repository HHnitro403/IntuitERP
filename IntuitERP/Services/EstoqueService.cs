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
    public class EstoqueService
    {
        private readonly IDbConnection _connection;

        public EstoqueService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<EstoqueModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM estoque";
            return await _connection.QueryAsync<EstoqueModel>(query);
        }

        public async Task<EstoqueModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM estoque WHERE CodEstoque = @Id";
            return await _connection.QueryFirstOrDefaultAsync<EstoqueModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(EstoqueModel estoque)
        {
            const string query =
                @"INSERT INTO estoque 
            (CodProduto, Quantidade, Localizacao) 
            VALUES 
            (@CodProduto, @Quantidade, @Localizacao)";
            return await _connection.ExecuteAsync(query, estoque);
        }

        public async Task<int> UpdateAsync(EstoqueModel estoque)
        {
            const string query =
                @"UPDATE estoque SET 
            CodProduto = @CodProduto,
            Quantidade = @Quantidade,
            Localizacao = @Localizacao
            WHERE CodEstoque = @CodEstoque";
            return await _connection.ExecuteAsync(query, estoque);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM estoque WHERE CodEstoque = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
