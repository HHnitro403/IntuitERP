using Dapper;
using IntuitERP.models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class UsuarioService
    {
        private readonly IDbConnection _connection;

        public UsuarioService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<UsuarioModel>> GetAllAsync()
        {
            const string query = "SELECT * FROM usuarios";
            return await _connection.QueryAsync<UsuarioModel>(query);
        }

        public async Task<UsuarioModel> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM usuarios WHERE CodUsuarios = @Id";
            return await _connection.QueryFirstOrDefaultAsync<UsuarioModel>(query, new { Id = id });
        }

        public async Task<UsuarioModel> GetByUsuarioAsync(string usuario)
        {
            const string query = "SELECT * FROM usuarios WHERE Usuario = @Usuario";
            return await _connection.QueryFirstOrDefaultAsync<UsuarioModel>(query, new { Usuario = usuario });
        }

        public async Task<int> InsertAsync(UsuarioModel usuario)
        {
            const string query =
                @"INSERT INTO usuarios 
                (Usuario, Senha, PermissaoProdutosCreate, PermissaoProdutosRead, 
                PermissaoProdutosUpdate, PermissaoProdutosDelete, PermissaoVendasCreate, 
                PermissaoVendasRead, PermissaoVendasUpdate, PermissaoVendasDelete, 
                PermissaoRelatoriosGenerate, PermissaoVendedoresCreate, PermissaoVendedoresRead, 
                PermissaoVendedoresUpdate, PermissaoVendedoresDelete, PermissaoFornecedoresCreate, 
                PermissaoFornecedoresRead, PermissaoFornecedoresUpdate, PermissaoFornecedoresDelete, 
                PermissaoClientesCreate, PermissaoClientesRead, PermissaoClientesUpdate, 
                PermissaoClientesDelete) 
                VALUES 
                (@Usuario, @Senha, @PermissaoProdutosCreate, @PermissaoProdutosRead, 
                @PermissaoProdutosUpdate, @PermissaoProdutosDelete, @PermissaoVendasCreate, 
                @PermissaoVendasRead, @PermissaoVendasUpdate, @PermissaoVendasDelete, 
                @PermissaoRelatoriosGenerate, @PermissaoVendedoresCreate, @PermissaoVendedoresRead, 
                @PermissaoVendedoresUpdate, @PermissaoVendedoresDelete, @PermissaoFornecedoresCreate, 
                @PermissaoFornecedoresRead, @PermissaoFornecedoresUpdate, @PermissaoFornecedoresDelete, 
                @PermissaoClientesCreate, @PermissaoClientesRead, @PermissaoClientesUpdate, 
                @PermissaoClientesDelete);
                SELECT LAST_INSERT_ID();";

            return await _connection.ExecuteScalarAsync<int>(query, usuario);
        }

        public async Task<int> UpdateAsync(UsuarioModel usuario)
        {
            const string query =
                @"UPDATE usuarios SET 
                Usuario = @Usuario, 
                Senha = @Senha, 
                PermissaoProdutosCreate = @PermissaoProdutosCreate, 
                PermissaoProdutosRead = @PermissaoProdutosRead, 
                PermissaoProdutosUpdate = @PermissaoProdutosUpdate, 
                PermissaoProdutosDelete = @PermissaoProdutosDelete, 
                PermissaoVendasCreate = @PermissaoVendasCreate, 
                PermissaoVendasRead = @PermissaoVendasRead, 
                PermissaoVendasUpdate = @PermissaoVendasUpdate, 
                PermissaoVendasDelete = @PermissaoVendasDelete, 
                PermissaoRelatoriosGenerate = @PermissaoRelatoriosGenerate, 
                PermissaoVendedoresCreate = @PermissaoVendedoresCreate, 
                PermissaoVendedoresRead = @PermissaoVendedoresRead, 
                PermissaoVendedoresUpdate = @PermissaoVendedoresUpdate, 
                PermissaoVendedoresDelete = @PermissaoVendedoresDelete, 
                PermissaoFornecedoresCreate = @PermissaoFornecedoresCreate, 
                PermissaoFornecedoresRead = @PermissaoFornecedoresRead, 
                PermissaoFornecedoresUpdate = @PermissaoFornecedoresUpdate, 
                PermissaoFornecedoresDelete = @PermissaoFornecedoresDelete, 
                PermissaoClientesCreate = @PermissaoClientesCreate, 
                PermissaoClientesRead = @PermissaoClientesRead, 
                PermissaoClientesUpdate = @PermissaoClientesUpdate, 
                PermissaoClientesDelete = @PermissaoClientesDelete 
                WHERE CodUsuarios = @CodUsuarios";

            return await _connection.ExecuteAsync(query, usuario);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM usuarios WHERE CodUsuarios = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<UsuarioModel> AuthenticateAsync(string usuario, string senha)
        {
            const string query = "SELECT * FROM usuarios WHERE Usuario = @Usuario AND Senha = @Senha";
            return await _connection.QueryFirstOrDefaultAsync<UsuarioModel>(query,
                new { Usuario = usuario, Senha = senha });
        }
    }
}
