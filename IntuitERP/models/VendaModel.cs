using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("venda")]
    public class VendaModel
    {
        [Key]
        [Column("CodVenda")]
        public int CodVenda { get; set; }

        [Column("data_venda")]
        public DateTime? data_venda { get; set; }

        [Column("hora_venda")]
        public TimeSpan? hora_venda { get; set; }

        [Column("CodCliente")]
        [Required]
        public int CodCliente { get; set; }

        [Column("Desconto", TypeName = "decimal(10,2)")]
        public decimal? Desconto { get; set; }

        [Column("CodVendedor")]
        public int? CodVendedor { get; set; }

        [Column("OBS")]
        public string? OBS { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        [Required]
        public decimal valor_total { get; set; }

        [Column("forma_pagamento")]
        [StringLength(50)]
        public string? forma_pagamento { get; set; }

        [Column("status_venda")]
        public byte? status_venda { get; set; }

        // Navigation properties
        [ForeignKey("CodCliente")]
        public virtual ClienteModel? Cliente { get; set; }

        [ForeignKey("CodVendedor")]
        public virtual VendedorModel? Vendedor { get; set; }

        public virtual ICollection<ItemVendaModel>? ItensVenda { get; set; }
    }

    public class VendaFilterModel
    {
        public int? CodCliente { get; set; }
        public int? CodVendedor { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public decimal? ValorTotalMinimo { get; set; }
        public decimal? ValorTotalMaximo { get; set; }
        public string? FormaPagamento { get; set; }
        public int? StatusVenda { get; set; }
    }
}
