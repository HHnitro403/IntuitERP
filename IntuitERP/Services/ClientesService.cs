using Dapper;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntuitERP.models;

namespace IntuitERP.Services
{
    public class ClientesService
    {
        private readonly IDbConnection _connection;

        public ClientesService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<ClienteModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM clientes";
            return await _connection.QueryAsync<ClienteModel>(query);
        }

        public async Task<ClienteModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM clientes WHERE CodCliente = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ClienteModel cliente)
        {
            const string query =
                @"INSERT INTO clientes 
            (Nome, Email, Telefone, DataNascimento, CPF, Endereco, DataCadastro, DataUltimaCompra, Ativo) 
            VALUES 
            (@Nome, @Email, @Telefone, @DataNascimento, @CPF, @Endereco, @DataCadastro, @DataUltimaCompra, @Ativo)";
            return await _connection.ExecuteAsync(query, cliente);
        }

        public async Task<int> UpdateAsync(ClienteModel cliente)
        {
            const string query =
                @"UPDATE clientes SET 
            Nome = @Nome,
            Email = @Email,
            Telefone = @Telefone,
            DataNascimento = @DataNascimento,
            CPF = @CPF,
            Endereco = @Endereco,
            DataCadastro = @DataCadastro,
            DataUltimaCompra = @DataUltimaCompra,
            Ativo = @Ativo
            WHERE CodCliente = @CodCliente";
            return await _connection.ExecuteAsync(query, cliente);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM clientes WHERE CodCliente = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
