using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
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
            const string query = "SELECT * FROM estoque WHERE CodEst = @Id";
            return await _connection.QueryFirstOrDefaultAsync<EstoqueModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(EstoqueModel estoque)
        {
            const string query =
                @"INSERT INTO estoque 
                (CodProduto, Tipo, Qtd, Data) 
                VALUES 
                (@CodProduto, @Tipo, @Qtd, @Data);
                SELECT LAST_INSERT_ID();";

            if (estoque.Data == DateTime.MinValue)
                estoque.Data = DateTime.Now;

            return await _connection.ExecuteScalarAsync<int>(query, estoque);
        }

        public async Task<int> UpdateAsync(EstoqueModel estoque)
        {
            const string query =
                @"UPDATE estoque SET 
                CodProduto = @CodProduto, 
                Tipo = @Tipo, 
                Qtd = @Qtd, 
                Data = @Data 
                WHERE CodEst = @CodEst";
            return await _connection.ExecuteAsync(query, estoque);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM estoque WHERE CodEst = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<EstoqueModel>> GetByProdutoAsync(int produtoId)
        {
            const string query = "SELECT * FROM estoque WHERE CodProduto = @ProdutoId";
            return await _connection.QueryAsync<EstoqueModel>(query,
                new { ProdutoId = produtoId });
        }

        public async Task<int> AtualizarSaldoAsync(int produtoId, decimal quantidade, char tipo)
        {
            EstoqueModel estoque = new EstoqueModel
            {
                CodProduto = produtoId,
                Tipo = tipo,
                Qtd = quantidade,
                Data = DateTime.Now
            };

            return await InsertAsync(estoque);
        }
    }
}
