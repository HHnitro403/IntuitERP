using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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

        public async Task<IEnumerable<ProdutoModel>> GetAllAsync(ProdutoFilterModel filter)
        {
            var sqlBuilder = new StringBuilder("SELECT * FROM produto");
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            if (filter.CodProduto.HasValue)
            {
                whereClauses.Add("CodProduto = @CodProduto");
                parameters.Add("@CodProduto", filter.CodProduto.Value);
            }

            if (filter.Descricao != null)
            {
                whereClauses.Add("Descricao LIKE @Descricao");
                parameters.Add("@Descricao", $"%{filter.Descricao}%");
            }

            if (filter.Categoria != null)
            {
                whereClauses.Add("Categoria = @Categoria");
                parameters.Add("@Categoria", filter.Categoria);
            }

            if (filter.PrecoUnitario.HasValue)
            {
                whereClauses.Add("PrecoUnitario = @PrecoUnitario");
                parameters.Add("@PrecoUnitario", filter.PrecoUnitario.Value);
            }

            if (filter.SaldoEst.HasValue)
            {
                whereClauses.Add("SaldoEst = @SaldoEst");
                parameters.Add("@SaldoEst", filter.SaldoEst.Value);
            }

            if (filter.FornecedorP_ID.HasValue)
            {
                whereClauses.Add("FornecedorP_ID = @FornecedorP_ID");
                parameters.Add("@FornecedorP_ID", filter.FornecedorP_ID.Value);
            }

            if (filter.DataCadastro.HasValue)
            {
                whereClauses.Add("DataCadastro = @DataCadastro");
                parameters.Add("@DataCadastro", filter.DataCadastro.Value);
            }

            if (filter.EstMinimo.HasValue)
            {
                whereClauses.Add("EstMinimo = @EstMinimo");
                parameters.Add("@EstMinimo", filter.EstMinimo.Value);
            }

            if (filter.comparativo && filter.positivo)
            {
                whereClauses.Add("SaldoEst > EstMinimo");
            }
            else if (filter.comparativo && !filter.positivo)
            {
                whereClauses.Add("SaldoEst < EstMinimo");
            }
            else
            {
                if (filter.EstoqueID.HasValue)
                {
                    whereClauses.Add("EstoqueID = @EstoqueID");
                    parameters.Add("@EstoqueID", filter.EstoqueID.Value);
                }

                if (filter.VarianteID.HasValue)
                {
                    whereClauses.Add("VarianteID = @VarianteID");
                    parameters.Add("@VarianteID", filter.VarianteID.Value);
                }
            }

            if (filter.Tipo != null)
            {
                whereClauses.Add("Tipo = @Tipo");
                parameters.Add("@Tipo", filter.Tipo);
            }

            if (filter.Ativo.HasValue)
            {
                whereClauses.Add("Ativo = @Ativo");
                parameters.Add("@Ativo", filter.Ativo.Value);
            }

            if (whereClauses.Count > 0)
            {
                sqlBuilder.Append(" WHERE ");
                sqlBuilder.Append(string.Join(" AND ", whereClauses));
            }

            var result = await _connection.QueryAsync<ProdutoModel>(sqlBuilder.ToString(), parameters);

            return result;
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
