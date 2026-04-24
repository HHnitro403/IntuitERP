using Dapper;
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
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
            const string query = "SELECT * FROM estoque WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<EstoqueModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(EstoqueModel estoque)
        {
            const string query =
                @"INSERT INTO estoque 
                (cod_produto, tipo, qtd, data) 
                VALUES 
                (@CodProduto, @Tipo, @Qtd, @Data) RETURNING id;";

            if (estoque.Data == DateTime.MinValue)
                estoque.Data = DateTime.Now;

            return await _connection.ExecuteScalarAsync<int>(query, estoque);
        }

        public async Task<int> UpdateAsync(EstoqueModel estoque)
        {
            const string query =
                @"UPDATE estoque SET 
                cod_produto = @CodProduto, 
                tipo = @Tipo, 
                qtd = @Qtd, 
                data = @Data 
                WHERE id = @Id";
            return await _connection.ExecuteAsync(query, estoque);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM estoque WHERE id = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<EstoqueModel>> GetByProdutoAsync(int produtoId)
        {
            const string query = "SELECT * FROM estoque WHERE cod_produto = @ProdutoId";
            return await _connection.QueryAsync<EstoqueModel>(query,
                new { ProdutoId = produtoId });
        }

        public async Task<int> AtualizarSaldoAsync(int produtoId, int quantidade, char tipo)
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
