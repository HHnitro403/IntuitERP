using Avalonia.Media;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    /// <summary>
    /// Model for Parcelas de Contas a Receber (Receivable Installments)
    /// Represents individual installment payments
    /// </summary>
    [Table("parcelas_receber")]
    public class ParcelaReceberModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cod_conta_receber")]
        public int CodContaReceber { get; set; }

        [Column("numero_parcela")]
        public int NumeroParcela { get; set; }

        [Column("data_vencimento")]
        public DateTime DataVencimento { get; set; }

        [Column("valor_parcela")]
        public decimal ValorParcela { get; set; }

        [Column("valor_pago")]
        public decimal ValorPago { get; set; }

        [Column("data_pagamento")]
        public DateTime? DataPagamento { get; set; }

        [Column("forma_pagamento")]
        public string? FormaPagamento { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Vencido, Cancelado

        [Column("juros")]
        public decimal Juros { get; set; }

        [Column("multa")]
        public decimal Multa { get; set; }

        [Column("desconto")]
        public decimal Desconto { get; set; }

        // ValorTotal is calculated: valor_parcela + juros + multa - desconto
        [Column("valor_total")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ValorTotal { get; set; }

        [Column("observacoes")]
        public string? Observacoes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Calculated properties
        [NotMapped]
        public bool IsVencida
        {
            get
            {
                if (Status == "Pago" || Status == "Cancelado")
                    return false;

                return DateTime.Today > DataVencimento;
            }
        }

        [NotMapped]
        public int DiasAtraso
        {
            get
            {
                if (!IsVencida)
                    return 0;

                return (DateTime.Today - DataVencimento).Days;
            }
        }

        [NotMapped]
        public decimal ValorRestante => ValorTotal - ValorPago;

        public bool IsPago => Status == "Pago" && ValorPago >= ValorTotal;
        public bool IsPendente => Status == "Pendente";
        public bool IsCancelado => Status == "Cancelado";

        /// <summary>
        /// Gets color for status display
        /// </summary>
        public Color GetStatusColor()
        {
            if (IsPago)
                return Colors.Green;

            if (IsCancelado)
                return Colors.DarkGray;

            if (IsVencida)
                return Colors.Red;

            return Colors.Gray; // Pendente
        }

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
            if (CodContaReceber <= 0)
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
