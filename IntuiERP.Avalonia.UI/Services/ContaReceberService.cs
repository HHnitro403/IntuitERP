using IntuiERP.Avalonia.UI.models;
using Dapper;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntuiERP.Avalonia.UI.Services
{
    public class ContaReceberService
    {
        private readonly IDbConnection _connection;

        public ContaReceberService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<ContaReceberModel>> GetAllAsync()
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        ORDER BY cr.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaReceberModel>(sql);
            return contas.ToList();
        }

        public async Task<ContaReceberModel?> GetByIdAsync(int id)
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        WHERE cr.id = @Id";

            return await _connection.QueryFirstOrDefaultAsync<ContaReceberModel>(sql, new { Id = id });
        }

        public async Task<ContaReceberModel?> GetByVendaAsync(int codVenda)
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        WHERE cr.cod_venda = @CodVenda";

            return await _connection.QueryFirstOrDefaultAsync<ContaReceberModel>(sql, new { CodVenda = codVenda });
        }

        public async Task<List<ContaReceberModel>> GetByClienteAsync(int codCliente)
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        WHERE cr.cod_cliente = @CodCliente
                        ORDER BY cr.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaReceberModel>(sql, new { CodCliente = codCliente });
            return contas.ToList();
        }

        public async Task<List<ContaReceberModel>> GetByStatusAsync(string status)
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        WHERE cr.status = @Status
                        ORDER BY cr.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaReceberModel>(sql, new { Status = status });
            return contas.ToList();
        }

        public async Task<List<ContaReceberModel>> GetVencidasAsync()
        {
            return await GetByStatusAsync("Vencido");
        }

        public async Task<List<ContaReceberModel>> GetByDateRangeAsync(DateTime dataInicio, DateTime dataFim)
        {
            var sql = @"SELECT cr.*,
                               c.nome as ClienteNome,
                               c.cpf as ClienteCpf
                        FROM contas_receber cr
                        INNER JOIN clientes c ON cr.cod_cliente = c.CodCliente
                        WHERE cr.data_emissao BETWEEN @DataInicio AND @DataFim
                        ORDER BY cr.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaReceberModel>(sql,
                new { DataInicio = dataInicio, DataFim = dataFim });
            return contas.ToList();
        }

        public async Task<int> InsertAsync(ContaReceberModel conta)
        {
            if (!conta.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Check if conta already exists for this venda
            var existing = await GetByVendaAsync(conta.CodVenda);
            if (existing != null)
            {
                throw new InvalidOperationException($"Já existe uma conta a receber para a venda #{conta.CodVenda}");
            }

            var sql = @"INSERT INTO contas_receber
                        (cod_venda, cod_cliente, data_emissao, valor_total, valor_pago,
                         valor_pendente, num_parcelas, status, observacoes)
                        VALUES
                        (@CodVenda, @CodCliente, @DataEmissao, @ValorTotal, @ValorPago,
                         @ValorPendente, @NumParcelas, @Status, @Observacoes) RETURNING id;";

            return await _connection.ExecuteScalarAsync<int>(sql, conta);
        }

        public async Task<int> UpdateAsync(ContaReceberModel conta)
        {
            if (!conta.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            var sql = @"UPDATE contas_receber
                        SET cod_venda = @CodVenda,
                            cod_cliente = @CodCliente,
                            data_emissao = @DataEmissao,
                            valor_total = @ValorTotal,
                            valor_pago = @ValorPago,
                            valor_pendente = @ValorPendente,
                            num_parcelas = @NumParcelas,
                            status = @Status,
                            observacoes = @Observacoes
                        WHERE id = @Id";

            return await _connection.ExecuteAsync(sql, conta);
        }

        public async Task<int> DeleteAsync(int id)
        {
            // Check if any parcela is paid
            var parcelasPagas = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM parcelas_receber WHERE cod_conta_receber = @Id AND status = 'Pago'",
                new { Id = id });

            if (parcelasPagas > 0)
            {
                throw new InvalidOperationException("Não é possível excluir uma conta com parcelas pagas");
            }

            var sql = "DELETE FROM contas_receber WHERE id = @Id";
            return await _connection.ExecuteAsync(sql, new { Id = id });
        }

        /// <summary>
        /// Recalculates conta status based on parcelas
        /// Call this after any parcela payment/change
        /// </summary>
        public async Task RecalcularStatusAsync(int id)
        {
            var sql = @"
                SELECT
                    SUM(valor_pago) as TotalPago,
                    COUNT(*) as TotalParcelas,
                    SUM(CASE WHEN status = 'Vencido' THEN 1 ELSE 0 END) as ParcelasVencidas
                FROM parcelas_receber
                WHERE cod_conta_receber = @Id";

            var stats = await _connection.QueryFirstOrDefaultAsync(sql, new { Id = id });

            decimal valorPago = stats?.TotalPago ?? 0;
            int parcelasVencidas = stats?.ParcelasVencidas ?? 0;

            var conta = await GetByIdAsync(id);
            if (conta == null) return;

            string novoStatus;
            if (valorPago >= conta.ValorTotal)
            {
                novoStatus = "Pago";
            }
            else if (valorPago > 0)
            {
                novoStatus = "Parcial";
            }
            else if (parcelasVencidas > 0)
            {
                novoStatus = "Vencido";
            }
            else
            {
                novoStatus = "Pendente";
            }

            decimal valorPendente = conta.ValorTotal - valorPago;

            await _connection.ExecuteAsync(@"
                UPDATE contas_receber
                SET valor_pago = @ValorPago,
                    valor_pendente = @ValorPendente,
                    status = @Status
                WHERE id = @Id",
                new
                {
                    ValorPago = valorPago,
                    ValorPendente = valorPendente,
                    Status = novoStatus,
                    Id = id
                });
        }

        /// <summary>
        /// Gets dashboard summary (totals by status)
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetDashboardSummaryAsync()
        {
            var sql = @"SELECT status, SUM(valor_pendente) as total
                        FROM contas_receber
                        WHERE status != 'Cancelado'
                        GROUP BY status";

            var results = await _connection.QueryAsync<(string status, decimal total)>(sql);

            var summary = new Dictionary<string, decimal>
            {
                { "Pendente", 0 },
                { "Parcial", 0 },
                { "Vencido", 0 },
                { "Pago", 0 }
            };

            foreach (var (status, total) in results)
            {
                if (summary.ContainsKey(status))
                {
                    summary[status] = total;
                }
            }

            return summary;
        }

        /// <summary>
        /// Creates conta from venda
        /// </summary>
        public async Task<ContaReceberModel> CreateFromVendaAsync(VendaModel venda)
        {
            if (venda.status_venda != 2) // Not Faturada
            {
                throw new InvalidOperationException("Apenas vendas faturadas podem gerar contas a receber");
            }

            var conta = new ContaReceberModel
            {
                CodVenda = venda.CodVenda,
                CodCliente = venda.CodCliente,
                DataEmissao = venda.data_venda ?? DateTime.Today,
                ValorTotal = venda.valor_total,
                ValorPago = 0,
                ValorPendente = venda.valor_total,
                NumParcelas = 1,
                Status = "Pendente",
                Observacoes = $"Gerado automaticamente da venda #{venda.CodVenda}"
            };

            return conta;
        }
    }
}
