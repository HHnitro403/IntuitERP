using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuitERP.models
{
    [Table("compra")]
    public class CompraModel
    {
        [Key]
        [Column("CodCompra")]
        public int CodCompra { get; set; }

        [Column("data_compra")]
        public DateTime? DataCompra { get; set; }

        [Column("hora_compra")]
        public TimeSpan? HoraCompra { get; set; }

        [Column("CodFornec")]
        public int? CodFornec { get; set; }

        [Column("Desconto")]
        public decimal? Desconto { get; set; }

        [Column("CodVendedor")]
        public int? CodVendedor { get; set; }

        [Column("OBS")]
        public string? OBS { get; set; }

        [Column("valor_total")]
        public decimal? ValorTotal { get; set; }

        [Column("forma_pagamento")]
        [StringLength(50)]
        public string? FormaPagamento { get; set; }

        [Column("status_compra")]
        public byte? StatusCompra { get; set; }

        // Navigation properties
        [ForeignKey("CodFornec")]
        public virtual FornecedorModel? Fornecedor { get; set; }

        [ForeignKey("CodVendedor")]
        public virtual VendedorModel? Vendedor { get; set; }

        public virtual ICollection<ItemCompraModel>? ItensCompra { get; set; }
    }
}
