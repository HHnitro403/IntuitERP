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

            const string query = "SELECT * FROM usuarios WHERE cod_usuarios = @Id";
            return await connection.QueryFirstOrDefaultAsync<UsuarioModel>(query, new { Id = id });
        }

        public async Task<UsuarioModel> GetByUsuarioAsync(string usuario)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "SELECT * FROM usuarios WHERE usuario = @Usuario";
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
                (usuario, senha, permissao_produtos_create, permissao_produtos_read,
                permissao_produtos_update, permissao_produtos_delete, permissao_vendas_create,
                permissao_vendas_read, permissao_vendas_update, permissao_vendas_delete,
                permissao_relatorios_generate, permissao_vendedores_create, permissao_vendedores_read,
                permissao_vendedores_update, permissao_vendedores_delete, permissao_fornecedores_create,
                permissao_fornecedores_read, permissao_fornecedores_update, permissao_fornecedores_delete,
                permissao_clientes_create, permissao_clientes_read, permissao_clientes_update,
                permissao_clientes_delete)
                VALUES
                (@Usuario, @Senha, @PermissaoProdutosCreate, @PermissaoProdutosRead,
                @PermissaoProdutosUpdate, @PermissaoProdutosDelete, @PermissaoVendasCreate,
                @PermissaoVendasRead, @PermissaoVendasUpdate, @PermissaoVendasDelete,
                @PermissaoRelatoriosGenerate, @PermissaoVendedoresCreate, @PermissaoVendedoresRead,
                @PermissaoVendedoresUpdate, @PermissaoVendedoresDelete, @PermissaoFornecedoresCreate,
                @PermissaoFornecedoresRead, @PermissaoFornecedoresUpdate, @PermissaoFornecedoresDelete,
                @PermissaoClientesCreate, @PermissaoClientesRead, @PermissaoClientesUpdate,
                @PermissaoClientesDelete) RETURNING cod_usuarios;";

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
                usuario = @Usuario,
                senha = @Senha,
                permissao_produtos_create = @PermissaoProdutosCreate,
                permissao_produtos_read = @PermissaoProdutosRead,
                permissao_produtos_update = @PermissaoProdutosUpdate,
                permissao_produtos_delete = @PermissaoProdutosDelete,
                permissao_vendas_create = @PermissaoVendasCreate,
                permissao_vendas_read = @PermissaoVendasRead,
                permissao_vendas_update = @PermissaoVendasUpdate,
                permissao_vendas_delete = @PermissaoVendasDelete,
                permissao_relatorios_generate = @PermissaoRelatoriosGenerate,
                permissao_vendedores_create = @PermissaoVendedoresCreate,
                permissao_vendedores_read = @PermissaoVendedoresRead,
                permissao_vendedores_update = @PermissaoVendedoresUpdate,
                permissao_vendedores_delete = @PermissaoVendedoresDelete,
                permissao_fornecedores_create = @PermissaoFornecedoresCreate,
                permissao_fornecedores_read = @PermissaoFornecedoresRead,
                permissao_fornecedores_update = @PermissaoFornecedoresUpdate,
                permissao_fornecedores_delete = @PermissaoFornecedoresDelete,
                permissao_clientes_create = @PermissaoClientesCreate,
                permissao_clientes_read = @PermissaoClientesRead,
                permissao_clientes_update = @PermissaoClientesUpdate,
                permissao_clientes_delete = @PermissaoClientesDelete
                WHERE cod_usuarios = @CodUsuarios";

            return await connection.ExecuteAsync(query, usuario);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "DELETE FROM usuarios WHERE cod_usuarios = @Id";
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<UsuarioModel?> AuthenticateAsync(string usuario, string senha)
        {
            using var connection = CreateConnection();
            if (connection is DbConnection dbConn) await dbConn.OpenAsync();

            const string query = "SELECT * FROM usuarios WHERE usuario = @Usuario";
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
