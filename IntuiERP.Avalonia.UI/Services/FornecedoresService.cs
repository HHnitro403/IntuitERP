using Dapper;
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
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
            const string query = "SELECT * FROM fornecedor WHERE cod_fornecedor = @Id";
            return await _connection.QueryFirstOrDefaultAsync<FornecedorModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"INSERT INTO fornecedor 
                (cod_cidade, razao_social, nome_fantasia, cnpj, email, telefone, 
                endereco, numero, bairro, cep, ativo) 
                VALUES 
                (@CodCidade, @RazaoSocial, @NomeFantasia, @CNPJ, @Email, @Telefone, 
                @Endereco, @Numero, @Bairro, @CEP, @Ativo) RETURNING cod_fornecedor;";

            if (fornecedor.Ativo == null)
                fornecedor.Ativo = true;

            return await _connection.ExecuteScalarAsync<int>(query, fornecedor);
        }

        public async Task<int> UpdateAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"UPDATE fornecedor SET 
                cod_cidade = @CodCidade,
                razao_social = @RazaoSocial, 
                nome_fantasia = @NomeFantasia, 
                cnpj = @CNPJ, 
                email = @Email, 
                telefone = @Telefone, 
                endereco = @Endereco, 
                numero = @Numero,
                bairro = @Bairro,
                cep = @CEP,
                ativo = @Ativo 
                WHERE cod_fornecedor = @CodFornecedor";
            return await _connection.ExecuteAsync(query, fornecedor);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM fornecedor WHERE cod_fornecedor = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<FornecedorModel> GetByCNPJAsync(string cnpj)
        {
            const string query = "SELECT * FROM fornecedor WHERE cnpj = @CNPJ";
            return await _connection.QueryFirstOrDefaultAsync<FornecedorModel>(query, new { CNPJ = cnpj });
        }

        public async Task<IEnumerable<FornecedorModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM fornecedor 
                WHERE razao_social LIKE @SearchTerm 
                OR nome_fantasia LIKE @SearchTerm 
                OR cnpj LIKE @SearchTerm";
            return await _connection.QueryAsync<FornecedorModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }
    }
}
