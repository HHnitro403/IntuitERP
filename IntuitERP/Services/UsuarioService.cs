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
        private readonly PasswordHashingService _passwordHashingService;

        public UsuarioService(IDbConnection connection)
        {
            _connection = connection;
            _passwordHashingService = new PasswordHashingService();
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
            // Hash the password before storing
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                // Only hash if it's not already hashed
                if (!_passwordHashingService.IsPasswordHashed(usuario.Senha))
                {
                    usuario.Senha = _passwordHashingService.HashPassword(usuario.Senha);
                }
            }

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
            // Hash the password before storing if it's being changed
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                // Only hash if it's not already hashed (e.g., user is changing password)
                if (!_passwordHashingService.IsPasswordHashed(usuario.Senha))
                {
                    usuario.Senha = _passwordHashingService.HashPassword(usuario.Senha);
                }
            }

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

            var result = await _connection.ExecuteAsync(query, usuario);

            return result;
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string query = "DELETE FROM usuarios WHERE CodUsuarios = @Id";
            return await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<UsuarioModel?> AuthenticateAsync(string usuario, string senha)
        {
            // First, retrieve the user by username only
            const string query = "SELECT * FROM usuarios WHERE Usuario = @Usuario";
            var user = await _connection.QueryFirstOrDefaultAsync<UsuarioModel>(query,
                new { Usuario = usuario });

            if (user == null)
            {
                // User not found
                return null;
            }

            // Check if password is hashed or plain text (for migration compatibility)
            bool isPasswordValid = false;

            if (_passwordHashingService.IsPasswordHashed(user.Senha))
            {
                // Verify hashed password
                isPasswordValid = _passwordHashingService.VerifyPassword(senha, user.Senha);

                // Check if password needs rehashing (e.g., work factor changed)
                if (isPasswordValid && _passwordHashingService.NeedsRehash(user.Senha))
                {
                    // Rehash and update password
                    user.Senha = _passwordHashingService.HashPassword(senha);
                    await UpdateAsync(user);
                }
            }
            else
            {
                // MIGRATION PATH: Password is still in plain text
                // Check plain text match and then hash it
                isPasswordValid = user.Senha == senha;

                if (isPasswordValid)
                {
                    // Hash the password and update in database
                    user.Senha = _passwordHashingService.HashPassword(senha);
                    await UpdateAsync(user);
                }
            }

            return isPasswordValid ? user : null;
        }
    }
}
