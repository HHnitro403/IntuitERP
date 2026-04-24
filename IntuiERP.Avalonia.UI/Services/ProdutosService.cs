using Dapper;
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
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
                whereClauses.Add("cod_produto = @CodProduto");
                parameters.Add("@CodProduto", filter.CodProduto.Value);
            }

            if (filter.Descricao != null)
            {
                whereClauses.Add("descricao LIKE @Descricao");
                parameters.Add("@Descricao", $"%{filter.Descricao}%");
            }

            if (filter.Categoria != null)
            {
                whereClauses.Add("categoria = @Categoria");
                parameters.Add("@Categoria", filter.Categoria);
            }

            if (filter.PrecoUnitario.HasValue)
            {
                whereClauses.Add("preco_unitario = @PrecoUnitario");
                parameters.Add("@PrecoUnitario", filter.PrecoUnitario.Value);
            }

            if (filter.SaldoEst.HasValue)
            {
                whereClauses.Add("saldo_est = @SaldoEst");
                parameters.Add("@SaldoEst", filter.SaldoEst.Value);
            }

            if (filter.FornecedorId.HasValue)
            {
                whereClauses.Add("fornecedor_id = @FornecedorId");
                parameters.Add("@FornecedorId", filter.FornecedorId.Value);
            }

            if (filter.DataCadastro.HasValue)
            {
                whereClauses.Add("data_cadastro = @DataCadastro");
                parameters.Add("@DataCadastro", filter.DataCadastro.Value);
            }

            if (filter.EstMinimo.HasValue)
            {
                whereClauses.Add("est_minimo = @EstMinimo");
                parameters.Add("@EstMinimo", filter.EstMinimo.Value);
            }

            if (filter.comparativo && filter.positivo)
            {
                whereClauses.Add("saldo_est > est_minimo");
            }
            else if (filter.comparativo && !filter.positivo)
            {
                whereClauses.Add("saldo_est < est_minimo");
            }
            else
            {
                if (filter.EstoqueId.HasValue)
                {
                    whereClauses.Add("estoque_id = @EstoqueId");
                    parameters.Add("@EstoqueId", filter.EstoqueId.Value);
                }

                if (filter.VarianteId.HasValue)
                {
                    whereClauses.Add("variante_id = @VarianteId");
                    parameters.Add("@VarianteId", filter.VarianteId.Value);
                }
            }

            if (filter.Tipo != null)
            {
                whereClauses.Add("tipo = @Tipo");
                parameters.Add("@Tipo", filter.Tipo);
            }

            if (filter.Ativo.HasValue)
            {
                whereClauses.Add("ativo = @Ativo");
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
            const string query = "SELECT * FROM produto WHERE cod_produto = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ProdutoModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ProdutoModel produto)
        {
            const string query =
                @"INSERT INTO produto 
                (descricao, categoria, preco_unitario, saldo_est, fornecedor_id, 
                data_cadastro, est_minimo, estoque_id, variante_id, tipo, ativo) 
                VALUES 
                (@Descricao, @Categoria, @PrecoUnitario, @SaldoEst, @FornecedorId, 
                @DataCadastro, @EstMinimo, @EstoqueId, @VarianteId, @Tipo, @Ativo) RETURNING cod_produto;";

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
                descricao = @Descricao, 
                categoria = @Categoria, 
                preco_unitario = @PrecoUnitario, 
                saldo_est = @SaldoEst, 
                fornecedor_id = @FornecedorId, 
                est_minimo = @EstMinimo, 
                estoque_id = @EstoqueId, 
                variante_id = @VarianteId, 
                tipo = @Tipo, 
                ativo = @Ativo 
                WHERE cod_produto = @CodProduto";
            return await _connection.ExecuteAsync(query, produto);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM produto WHERE cod_produto = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ProdutoModel>> GetByFornecedorAsync(int fornecedorId)
        {
            const string query = "SELECT * FROM produto WHERE fornecedor_id = @FornecedorId";
            return await _connection.QueryAsync<ProdutoModel>(query,
                new { FornecedorId = fornecedorId });
        }

        public async Task<IEnumerable<ProdutoModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM produto 
                WHERE descricao LIKE @SearchTerm 
                OR categoria LIKE @SearchTerm 
                OR tipo LIKE @SearchTerm";
            return await _connection.QueryAsync<ProdutoModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }

        public async Task<IEnumerable<ProdutoModel>> GetByCategoriaAsync(string categoria)
        {
            const string query = "SELECT * FROM produto WHERE categoria = @Categoria";
            return await _connection.QueryAsync<ProdutoModel>(query, new { Categoria = categoria });
        }

        public async Task<int> AtualizarEstoqueAsync(int produtoId, int quantidade)
        {
            const string query =
                @"UPDATE produto SET 
                saldo_est = saldo_est + @Quantidade 
                WHERE cod_produto = @ProdutoId";
            return await _connection.ExecuteAsync(query,
                new { ProdutoId = produtoId, Quantidade = quantidade });
        }
    }
}
