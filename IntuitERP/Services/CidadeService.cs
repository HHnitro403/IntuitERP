using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
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
            const string query = "SELECT * FROM cidade WHERE CodCIdade = @Id";
            return await _connection.QueryFirstOrDefaultAsync<CidadeModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(CidadeModel cidade)
        {
            const string query =
                @"INSERT INTO cidade 
                (Cidade, UF) 
                VALUES 
                (@Cidade, @UF);
                SELECT LAST_INSERT_ID();";
            return await _connection.ExecuteScalarAsync<int>(query, cidade);
        }

        public async Task<int> UpdateAsync(CidadeModel cidade)
        {
            const string query =
                @"UPDATE cidade SET 
                Cidade = @Cidade, 
                UF = @UF 
                WHERE CodCIdade = @CodCIdade";
            return await _connection.ExecuteAsync(query, cidade);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM cidade WHERE CodCIdade = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
