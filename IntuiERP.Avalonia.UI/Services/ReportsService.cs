using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntuiERP.Avalonia.UI.models.ReportModels;

namespace IntuiERP.Avalonia.UI.Services
{
    public class ReportsService
    {
        private readonly IDbConnection _connection;

        public ReportsService(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Gets data for the general sales report.
        /// Joins venda, cliente, and vendedor tables.
        /// </summary>
        public async Task<IEnumerable<VendaReportModel>> GetVendasReportAsync()
        {
            const string query = @"
                SELECT
                    v.cod_venda,
                    v.data_venda,
                    c.nome AS NomeCliente,
                    vd.nome_vendedor AS NomeVendedor,
                    v.valor_total,
                    v.forma_pagamento,
                    v.status_venda
                FROM venda v
                INNER JOIN cliente c ON v.cod_cliente = c.cod_cliente
                INNER JOIN vendedor vd ON v.cod_vendedor = vd.cod_vendedor
                ORDER BY v.cod_venda DESC;";
            return await _connection.QueryAsync<VendaReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general purchases report.
        /// Joins compra and fornecedor tables.
        /// </summary>
        public async Task<IEnumerable<CompraReportModel>> GetComprasReportAsync()
        {
            const string query = @"
                SELECT
                    c.cod_compra,
                    c.data_compra,
                    f.nome_fantasia AS NomeFornecedor,
                    c.valor_total,
                    c.status_compra
                FROM compra c
                INNER JOIN fornecedor f ON c.cod_fornecedor = f.cod_fornecedor
                ORDER BY c.cod_compra DESC;";
            return await _connection.QueryAsync<CompraReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general products report.
        /// Joins produto and fornecedor tables.
        /// </summary>
        public async Task<IEnumerable<ProdutoReportModel>> GetProdutosReportAsync()
        {
            const string query = @"
                SELECT
                    p.cod_produto,
                    p.descricao,
                    p.categoria,
                    p.preco_unitario,
                    p.saldo_est,
                    f.nome_fantasia AS NomeFornecedor
                FROM produto p
                INNER JOIN fornecedor f ON p.fornecedor_id = f.cod_fornecedor
                ORDER BY p.descricao;";
            return await _connection.QueryAsync<ProdutoReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general customers report.
        /// Joins cliente and cidade tables.
        /// </summary>
        public async Task<IEnumerable<ClienteReportModel>> GetClientesReportAsync()
        {
            const string query = @"
                SELECT
                    cl.cod_cliente,
                    cl.nome,
                    cl.email,
                    cl.telefone,
                    cl.cpf,
                    c.cidade AS NomeCidade,
                    c.uf
                FROM cliente cl
                INNER JOIN cidade c ON cl.cod_cidade = c.cod_cidade
                ORDER BY cl.nome;";
            return await _connection.QueryAsync<ClienteReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general stock report.
        /// Joins estoque and produto tables.
        /// </summary>
        public async Task<IEnumerable<EstoqueReportModel>> GetEstoqueReportAsync()
        {
            const string query = @"
                SELECT
                    e.id AS CodEst,
                    p.descricao AS DescricaoProduto,
                    e.tipo,
                    e.qtd,
                    e.data
                FROM estoque e
                INNER JOIN produto p ON e.cod_produto = p.cod_produto
                ORDER BY e.data DESC;";
            return await _connection.QueryAsync<EstoqueReportModel>(query);
        }
    }
}
