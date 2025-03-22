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
    public class FornecedoresService
    {
        private readonly IDbConnection _connection;

        public FornecedoresService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<FornecedorModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM fornecedores";
            return await _connection.QueryAsync<FornecedorModel>(query);
        }

        public async Task<FornecedorModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM fornecedores WHERE CodFornec = @Id";
            return await _connection.QueryFirstOrDefaultAsync<FornecedorModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"INSERT INTO fornecedores 
            (Nome, CNPJ, Telefone, Email, Endereco) 
            VALUES 
            (@Nome, @CNPJ, @Telefone, @Email, @Endereco)";
            return await _connection.ExecuteAsync(query, fornecedor);
        }

        public async Task<int> UpdateAsync(FornecedorModel fornecedor)
        {
            const string query =
                @"UPDATE fornecedores SET 
            Nome = @Nome,
            CNPJ = @CNPJ,
            Telefone = @Telefone,
            Email = @Email,
            Endereco = @Endereco
            WHERE CodFornec = @CodFornec";
            return await _connection.ExecuteAsync(query, fornecedor);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM fornecedores WHERE CodFornec = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
