using Dapper;
using IntuiERP.Avalonia.UI.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
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
            const string query = "SELECT * FROM vendedor WHERE cod_vendedor = @Id";
            return await _connection.QueryFirstOrDefaultAsync<VendedorModel>(query, new { Id = id });
        }

        public async Task<int> InsertAsync(VendedorModel vendedor)
        {
            const string query =
                @"INSERT INTO vendedor 
                (nome_vendedor, comissao, ativo, qtd_vendas, qtd_vendas_finalizadas) 
                VALUES 
                (@NomeVendedor, @Comissao, @Ativo, @QtdVendas, @QtdVendasFinalizadas) RETURNING cod_vendedor;";

            if (vendedor.QtdVendas == null)
                vendedor.QtdVendas = 0;
            if (vendedor.QtdVendasFinalizadas == null)
                vendedor.QtdVendasFinalizadas = 0;

            return await _connection.ExecuteScalarAsync<int>(query, vendedor);
        }

        public async Task<int> UpdateAsync(VendedorModel vendedor)
        {
            const string query =
                @"UPDATE vendedor SET 
                nome_vendedor = @NomeVendedor,
                comissao = @Comissao,
                ativo = @Ativo,
                qtd_vendas = @QtdVendas,
                qtd_vendas_finalizadas = @QtdVendasFinalizadas
                WHERE cod_vendedor = @CodVendedor";
            return await _connection.ExecuteAsync(query, vendedor);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM vendedor WHERE cod_vendedor = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task IncrementVendasAsync(int vendedorId)
        {
            const string query =
                @"UPDATE vendedor SET 
                qtd_vendas = COALESCE(qtd_vendas, 0) + 1 
                WHERE cod_vendedor = @VendedorId";
            await _connection.ExecuteAsync(query, new { VendedorId = vendedorId });
        }

        public async Task IncrementVendasFinalizadasAsync(int vendedorId)
        {
            const string query =
                @"UPDATE vendedor SET 
                qtd_vendas_finalizadas = COALESCE(qtd_vendas_finalizadas, 0) + 1 
                WHERE cod_vendedor = @VendedorId";
            await _connection.ExecuteAsync(query, new { VendedorId = vendedorId });
        }
    }
}
