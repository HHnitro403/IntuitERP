using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("venda")]
    public class VendaModel
    {
        [Key]
        [Column("cod_venda")]
        public int CodVenda { get; set; }

        [Column("data_venda")]
        public DateTime? data_venda { get; set; }

        [Column("hora_venda")]
        public TimeSpan? hora_venda { get; set; }

        [Column("cod_cliente")]
        [Required]
        public int CodCliente { get; set; }

        [Column("desconto", TypeName = "decimal(10,2)")]
        public decimal? Desconto { get; set; }

        [Column("cod_vendedor")]
        public int? CodVendedor { get; set; }

        [Column("obs")]
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
        [ForeignKey("cod_cliente")]
        public virtual ClienteModel? Cliente { get; set; }

        [ForeignKey("cod_vendedor")]
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

    public class VendaDisplayModel
    {
        public int CodVenda { get; set; }
        public DateTime? DataVenda { get; set; }
        public decimal? ValorTotal { get; set; }
        public string? Status { get; set; }
        public string? NomeCliente { get; set; }
        public string? NomeVendedor { get; set; }
    }
}
