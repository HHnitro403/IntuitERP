using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace IntuitERP.models
{
    /// <summary>
    /// Model for Parcelas de Contas a Pagar (Payable Installments)
    /// Represents individual installment payments to suppliers
    /// </summary>
    public class ParcelaPagarModel
    {
        public int Id { get; set; }
        public int CodContaPagar { get; set; }
        public int NumeroParcela { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorParcela { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string? FormaPagamento { get; set; }
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Vencido, Cancelado
        public decimal Juros { get; set; }
        public decimal Multa { get; set; }
        public decimal Desconto { get; set; }
        // ValorTotal is calculated: valor_parcela + juros + multa - desconto
        public decimal ValorTotal => ValorParcela + Juros + Multa - Desconto;
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Calculated properties
        public bool IsVencida
        {
            get
            {
                if (Status == "Pago" || Status == "Cancelado")
                    return false;

                return DateTime.Today > DataVencimento;
            }
        }

        public int DiasAtraso
        {
            get
            {
                if (!IsVencida)
                    return 0;

                return (DateTime.Today - DataVencimento).Days;
            }
        }

        public decimal ValorRestante => ValorTotal - ValorPago;

        public bool IsPago => Status == "Pago" && ValorPago >= ValorTotal;
        public bool IsPendente => Status == "Pendente";
        public bool IsCancelado => Status == "Cancelado";

        /// <summary>
        /// Gets display text for status
        /// </summary>
        public string GetStatusDisplay()
        {
            if (IsPago)
                return "✓ Pago";

            if (IsCancelado)
                return "✗ Cancelado";

            if (IsVencida)
                return $"⚠ Vencido ({DiasAtraso}d)";

            // Calculate days until due
            int diasAteVencimento = (DataVencimento - DateTime.Today).Days;
            if (diasAteVencimento <= 3 && diasAteVencimento >= 0)
                return $"⏰ Vence em {diasAteVencimento}d";

            return "○ Pendente";
        }

        /// <summary>
        /// Calculates interest and penalty based on days overdue
        /// </summary>
        public void CalcularJurosMulta(decimal jurosMensalPercent, decimal multaPercent, int carenciaDias = 0)
        {
            if (!IsVencida || IsPago || IsCancelado)
            {
                Juros = 0;
                Multa = 0;
                return;
            }

            int diasAtrasoComCarencia = DiasAtraso - carenciaDias;
            if (diasAtrasoComCarencia <= 0)
            {
                Juros = 0;
                Multa = 0;
                return;
            }

            // Multa (one-time penalty)
            Multa = ValorParcela * (multaPercent / 100);

            // Juros (pro-rata daily interest)
            decimal jurosDiario = (jurosMensalPercent / 30) / 100;
            Juros = ValorParcela * jurosDiario * diasAtrasoComCarencia;
        }

        /// <summary>
        /// Validates the model
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (CodContaPagar <= 0)
            {
                errorMessage = "Conta inválida";
                return false;
            }

            if (NumeroParcela <= 0)
            {
                errorMessage = "Número da parcela deve ser maior que zero";
                return false;
            }

            if (ValorParcela <= 0)
            {
                errorMessage = "Valor da parcela deve ser maior que zero";
                return false;
            }

            if (ValorPago < 0)
            {
                errorMessage = "Valor pago não pode ser negativo";
                return false;
            }

            if (ValorPago > ValorTotal)
            {
                errorMessage = $"Valor pago (R$ {ValorPago:N2}) não pode ser maior que o total (R$ {ValorTotal:N2})";
                return false;
            }

            if (Juros < 0)
            {
                errorMessage = "Juros não pode ser negativo";
                return false;
            }

            if (Multa < 0)
            {
                errorMessage = "Multa não pode ser negativa";
                return false;
            }

            if (Desconto < 0)
            {
                errorMessage = "Desconto não pode ser negativo";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}

