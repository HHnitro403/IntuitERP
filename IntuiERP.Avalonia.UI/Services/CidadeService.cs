using Dapper;
using IntuiERP.Avalonia.UI.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
{
    public class CidadeService
    {
        private readonly IDbConnection _connection;

        public CidadeService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<CidadeModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM cidade";
            return await _connection.QueryAsync<CidadeModel>(query);
        }

        public async Task<CidadeModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM cidade WHERE cod_cidade = @Id";
            return await _connection.QueryFirstOrDefaultAsync<CidadeModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(CidadeModel cidade)
        {
            const string query =
                @"INSERT INTO cidade 
                (cidade, uf) 
                VALUES 
                (@Cidade, @UF) RETURNING cod_cidade;";
            return await _connection.ExecuteScalarAsync<int>(query, cidade);
        }

        public async Task<int> UpdateAsync(CidadeModel cidade)
        {
            const string query =
                @"UPDATE cidade SET 
                cidade = @Cidade, 
                uf = @UF 
                WHERE cod_cidade = @CodCIdade";
            return await _connection.ExecuteAsync(query, cidade);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM cidade WHERE cod_cidade = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
