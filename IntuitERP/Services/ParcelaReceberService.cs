using IntuitERP.models;
using Dapper;
using System.Data;

namespace IntuitERP.Services
{
    public class ParcelaReceberService
    {
        private readonly IDbConnection _connection;
        private readonly ContaReceberService _contaService;

        public ParcelaReceberService(IDbConnection connection, ContaReceberService contaService)
        {
            _connection = connection;
            _contaService = contaService;
        }

        public async Task<List<ParcelaReceberModel>> GetByContaAsync(int codContaReceber)
        {
            var sql = @"SELECT * FROM parcelas_receber
                        WHERE cod_conta_receber = @CodConta
                        ORDER BY numero_parcela";

            var parcelas = await _connection.QueryAsync<ParcelaReceberModel>(sql,
                new { CodConta = codContaReceber });
            return parcelas.ToList();
        }

        public async Task<ParcelaReceberModel?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM parcelas_receber WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ParcelaReceberModel>(sql, new { Id = id });
        }

        public async Task<List<ParcelaReceberModel>> GetVencidasAsync()
        {
            var sql = @"SELECT pr.* FROM parcelas_receber pr
                        INNER JOIN contas_receber cr ON pr.cod_conta_receber = cr.id
                        WHERE pr.status IN ('Pendente', 'Vencido')
                          AND pr.data_vencimento < CURDATE()
                          AND cr.status != 'Cancelado'
                        ORDER BY pr.data_vencimento";

            var parcelas = await _connection.QueryAsync<ParcelaReceberModel>(sql);
            return parcelas.ToList();
        }

        public async Task<List<ParcelaReceberModel>> GetVencendoHojeAsync()
        {
            var sql = @"SELECT pr.* FROM parcelas_receber pr
                        INNER JOIN contas_receber cr ON pr.cod_conta_receber = cr.id
                        WHERE pr.status = 'Pendente'
                          AND pr.data_vencimento = CURDATE()
                          AND cr.status != 'Cancelado'
                        ORDER BY pr.numero_parcela";

            var parcelas = await _connection.QueryAsync<ParcelaReceberModel>(sql);
            return parcelas.ToList();
        }

