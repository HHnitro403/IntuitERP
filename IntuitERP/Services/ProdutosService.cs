using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class ProdutoService
    {
        private readonly IDbConnection _connection;

        public ProdutoService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ProdutoModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM produto";
            return await _connection.QueryAsync<ProdutoModel>(query);
        }

        public async Task<ProdutoModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM produto WHERE CodProduto = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ProdutoModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ProdutoModel produto)
        {
            const string query =
                @"INSERT INTO produto 
                (Descricao, Categoria, PrecoUnitario, SaldoEst, FornecedorP_ID, 
                DataCadastro, EstMinimo, EstoqueID, VarianteID, Tipo, Ativo) 
                VALUES 
                (@Descricao, @Categoria, @PrecoUnitario, @SaldoEst, @FornecedorP_ID, 
                @DataCadastro, @EstMinimo, @EstoqueID, @VarianteID, @Tipo, @Ativo);
                SELECT LAST_INSERT_ID();";

            if (produto.DataCadastro == null)
                produto.DataCadastro = DateTime.Now;
            if (produto.Ativo == null)
                produto.Ativo = true;
            if (produto.SaldoEst == null)
                produto.SaldoEst = 0;

            return await _connection.ExecuteScalarAsync<int>(query, produto);
        }

        public async Task<int> UpdateAsync(ProdutoModel produto)
        {
            const string query =
                @"UPDATE produto SET 
                Descricao = @Descricao, 
                Categoria = @Categoria, 
                PrecoUnitario = @PrecoUnitario, 
                SaldoEst = @SaldoEst, 
                FornecedorP_ID = @FornecedorP_ID, 
                EstMinimo = @EstMinimo, 
                EstoqueID = @EstoqueID, 
                VarianteID = @VarianteID, 
                Tipo = @Tipo, 
                Ativo = @Ativo 
                WHERE CodProduto = @CodProduto";
            return await _connection.ExecuteAsync(query, produto);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM produto WHERE CodProduto = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ProdutoModel>> GetByFornecedorAsync(int fornecedorId)
        {
            const string query = "SELECT * FROM produto WHERE FornecedorP_ID = @FornecedorId";
            return await _connection.QueryAsync<ProdutoModel>(query,
                new { FornecedorId = fornecedorId });
        }

        public async Task<IEnumerable<ProdutoModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM produto 
                WHERE Descricao LIKE @SearchTerm 
                OR Categoria LIKE @SearchTerm 
                OR Tipo LIKE @SearchTerm";
            return await _connection.QueryAsync<ProdutoModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }

        public async Task<IEnumerable<ProdutoModel>> GetByCategoriaAsync(string categoria)
        {
            const string query = "SELECT * FROM produto WHERE Categoria = @Categoria";
            return await _connection.QueryAsync<ProdutoModel>(query, new { Categoria = categoria });
        }

        public async Task<int> AtualizarEstoqueAsync(int produtoId, int quantidade)
        {
            const string query =
                @"UPDATE produto SET 
                SaldoEst = SaldoEst + @Quantidade 
                WHERE CodProduto = @ProdutoId";
            return await _connection.ExecuteAsync(query,
                new { ProdutoId = produtoId, Quantidade = quantidade });
        }
    }
}
