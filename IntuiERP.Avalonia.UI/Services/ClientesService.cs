
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace IntuiERP.Avalonia.UI.Services
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
            const string query = "SELECT * FROM cliente WHERE cod_cliente = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(ClienteModel cliente)
        {
            const string query =
                @"INSERT INTO cliente 
                (cod_cidade, nome, email, telefone, data_nascimento, cpf, endereco, 
                 numero, bairro, cep, data_cadastro, data_ultima_compra, ativo) 
                VALUES 
                (@CodCidade, @Nome, @Email, @Telefone, @DataNascimento, @CPF, @Endereco,
                 @Numero, @Bairro, @CEP, @DataCadastro, @DataUltimaCompra, @Ativo) RETURNING cod_cliente;";

            if (cliente.DataCadastro == null)
                cliente.DataCadastro = DateTime.Now;

            return await _connection.ExecuteScalarAsync<int>(query, cliente);
        }

        public async Task<int> UpdateAsync(ClienteModel cliente)
        {
            const string query =
                @"UPDATE cliente SET 
                cod_cidade = @CodCidade,
                nome = @Nome, 
                email = @Email, 
                telefone = @Telefone, 
                data_nascimento = @DataNascimento, 
                cpf = @CPF, 
                endereco = @Endereco,
                numero = @Numero,
                bairro = @Bairro,
                cep = @CEP,
                data_ultima_compra = @DataUltimaCompra, 
                ativo = @Ativo 
                WHERE cod_cliente = @CodCliente";
            return await _connection.ExecuteAsync(query, cliente);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM cliente WHERE cod_cliente = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ClienteModel> GetByEmailAsync(string email)
        {
            const string query = "SELECT * FROM cliente WHERE email = @Email";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { Email = email });
        }

        public async Task<ClienteModel> GetByCPFAsync(string cpf)
        {
            const string query = "SELECT * FROM cliente WHERE cpf = @CPF";
            return await _connection.QueryFirstOrDefaultAsync<ClienteModel>(query, new { CPF = cpf });
        }

        public async Task<IEnumerable<ClienteModel>> SearchAsync(string searchTerm)
        {
            const string query =
                @"SELECT * FROM cliente 
                WHERE nome LIKE @SearchTerm 
                OR email LIKE @SearchTerm 
                OR cpf LIKE @SearchTerm";
            return await _connection.QueryAsync<ClienteModel>(query, new { SearchTerm = $"%{searchTerm}%" });
        }
    }
}
