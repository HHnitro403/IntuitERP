using Avalonia.Media;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    /// <summary>
    /// Model for Contas a Receber (Accounts Receivable)
    /// Represents money owed TO the business BY customers
    /// </summary>
    [Table("contas_receber")]
    public class ContaReceberModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cod_venda")]
        public int CodVenda { get; set; }

        [Column("cod_cliente")]
        public int CodCliente { get; set; }

        [Column("data_emissao")]
        public DateTime DataEmissao { get; set; }

        [Column("valor_total")]
        public decimal ValorTotal { get; set; }

        [Column("valor_pago")]
        public decimal ValorPago { get; set; }

        [Column("valor_pendente")]
        public decimal ValorPendente { get; set; }

        [Column("num_parcelas")]
        public int NumParcelas { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Pendente"; // Pendente, Parcial, Pago, Vencido, Cancelado

        [Column("observacoes")]
        public string? Observacoes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties (populated by joins)
        [NotMapped]
        public string? ClienteNome { get; set; }
        [NotMapped]
        public string? ClienteCpf { get; set; }

        // Calculated properties
        [NotMapped]
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
        /// Gets color for status display
        /// </summary>
        public Color GetStatusColor()
        {
            return Status switch
            {
                "Pago" => Colors.Green,
                "Parcial" => Colors.Orange,
                "Vencido" => Colors.Red,
                "Cancelado" => Colors.DarkGray,
                _ => Colors.Gray // Pendente
            };
        }

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
            if (CodVenda <= 0)
            {
                errorMessage = "Venda inválida";
                return false;
            }

            if (CodCliente <= 0)
            {
                errorMessage = "Cliente inválido";
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
