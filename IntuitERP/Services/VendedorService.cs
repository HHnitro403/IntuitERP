using Dapper;
using IntuitERP.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class VendedorService
    {
        private readonly IDbConnection _connection;

        public VendedorService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<VendedorModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM vendedor";
            return await _connection.QueryAsync<VendedorModel>(query);
        }

        public async Task<VendedorModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM vendedor WHERE CodVendedor = @Id";
            return await _connection.QueryFirstOrDefaultAsync<VendedorModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(VendedorModel vendedor)
        {
            const string query =
                @"INSERT INTO vendedor 
                (NomeVendedor, totalvendas, vendasfinalizadas, vendascanceladas) 
                VALUES 
                (@NomeVendedor, @totalvendas, @vendasfinalizadas, @vendascanceladas);
                SELECT LAST_INSERT_ID();";

            if (vendedor.totalvendas == null)
                vendedor.totalvendas = 0;
            if (vendedor.vendasfinalizadas == null)
                vendedor.vendasfinalizadas = 0;
            if (vendedor.vendascanceladas == null)
                vendedor.vendascanceladas = 0;

            return await _connection.ExecuteScalarAsync<int>(query, vendedor);
        }

        public async Task<int> UpdateAsync(VendedorModel vendedor)
        {
            const string query =
                @"UPDATE vendedor SET 
                NomeVendedor = @NomeVendedor,                 
                WHERE CodVendedor = @CodVendedor";
            return await _connection.ExecuteAsync(query, vendedor);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM vendedor WHERE CodVendedor = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task IncrementVendasAsync(int vendedorId)
        {
            const string query =
                @"UPDATE vendedor SET 
                totalvendas = totalvendas + 1 
                WHERE CodVendedor = @VendedorId";
            await _connection.ExecuteAsync(query, new { VendedorId = vendedorId });
        }

        public async Task IncrementVendasFinalizadasAsync(int vendedorId)
        {
            const string query =
                @"UPDATE vendedor SET 
                vendasfinalizadas = vendasfinalizadas + 1 
                WHERE CodVendedor = @VendedorId";
            await _connection.ExecuteAsync(query, new { VendedorId = vendedorId });
        }

        public async Task IncrementVendasCanceladasAsync(int vendedorId)
        {
            const string query =
                @"UPDATE vendedor SET 
                vendascanceladas = vendascanceladas + 1 
                WHERE CodVendedor = @VendedorId";
            await _connection.ExecuteAsync(query, new { VendedorId = vendedorId });
        }
    }
}
