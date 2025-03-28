using Dapper;
using IntuitERP.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class UsuarioService
    {
        private readonly IDbConnection _connection;

        // Constructor that receives the database connection
        public UsuarioService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        // Get all users
        public async Task<IEnumerable<UsuarioModel>> GetAllUsuariosAsync()
        {
            const string sql = "SELECT * FROM Usuarios";
            return await _connection.QueryAsync<UsuarioModel>(sql);
        }

        // Get user by ID
        public async Task<UsuarioModel> GetUsuarioByIdAsync(int codUsuario)
        {
            const string sql = "SELECT * FROM Usuarios WHERE CodUsuarios = @CodUsuarios";
            return await _connection.QuerySingleOrDefaultAsync<UsuarioModel>(sql, new { CodUsuarios = codUsuario });
        }

        // Get user by username
        public async Task<UsuarioModel> GetUsuarioByUsernameAsync(string username)
        {
            const string sql = "SELECT * FROM Usuarios WHERE Usuario = @Usuario";
            return await _connection.QuerySingleOrDefaultAsync<UsuarioModel>(sql, new { Usuario = username });
        }

        // Create new user
        public async Task<int> CreateUsuarioAsync(UsuarioModel usuario)
        {
            const string sql = @"
            INSERT INTO Usuarios (
                Usuario, Senha, 
                PermissaoProdutosCreate, PermissaoProdutosRead, PermissaoProdutosUpdate, PermissaoProdutosDelete,
                PermissaoVendasCreate, PermissaoVendasRead, PermissaoVendasUpdate, PermissaoVendasDelete,
                PermissaoRelatoriosGenerate,
                PermissaoVendedoresCreate, PermissaoVendedoresRead, PermissaoVendedoresUpdate, PermissaoVendedoresDelete,
                PermissaoFornecedoresCreate, PermissaoFornecedoresRead, PermissaoFornecedoresUpdate, PermissaoFornecedoresDelete,
                PermissaoClientesCreate, PermissaoClientesRead, PermissaoClientesUpdate, PermissaoClientesDelete
            ) VALUES (
                @Usuario, @Senha, 
                @PermissaoProdutosCreate, @PermissaoProdutosRead, @PermissaoProdutosUpdate, @PermissaoProdutosDelete,
                @PermissaoVendasCreate, @PermissaoVendasRead, @PermissaoVendasUpdate, @PermissaoVendasDelete,
                @PermissaoRelatoriosGenerate,
                @PermissaoVendedoresCreate, @PermissaoVendedoresRead, @PermissaoVendedoresUpdate, @PermissaoVendedoresDelete,
                @PermissaoFornecedoresCreate, @PermissaoFornecedoresRead, @PermissaoFornecedoresUpdate, @PermissaoFornecedoresDelete,
                @PermissaoClientesCreate, @PermissaoClientesRead, @PermissaoClientesUpdate, @PermissaoClientesDelete
            );
            SELECT LAST_INSERT_ID();";

            return await _connection.ExecuteScalarAsync<int>(sql, usuario);
        }

        // Update existing user
        public async Task<bool> UpdateUsuarioAsync(UsuarioModel usuario)
        {
            const string sql = @"
            UPDATE Usuarios SET 
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

            int rowsAffected = await _connection.ExecuteAsync(sql, usuario);
            return rowsAffected > 0;
        }

        // Delete user
        public async Task<bool> DeleteUsuarioAsync(int codUsuario)
        {
            const string sql = "DELETE FROM Usuarios WHERE CodUsuarios = @CodUsuarios";
            int rowsAffected = await _connection.ExecuteAsync(sql, new { CodUsuarios = codUsuario });
            return rowsAffected > 0;
        }

        // Authenticate user
        public async Task<UsuarioModel> AuthenticateAsync(string username, string password)
        {
            const string sql = "SELECT * FROM usuarios WHERE Usuario = @Usuario AND Senha = @Senha";
            return await _connection.QuerySingleOrDefaultAsync<UsuarioModel>(sql, new { Usuario = username, Senha = password });
        }

        // Update specific permission
        public async Task<bool> UpdatePermissionAsync(int codUsuario, string permissionName, bool value)
        {
            string sql = $"UPDATE Usuarios SET {permissionName} = @Value WHERE CodUsuarios = @CodUsuarios";
            int rowsAffected = await _connection.ExecuteAsync(sql, new { Value = value, CodUsuarios = codUsuario });
            return rowsAffected > 0;
        }
    }
}
