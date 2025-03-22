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
    public class ProdutosService
    {
        private readonly IDbConnection _connection;

        public ProdutosService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ProdutoModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM produtos";
            return await _connection.QueryAsync<ProdutoModel>(query);
        }

        public async Task<ProdutoModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM produtos WHERE CodProduto = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ProdutoModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ProdutoModel produto)
        {
            const string query =
                @"INSERT INTO produtos 
            (Descricao, Categoria, PrecoUnitario, SaldoEst, FornecedorP_ID, DataCadastro, EstMinimo, EstoqueID, VarianteID, Tipo, Ativo) 
            VALUES 
            (@Descricao, @Categoria, @PrecoUnitario, @SaldoEst, @FornecedorP_ID, @DataCadastro, @EstMinimo, @EstoqueID, @VarianteID, @Tipo, @Ativo)";
            return await _connection.ExecuteAsync(query, produto);
        }

        public async Task<int> UpdateAsync(ProdutoModel produto)
        {
            const string query =
                @"UPDATE produtos SET 
            Descricao = @Descricao,
            Categoria = @Categoria,
            PrecoUnitario = @PrecoUnitario,
            SaldoEst = @SaldoEst,
            FornecedorP_ID = @FornecedorP_ID,
            DataCadastro = @DataCadastro,
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
            const string query = "DELETE FROM produtos WHERE CodProduto = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
