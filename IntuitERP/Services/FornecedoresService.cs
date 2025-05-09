using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class FornecedorService
    {
        private readonly IDbConnection _connection;

        public FornecedorService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<FornecedorModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM fornecedor";
            return await _connection.QueryAsync<FornecedorModel>(query);
        }

        public async Task<FornecedorModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM fornecedor WHERE CodFornecedor = @Id";
            return await _connection.QueryFirstOrDefaultAsync<FornecedorModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"INSERT INTO fornecedor 
                (CodCidade, RazaoSocial, NomeFantasia, CNPJ, Email, Telefone, 
                Endereco, DataCadastro, DataUltimaCompra, Ativo) 
                VALUES 
                (@CodCidade, @RazaoSocial, @NomeFantasia, @CNPJ, @Email, @Telefone, 
                @Endereco, @DataCadastro, @DataUltimaCompra, @Ativo);
                SELECT LAST_INSERT_ID();";

            if (fornecedor.DataCadastro == null)
                fornecedor.DataCadastro = DateTime.Now;
            if (fornecedor.Ativo == null)
                fornecedor.Ativo = true;

            return await _connection.ExecuteScalarAsync<int>(query, fornecedor);
        }

        public async Task<int> UpdateAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"UPDATE fornecedor SET 
                CodCidade = @CodCidade,
                RazaoSocial = @RazaoSocial, 
                NomeFantasia = @NomeFantasia, 
                CNPJ = @CNPJ, 
                Email = @Email, 
                Telefone = @Telefone, 
                Endereco = @Endereco, 
                DataUltimaCompra = @DataUltimaCompra, 
                Ativo = @Ativo 
                WHERE CodFornecedor = @CodFornecedor";
            return await _connection.ExecuteAsync(query, fornecedor);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM fornecedor WHERE CodFornecedor = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<FornecedorModel> GetByCNPJAsync(string cnpj)
        {
            const string query = "SELECT * FROM fornecedor WHERE CNPJ = @CNPJ";
            return await _connection.QueryFirstOrDefaultAsync<FornecedorModel>(query, new { CNPJ = cnpj });
        }

        public async Task<IEnumerable<FornecedorModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM fornecedor 
                WHERE RazaoSocial LIKE @SearchTerm 
                OR NomeFantasia LIKE @SearchTerm 
                OR CNPJ LIKE @SearchTerm";
            return await _connection.QueryAsync<FornecedorModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }

        public async Task UpdateUltimaCompraAsync(int fornecedorId)
        {
            const string query =
                @"UPDATE fornecedor SET 
                DataUltimaCompra = @DataUltimaCompra 
                WHERE CodFornecedor = @CodFornecedor";
            await _connection.ExecuteAsync(query,
                new { CodFornecedor = fornecedorId, DataUltimaCompra = DateTime.Now });
        }
    }
}
