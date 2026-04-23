using Dapper;
using IntuiERP.Avalonia.UI.models;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Services
{
    public class UsuarioService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly PasswordHashingService _passwordHashingService;

        public UsuarioService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _passwordHashingService = new PasswordHashingService();
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        public async Task<IEnumerable<UsuarioModel>> GetAllAsync()
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();
            
            const string query = "SELECT * FROM usuarios";
            return await connection.QueryAsync<UsuarioModel>(query);
        }

        public async Task<UsuarioModel> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "SELECT * FROM usuarios WHERE CodUsuarios = @Id";
            return await connection.QueryFirstOrDefaultAsync<UsuarioModel>(query, new { Id = id });
        }

        public async Task<UsuarioModel> GetByUsuarioAsync(string usuario)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "SELECT * FROM usuarios WHERE Usuario = @Usuario";
            return await connection.QueryFirstOrDefaultAsync<UsuarioModel>(query, new { Usuario = usuario });
        }

        public async Task<int> InsertAsync(UsuarioModel usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                if (!_passwordHashingService.IsPasswordHashed(usuario.Senha))
                {
                    usuario.Senha = _passwordHashingService.HashPassword(usuario.Senha);
                }
            }

            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

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
                @PermissaoClientesDelete) RETURNING CodUsuarios;";

            return await connection.ExecuteScalarAsync<int>(query, usuario);
        }

        public async Task<int> UpdateAsync(UsuarioModel usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                if (!_passwordHashingService.IsPasswordHashed(usuario.Senha))
                {
                    usuario.Senha = _passwordHashingService.HashPassword(usuario.Senha);
                }
            }

            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

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

            return await connection.ExecuteAsync(query, usuario);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "DELETE FROM usuarios WHERE CodUsuarios = @Id";
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<UsuarioModel?> AuthenticateAsync(string usuario, string senha)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "SELECT * FROM usuarios WHERE Usuario = @Usuario";
            var user = await connection.QueryFirstOrDefaultAsync<UsuarioModel>(query,
                new { Usuario = usuario });

            if (user == null) return null;

            bool isPasswordValid = false;

            if (_passwordHashingService.IsPasswordHashed(user.Senha))
            {
                isPasswordValid = _passwordHashingService.VerifyPassword(senha, user.Senha);

                if (isPasswordValid && _passwordHashingService.NeedsRehash(user.Senha))
                {
                    user.Senha = _passwordHashingService.HashPassword(senha);
                    await UpdateAsync(user);
                }
            }
            else
            {
                isPasswordValid = user.Senha == senha;

                if (isPasswordValid)
                {
                    user.Senha = _passwordHashingService.HashPassword(senha);
                    await UpdateAsync(user);
                }
            }

            return isPasswordValid ? user : null;
        }
    }
}