        public async Task<int> InsertAsync(ParcelaReceberModel parcela)
        {
            if (!parcela.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            var sql = @"INSERT INTO parcelas_receber
                        (cod_conta_receber, numero_parcela, data_vencimento, valor_parcela,
                         valor_pago, data_pagamento, forma_pagamento, status,
                         juros, multa, desconto, observacoes)
                        VALUES
                        (@CodContaReceber, @NumeroParcela, @DataVencimento, @ValorParcela,
                         @ValorPago, @DataPagamento, @FormaPagamento, @Status,
                         @Juros, @Multa, @Desconto, @Observacoes);
                        SELECT LAST_INSERT_ID();";

            return await _connection.ExecuteScalarAsync<int>(sql, parcela);
        }

        public async Task<int> UpdateAsync(ParcelaReceberModel parcela)
        {
            if (!parcela.IsValid(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            var sql = @"UPDATE parcelas_receber
                        SET numero_parcela = @NumeroParcela,
                            data_vencimento = @DataVencimento,
                            valor_parcela = @ValorParcela,
                            valor_pago = @ValorPago,
                            data_pagamento = @DataPagamento,
                            forma_pagamento = @FormaPagamento,
                            status = @Status,
                            juros = @Juros,
                            multa = @Multa,
                            desconto = @Desconto,
                            observacoes = @Observacoes
                        WHERE id = @Id";

            return await _connection.ExecuteAsync(sql, parcela);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var parcela = await GetByIdAsync(id);
            if (parcela == null)
                throw new ArgumentException("Parcela não encontrada");

            if (parcela.IsPago)
                throw new InvalidOperationException("Não é possível excluir uma parcela paga");

            var sql = "DELETE FROM parcelas_receber WHERE id = @Id";
            return await _connection.ExecuteAsync(sql, new { Id = id });
        }

        /// <summary>
        /// Registers payment for a parcela
        /// </summary>
        public async Task RegistrarPagamentoAsync(
            int parcelaId,
            decimal valorPago,
            DateTime dataPagamento,
            string formaPagamento,
            decimal juros = 0,
            decimal multa = 0,
            decimal desconto = 0,
            string? observacoes = null)
        {
            var parcela = await GetByIdAsync(parcelaId);
            if (parcela == null)
                throw new ArgumentException("Parcela não encontrada");

            if (parcela.IsCancelado)
                throw new InvalidOperationException("Não é possível registrar pagamento em parcela cancelada");

            // Update parcela
            parcela.ValorPago += valorPago;
            parcela.DataPagamento = dataPagamento;
            parcela.FormaPagamento = formaPagamento;
            parcela.Juros = juros;
            parcela.Multa = multa;
            parcela.Desconto = desconto;

            if (!string.IsNullOrEmpty(observacoes))
            {
                parcela.Observacoes = observacoes;
            }

            // Update status
            if (parcela.ValorPago >= parcela.ValorTotal)
            {
                parcela.Status = "Pago";
            }
            else if (parcela.IsVencida)
            {
                parcela.Status = "Vencido";
            }
            else
            {
                parcela.Status = "Pendente";
            }

            await UpdateAsync(parcela);

            // Recalculate conta status
            await _contaService.RecalcularStatusAsync(parcela.CodContaReceber);
        }

        /// <summary>
        /// Cancels a parcela
        /// </summary>
        public async Task CancelarParcelaAsync(int id)
        {
            var parcela = await GetByIdAsync(id);
            if (parcela == null)
                throw new ArgumentException("Parcela não encontrada");

            if (parcela.ValorPago > 0)
                throw new InvalidOperationException("Não é possível cancelar parcela com pagamento registrado");

            parcela.Status = "Cancelado";
            await UpdateAsync(parcela);

            // Recalculate conta status
            await _contaService.RecalcularStatusAsync(parcela.CodContaReceber);
        }

        /// <summary>
        /// Updates overdue status for all pending parcelas (batch job)
        /// </summary>
        public async Task AtualizarStatusVencimentoAsync()
        {
            var sql = @"UPDATE parcelas_receber
                        SET status = 'Vencido'
                        WHERE status = 'Pendente'
                          AND data_vencimento < CURDATE()";

            await _connection.ExecuteAsync(sql);

            // Update conta status for affected accounts
            var affectedCodContas = await _connection.QueryAsync<int>(@"
                SELECT DISTINCT cod_conta_receber
                FROM parcelas_receber
                WHERE status = 'Vencido'");

            foreach (var codConta in affectedCodContas)
            {
                await _contaService.RecalcularStatusAsync(codConta);
            }
        }

        /// <summary>
        /// Calculates interest and penalty for a parcela
        /// </summary>
        public async Task<(decimal juros, decimal multa)> CalcularJurosMultaAsync(
            int parcelaId,
            decimal jurosMensalPercent,
            decimal multaPercent,
            int carenciaDias = 0)
        {
            var parcela = await GetByIdAsync(parcelaId);
            if (parcela == null)
                throw new ArgumentException("Parcela não encontrada");

            parcela.CalcularJurosMulta(jurosMensalPercent, multaPercent, carenciaDias);
            return (parcela.Juros, parcela.Multa);
        }

        /// <summary>
        /// Generates equal installments for a conta
        /// </summary>
        public List<ParcelaReceberModel> GerarParcelasIguais(
            int codContaReceber,
            decimal valorTotal,
            int numParcelas,
            DateTime primeiraData,
            int intervaloDias = 30)
        {
            if (numParcelas <= 0)
                throw new ArgumentException("Número de parcelas deve ser maior que zero");

            if (valorTotal <= 0)
                throw new ArgumentException("Valor total deve ser maior que zero");

            var parcelas = new List<ParcelaReceberModel>();
            decimal valorParcela = Math.Floor(valorTotal / numParcelas * 100) / 100; // Round down
            decimal restante = valorTotal - (valorParcela * numParcelas);

            for (int i = 1; i <= numParcelas; i++)
            {
                decimal valorDessaParcela = valorParcela;

                // Last parcela gets remainder
                if (i == numParcelas)
                {
                    valorDessaParcela += restante;
                }

                var parcela = new ParcelaReceberModel
                {
                    CodContaReceber = codContaReceber,
                    NumeroParcela = i,
                    DataVencimento = primeiraData.AddDays((i - 1) * intervaloDias),
                    ValorParcela = valorDessaParcela,
                    ValorPago = 0,
                    Status = "Pendente",
                    Juros = 0,
                    Multa = 0,
                    Desconto = 0
                };

                parcelas.Add(parcela);
            }

            return parcelas;
        }

        /// <summary>
        /// Inserts multiple parcelas in batch
        /// </summary>
        public async Task<int> InsertBatchAsync(List<ParcelaReceberModel> parcelas)
        {
            if (parcelas == null || parcelas.Count == 0)
                return 0;

            // Validate all parcelas
            foreach (var parcela in parcelas)
            {
                if (!parcela.IsValid(out string errorMessage))
                {
                    throw new ArgumentException($"Parcela {parcela.NumeroParcela}: {errorMessage}");
                }
            }

            // Validate total matches conta
            if (parcelas.Count > 0)
            {
                int codConta = parcelas[0].CodContaReceber;
                var conta = await _contaService.GetByIdAsync(codConta);

                if (conta != null)
                {
                    decimal totalParcelas = parcelas.Sum(p => p.ValorParcela);
                    if (Math.Abs(totalParcelas - conta.ValorTotal) > 0.01m) // Allow 1 cent difference
                    {
                        throw new InvalidOperationException(
                            $"Soma das parcelas (R$ {totalParcelas:N2}) não corresponde ao valor total (R$ {conta.ValorTotal:N2})");
                    }
                }
            }

            int inserted = 0;
            foreach (var parcela in parcelas)
            {
                await InsertAsync(parcela);
                inserted++;
            }

            // Update conta num_parcelas
            if (parcelas.Count > 0)
            {
                int codConta = parcelas[0].CodContaReceber;
                await _connection.ExecuteAsync(
                    "UPDATE contas_receber SET num_parcelas = @NumParcelas WHERE id = @Id",
                    new { NumParcelas = parcelas.Count, Id = codConta });
            }

            return inserted;
        }
    }
}
