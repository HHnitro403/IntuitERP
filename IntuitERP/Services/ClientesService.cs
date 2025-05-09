using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class ClienteService
    {
        private readonly IDbConnection _connection;

        public ClienteService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ClienteModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM cliente";
            return await _connection.QueryAsync<ClienteModel>(query);
        }

        public async Task<ClienteModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM cliente WHERE CodCliente = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ClienteModel cliente)
        {
            const string query =
                @"INSERT INTO cliente 
                (CodCidade, Nome, Email, Telefone, DataNascimento, CPF, Endereco, 
                 Numero, Bairro, CEP, DataCadastro, DataUltimaCompra, Ativo) 
                VALUES 
                (@CodCidade, @Nome, @Email, @Telefone, @DataNascimento, @CPF, @Endereco,
                 @Numero, @Bairro, @CEP, @DataCadastro, @DataUltimaCompra, @Ativo);
                SELECT LAST_INSERT_ID();";

            if (cliente.DataCadastro == null)
                cliente.DataCadastro = DateTime.Now;

            return await _connection.ExecuteScalarAsync<int>(query, cliente);
        }

        public async Task<int> UpdateAsync(ClienteModel cliente)
        {
            const string query =
                @"UPDATE cliente SET 
                CodCidade = @CodCidade,
                Nome = @Nome, 
                Email = @Email, 
                Telefone = @Telefone, 
                DataNascimento = @DataNascimento, 
                CPF = @CPF, 
                Endereco = @Endereco,
                Numero = @Numero,
                Bairro = @Bairro,
                CEP = @CEP,
                DataUltimaCompra = @DataUltimaCompra, 
                Ativo = @Ativo 
                WHERE CodCliente = @CodCliente";
            return await _connection.ExecuteAsync(query, cliente);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM cliente WHERE CodCliente = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ClienteModel> GetByEmailAsync(string email)
        {
            const string query = "SELECT * FROM cliente WHERE Email = @Email";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { Email = email });
        }

        public async Task<ClienteModel> GetByCPFAsync(string cpf)
        {
            const string query = "SELECT * FROM cliente WHERE CPF = @CPF";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { CPF = cpf });
        }

        public async Task<IEnumerable<ClienteModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM cliente 
                WHERE Nome LIKE @SearchTerm 
                OR Email LIKE @SearchTerm 
                OR CPF LIKE @SearchTerm";
            return await _connection.QueryAsync<ClienteModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }
    }
}
