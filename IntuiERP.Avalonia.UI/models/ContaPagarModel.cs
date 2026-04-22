using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace IntuitERP.models
{
    /// <summary>
    /// Model for Contas a Pagar (Accounts Payable)
    /// Represents money owed BY the business TO suppliers
    /// </summary>
    public class ContaPagarModel
    {
        public int Id { get; set; }
        public int CodCompra { get; set; }
        public int CodFornecedor { get; set; }
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorPago { get; set; }
        public decimal ValorPendente { get; set; }
        public int NumParcelas { get; set; }
        public string Status { get; set; } = "Pendente"; // Pendente, Parcial, Pago, Vencido, Cancelado
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties (populated by joins)
        public string? FornecedorNome { get; set; }
        public string? FornecedorCnpj { get; set; }

        // Calculated properties
        public decimal PercentualPago => ValorTotal > 0 ? (ValorPago / ValorTotal) * 100 : 0;

        public int DiasVencido
        {
            get
            {
                if (Status == "Vencido")
                {
                    // This would be calculated based on oldest overdue parcela
                    // For now, return 0 - will be set by service layer
                    return 0;
                }
                return 0;
            }
        }

        public bool IsVencida => Status == "Vencido";
        public bool IsPago => Status == "Pago";
        public bool IsParcial => Status == "Parcial";
        public bool IsPendente => Status == "Pendente";
        public bool IsCancelado => Status == "Cancelado";

        /// <summary>
        /// Gets display text for status
        /// </summary>
        public string GetStatusDisplay()
        {
            return Status switch
            {
                "Pago" => "✓ Pago",
                "Parcial" => "⏳ Parcial",
                "Vencido" => "⚠ Vencido",
                "Cancelado" => "✗ Cancelado",
                _ => "○ Pendente"
            };
        }

        /// <summary>
        /// Validates the model
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (CodCompra <= 0)
            {
                errorMessage = "Compra inválida";
                return false;
            }

            if (CodFornecedor <= 0)
            {
                errorMessage = "Fornecedor inválido";
                return false;
            }

            if (ValorTotal <= 0)
            {
                errorMessage = "Valor total deve ser maior que zero";
                return false;
            }

            if (NumParcelas <= 0)
            {
                errorMessage = "Número de parcelas deve ser maior que zero";
                return false;
            }

            if (ValorPago < 0)
            {
                errorMessage = "Valor pago não pode ser negativo";
                return false;
            }

            if (ValorPendente < 0)
            {
                errorMessage = "Valor pendente não pode ser negativo";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}

