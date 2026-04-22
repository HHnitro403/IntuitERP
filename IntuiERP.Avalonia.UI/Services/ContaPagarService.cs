using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using IntuitERP.models;
using Dapper;
using System.Data;

namespace IntuitERP.Services
{
    public class ContaPagarService
    {
        private readonly IDbConnection _connection;

        public ContaPagarService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<ContaPagarModel>> GetAllAsync()
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        ORDER BY cp.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaPagarModel>(sql);
            return contas.ToList();
        }

        public async Task<ContaPagarModel?> GetByIdAsync(int id)
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        WHERE cp.id = @Id";

            return await _connection.QueryFirstOrDefaultAsync<ContaPagarModel>(sql, new { Id = id });
        }

        public async Task<ContaPagarModel?> GetByCompraAsync(int codCompra)
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        WHERE cp.cod_compra = @CodCompra";

            return await _connection.QueryFirstOrDefaultAsync<ContaPagarModel>(sql, new { CodCompra = codCompra });
        }

        public async Task<List<ContaPagarModel>> GetByFornecedorAsync(int codFornecedor)
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        WHERE cp.cod_fornecedor = @CodFornecedor
                        ORDER BY cp.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaPagarModel>(sql, new { CodFornecedor = codFornecedor });
            return contas.ToList();
        }

        public async Task<List<ContaPagarModel>> GetByStatusAsync(string status)
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        WHERE cp.status = @Status
                        ORDER BY cp.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaPagarModel>(sql, new { Status = status });
            return contas.ToList();
        }

        public async Task<List<ContaPagarModel>> GetVencidasAsync()
        {
            return await GetByStatusAsync("Vencido");
        }

        public async Task<List<ContaPagarModel>> GetByDateRangeAsync(DateTime dataInicio, DateTime dataFim)
        {
            var sql = @"SELECT cp.*,
                               f.razao_social as FornecedorNome,
                               f.cnpj as FornecedorCnpj
                        FROM contas_pagar cp
                        INNER JOIN fornecedor f ON cp.cod_fornecedor = f.CodFornecedor
                        WHERE cp.data_emissao BETWEEN @DataInicio AND @DataFim
                        ORDER BY cp.data_emissao DESC";

            var contas = await _connection.QueryAsync<ContaPagarModel>(sql,
                new { DataInicio = dataInicio, DataFim = dataFim });
            return contas.ToList();
        }

        public async Task<int> InsertAsync(ContaPagarModel conta)
        {
            if (!conta.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Check if conta already exists for this compra
            var existing = await GetByCompraAsync(conta.CodCompra);
            if (existing != null)
            {
                throw new InvalidOperationException($"Já existe uma conta a pagar para a compra #{conta.CodCompra}");
            }

            var sql = @"INSERT INTO contas_pagar
                        (cod_compra, cod_fornecedor, data_emissao, valor_total, valor_pago,
                         valor_pendente, num_parcelas, status, observacoes)
                        VALUES
                        (@CodCompra, @CodFornecedor, @DataEmissao, @ValorTotal, @ValorPago,
                         @ValorPendente, @NumParcelas, @Status, @Observacoes);
                        SELECT LAST_INSERT_ID();";

            return await _connection.ExecuteScalarAsync<int>(sql, conta);
        }

        public async Task<int> UpdateAsync(ContaPagarModel conta)
        {
            if (!conta.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            var sql = @"UPDATE contas_pagar
                        SET cod_compra = @CodCompra,
                            cod_fornecedor = @CodFornecedor,
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
                "SELECT COUNT(*) FROM parcelas_pagar WHERE cod_conta_pagar = @Id AND status = 'Pago'",
                new { Id = id });

            if (parcelasPagas > 0)
            {
                throw new InvalidOperationException("Não é possível excluir uma conta com parcelas pagas");
            }

            var sql = "DELETE FROM contas_pagar WHERE id = @Id";
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
                FROM parcelas_pagar
                WHERE cod_conta_pagar = @Id";

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
                UPDATE contas_pagar
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
                        FROM contas_pagar
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
        /// Creates conta from compra
        /// </summary>
        public async Task<ContaPagarModel> CreateFromCompraAsync(CompraModel compra)
        {
            if (compra.status_compra != 2) // Not Concluída
            {
                throw new InvalidOperationException("Apenas compras concluídas podem gerar contas a pagar");
            }

            var conta = new ContaPagarModel
            {
                CodCompra = compra.CodCompra,
                CodFornecedor = compra.CodFornec ?? 0,
                DataEmissao = compra.data_compra ?? DateTime.Today,
                ValorTotal = compra.valor_total ?? 0,
                ValorPago = 0,
                ValorPendente = compra.valor_total ?? 0,
                NumParcelas = 1,
                Status = "Pendente",
                Observacoes = $"Gerado automaticamente da compra #{compra.CodCompra}"
            };

            return conta;
        }
    }
}

