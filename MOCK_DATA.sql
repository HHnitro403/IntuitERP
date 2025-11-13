-- ========================================
-- Mock Data for IntuitERP Management System
-- ========================================
-- This script provides realistic sample data for testing and demonstration
-- All data is fictional and designed for a Brazilian business context
-- ========================================

-- ========================================
-- CIDADES (Cities) - Brazilian cities
-- ========================================
INSERT INTO cidades (NomeCidade, Estado) VALUES
('São Paulo', 'SP'),
('Rio de Janeiro', 'RJ'),
('Belo Horizonte', 'MG'),
('Curitiba', 'PR'),
('Porto Alegre', 'RS'),
('Brasília', 'DF'),
('Salvador', 'BA'),
('Fortaleza', 'CE'),
('Recife', 'PE'),
('Manaus', 'AM');

-- ========================================
-- FORNECEDORES (Suppliers)
-- ========================================
INSERT INTO fornecedor (razao_social, nome_fantasia, cnpj, email, telefone, endereco, numero, bairro, CodCidade, cep, ativo) VALUES
('Tech Distribuidora Ltda', 'TechDist', '12.345.678/0001-90', 'contato@techdist.com.br', '(11) 3456-7890', 'Av. Paulista', '1000', 'Bela Vista', 1, '01310-100', 1),
('Eletrônica Brasil S.A.', 'EletronicaBR', '23.456.789/0001-01', 'vendas@eletronicabr.com.br', '(21) 2345-6789', 'Rua da Assembleia', '10', 'Centro', 2, '20011-000', 1),
('Mega Periféricos Importadora', 'MegaPeriféricos', '34.567.890/0001-12', 'comercial@megaperifericos.com.br', '(31) 3234-5678', 'Av. Afonso Pena', '500', 'Centro', 3, '30130-001', 1),
('InfoParts Distribuidora', 'InfoParts', '45.678.901/0001-23', 'info@infoparts.com.br', '(41) 3123-4567', 'Rua XV de Novembro', '200', 'Centro', 4, '80020-310', 1),
('Global Tech Comércio', 'GlobalTech', '56.789.012/0001-34', 'global@globaltech.com.br', '(51) 3012-3456', 'Av. Borges de Medeiros', '300', 'Centro Histórico', 5, '90020-021', 1);

-- ========================================
-- PRODUTOS (Products) - Technology products
-- ========================================
INSERT INTO produtos (nome, Descricao, preco_unit, SaldoEst, EstMinimo, categoria, tipo, FornecedorP_ID, Ativo) VALUES
-- Notebooks
('Notebook Dell Inspiron 15', 'Notebook Dell Inspiron 15 i5-1135G7 8GB 256GB SSD 15.6" Full HD Windows 11', 3299.00, 25, 5, 'Notebooks', 'Revenda', 1, 1),
('Notebook Lenovo IdeaPad 3', 'Notebook Lenovo IdeaPad 3 Ryzen 5 8GB 512GB SSD 15.6" Windows 11', 2899.00, 18, 5, 'Notebooks', 'Revenda', 1, 1),
('Notebook Acer Aspire 5', 'Notebook Acer Aspire 5 i7-1165G7 16GB 512GB SSD 15.6" Full HD Windows 11 Pro', 4599.00, 12, 3, 'Notebooks', 'Revenda', 2, 1),
('MacBook Air M2', 'Apple MacBook Air 13.6" M2 8GB 256GB SSD - Cinza Espacial', 9999.00, 5, 2, 'Notebooks', 'Revenda', 2, 1),

-- Peripherals
('Mouse Logitech MX Master 3', 'Mouse sem fio Logitech MX Master 3 ergonômico 4000 DPI', 499.00, 120, 20, 'Periféricos', 'Revenda', 3, 1),
('Mouse Gamer Razer DeathAdder', 'Mouse Gamer Razer DeathAdder Essential 6400 DPI RGB', 179.00, 85, 15, 'Periféricos', 'Revenda', 3, 1),
('Teclado Mecânico Keychron K2', 'Teclado Mecânico Keychron K2 Wireless RGB Hot-swappable', 699.00, 45, 10, 'Periféricos', 'Revenda', 3, 1),
('Teclado Logitech K380', 'Teclado Sem Fio Logitech K380 Multi-Device Bluetooth', 249.00, 95, 20, 'Periféricos', 'Revenda', 3, 1),
('Webcam Logitech C920', 'Webcam Logitech C920 Full HD 1080p com Microfone Estéreo', 389.00, 60, 15, 'Periféricos', 'Revenda', 3, 1),
('Headset HyperX Cloud II', 'Headset Gamer HyperX Cloud II 7.1 Surround USB', 549.00, 42, 10, 'Periféricos', 'Revenda', 3, 1),

-- Monitors
('Monitor LG 27" 4K', 'Monitor LG 27" UHD 4K IPS HDR10 27UP550', 1899.00, 15, 3, 'Monitores', 'Revenda', 2, 1),
('Monitor Samsung 24" Curvo', 'Monitor Gamer Samsung 24" Curvo 144Hz 1ms LC24F390', 899.00, 28, 5, 'Monitores', 'Revenda', 2, 1),
('Monitor Dell UltraSharp 27"', 'Monitor Dell UltraSharp U2723DE 27" QHD IPS USB-C', 2799.00, 8, 2, 'Monitores', 'Revenda', 1, 1),

-- Furniture
('Cadeira Gamer DXRacer', 'Cadeira Gamer DXRacer Racing Series ergonômica com apoio lombar', 1499.00, 8, 2, 'Móveis', 'Revenda', 4, 1),
('Mesa Gamer RGB', 'Mesa Gamer com LED RGB 120x60cm suporte para monitor e mouse pad', 599.00, 15, 3, 'Móveis', 'Revenda', 4, 1),
('Suporte Monitor Duplo', 'Suporte Articulado para 2 Monitores 17" a 32" VESA', 249.00, 35, 8, 'Móveis', 'Revenda', 4, 1),

-- Components
('SSD Kingston 1TB', 'SSD Kingston NV2 1TB M.2 2280 NVMe PCIe 4.0', 449.00, 65, 15, 'Componentes', 'Revenda', 5, 1),
('Memória RAM Corsair 16GB', 'Memória RAM Corsair Vengeance 16GB (2x8GB) DDR4 3200MHz', 389.00, 78, 20, 'Componentes', 'Revenda', 5, 1),
('Fonte Corsair 650W', 'Fonte Corsair CV650 650W 80 Plus Bronze', 449.00, 32, 8, 'Componentes', 'Revenda', 5, 1),
('Placa de Vídeo RTX 3060', 'Placa de Vídeo NVIDIA GeForce RTX 3060 12GB GDDR6', 2499.00, 6, 2, 'Componentes', 'Revenda', 5, 1),

-- Software/Services
('Windows 11 Pro', 'Microsoft Windows 11 Professional - Licença Digital', 799.00, 0, 0, 'Software', 'Serviço', 1, 1),
('Office 365 Personal', 'Microsoft Office 365 Personal - Assinatura 1 Ano', 299.00, 0, 0, 'Software', 'Serviço', 1, 1),
('Antivírus Kaspersky', 'Kaspersky Total Security - 3 Dispositivos 1 Ano', 149.00, 0, 0, 'Software', 'Serviço', 2, 1),

-- Accessories
('Hub USB-C 7 em 1', 'Hub USB-C 7 em 1 com HDMI 4K USB 3.0 SD/TF Card Reader', 189.00, 55, 12, 'Acessórios', 'Revenda', 4, 1),
('Cabo HDMI 2.1', 'Cabo HDMI 2.1 8K 60Hz 4K 120Hz 2 metros', 49.00, 150, 30, 'Acessórios', 'Revenda', 4, 1),
('Mousepad Gamer RGB', 'Mousepad Gamer RGB Grande 800x300mm impermeável', 89.00, 95, 20, 'Acessórios', 'Revenda', 3, 1);

-- ========================================
-- CLIENTES (Clients) - Mix of B2B and B2C
-- ========================================
INSERT INTO clientes (nome, email, telefone, cpf, data_nasc, endereco, numero, bairro, CodCidade, CEP) VALUES
-- Corporate clients
('TechCorp Soluções Ltda', 'compras@techcorp.com.br', '(11) 98765-4321', '12.345.678/0001-90', '2010-03-15', 'Av. Paulista', '1000', 'Bela Vista', 1, '01310-100'),
('InnovaNet Tecnologia', 'contato@innovanet.com.br', '(21) 97654-3210', '23.456.789/0001-01', '2012-07-20', 'Rua da Assembleia', '10', 'Centro', 2, '20011-000'),
('Escritório Advocacia Silva & Costa', 'ti@silvaecosta.adv.br', '(31) 96543-2109', '34.567.890/0001-12', '2015-01-10', 'Av. Afonso Pena', '500', 'Funcionários', 3, '30130-001'),
('Clínica Médica Saúde Plena', 'admin@saudeplena.com.br', '(41) 95432-1098', '45.678.901/0001-23', '2018-05-22', 'Rua XV de Novembro', '200', 'Centro', 4, '80020-310'),

-- Individual clients
('João Pedro Silva', 'joao.silva@email.com', '(11) 94321-0987', '123.456.789-00', '1990-05-15', 'Rua das Flores', '123', 'Jardim Paulista', 1, '01234-000'),
('Maria Santos Oliveira', 'maria.santos@email.com', '(21) 93210-9876', '234.567.890-11', '1985-08-22', 'Av. Atlântica', '456', 'Copacabana', 2, '22070-001'),
('Pedro Costa Ferreira', 'pedro.costa@email.com', '(31) 92109-8765', '345.678.901-22', '1992-11-30', 'Rua da Bahia', '789', 'Centro', 3, '30160-011'),
('Ana Paula Rodrigues', 'ana.rodrigues@email.com', '(41) 91098-7654', '456.789.012-33', '1988-03-18', 'Av. Sete de Setembro', '321', 'Centro', 4, '80060-010'),
('Carlos Eduardo Lima', 'carlos.lima@email.com', '(51) 90987-6543', '567.890.123-44', '1995-12-05', 'Rua dos Andradas', '654', 'Centro Histórico', 5, '90020-015'),
('Juliana Alves Martins', 'juliana.martins@email.com', '(11) 99876-5432', '678.901.234-55', '1987-06-25', 'Av. Ipiranga', '987', 'República', 1, '01046-010'),
('Ricardo Souza Barbosa', 'ricardo.souza@email.com', '(21) 98765-4321', '789.012.345-66', '1993-09-14', 'Rua Visconde de Pirajá', '159', 'Ipanema', 2, '22410-001'),
('Fernanda Gomes Pereira', 'fernanda.gomes@email.com', '(31) 97654-3210', '890.123.456-77', '1991-02-28', 'Av. Brasil', '753', 'Funcionários', 3, '30140-000'),
('Lucas Henrique Araújo', 'lucas.araujo@email.com', '(41) 96543-2109', '901.234.567-88', '1989-07-11', 'Rua Marechal Deodoro', '246', 'Centro', 4, '80010-010'),
('Camila Fernandes Costa', 'camila.costa@email.com', '(51) 95432-1098', '012.345.678-99', '1994-04-19', 'Av. Independência', '135', 'Independência', 5, '90035-070');

-- ========================================
-- VENDEDORES (Sellers)
-- ========================================
INSERT INTO vendedores (NomeVendedor, QtdVendas, QtdVendasFinalizadas) VALUES
('João Silva Santos', 145, 132),
('Maria Oliveira Costa', 198, 185),
('Pedro Henrique Souza', 123, 115),
('Ana Carolina Lima', 167, 158),
('Carlos Eduardo Martins', 89, 82),
('Juliana Ferreira Alves', 156, 148),
('Ricardo Barbosa Santos', 134, 125);

-- ========================================
-- VENDAS (Sales) - Sample sales with various statuses
-- ========================================
-- Note: Adjust dates to be recent for better testing
INSERT INTO vendas (data_venda, hora_venda, valor_total, forma_pagamento, status_venda, observacoes, CodCliente, CodVendedor) VALUES
-- Recent completed sales (Faturada = 2)
('2025-11-10', '10:30:00', 4548.00, 'PIX', 2, 'Cliente corporativo - entrega agendada', 1, 2),
('2025-11-11', '14:15:00', 899.00, 'Cartão de Crédito', 2, 'Parcelado em 3x sem juros', 5, 1),
('2025-11-11', '16:45:00', 1348.00, 'Cartão de Débito', 2, NULL, 6, 3),
('2025-11-12', '09:00:00', 9999.00, 'Transferência', 2, 'Compra corporativa - NF emitida', 2, 2),
('2025-11-12', '11:20:00', 2799.00, 'PIX', 2, 'Entrega expressa solicitada', 7, 4),

-- Pending sales (Pendente = 1)
('2025-11-12', '15:30:00', 1748.00, 'Boleto Bancário', 1, 'Aguardando compensação do boleto', 8, 5),
('2025-11-13', '10:00:00', 3298.00, 'Cartão de Crédito', 1, 'Aguardando aprovação do crédito', 9, 6),

-- Quotes (Orçamento = 0)
('2025-11-13', '11:00:00', 12499.00, NULL, 0, 'Orçamento para renovação completa do escritório', 3, 2),
('2025-11-13', '14:30:00', 5499.00, NULL, 0, 'Orçamento setup gamer completo', 10, 1),

-- Cancelled sale (Cancelada = 3)
('2025-11-09', '16:00:00', 699.00, 'PIX', 3, 'Cliente desistiu da compra', 11, 7);

-- ========================================
-- ITENSVENDA (Sale Items)
-- ========================================
-- Items for Sale 1 (Faturada) - Corporate client
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(5, 1, 10, 499.00, 'Mouse Logitech MX Master 3'),          -- 10 mice
(7, 1, 5, 699.00, 'Teclado Mecânico Keychron K2');         -- 5 keyboards

-- Items for Sale 2 (Faturada) - Individual
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(12, 2, 1, 899.00, 'Monitor Samsung 24" Curvo');

-- Items for Sale 3 (Faturada) - Individual
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(6, 3, 2, 179.00, 'Mouse Gamer Razer DeathAdder'),
(25, 3, 10, 49.00, 'Cabo HDMI 2.1'),
(26, 3, 2, 89.00, 'Mousepad Gamer RGB');

-- Items for Sale 4 (Faturada) - MacBook
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(4, 4, 1, 9999.00, 'MacBook Air M2');

-- Items for Sale 5 (Faturada) - Monitor
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(13, 5, 1, 2799.00, 'Monitor Dell UltraSharp 27"');

-- Items for Sale 6 (Pendente)
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(1, 6, 1, 3299.00, 'Notebook Dell Inspiron 15'),
(17, 6, 1, 449.00, 'SSD Kingston 1TB');

-- Items for Sale 7 (Pendente)
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(2, 7, 1, 2899.00, 'Notebook Lenovo IdeaPad 3'),
(9, 7, 1, 389.00, 'Webcam Logitech C920');

-- Items for Sale 8 (Orçamento) - Office renovation quote
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(1, 8, 3, 3299.00, 'Notebook Dell Inspiron 15'),
(11, 8, 3, 1899.00, 'Monitor LG 27" 4K'),
(14, 8, 3, 1499.00, 'Cadeira Gamer DXRacer');

-- Items for Sale 9 (Orçamento) - Gaming setup quote
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(3, 9, 1, 4599.00, 'Notebook Acer Aspire 5'),
(14, 9, 1, 1499.00, 'Cadeira Gamer DXRacer'),
(15, 9, 1, 599.00, 'Mesa Gamer RGB');

-- Items for Sale 10 (Cancelada)
INSERT INTO itensvenda (CodProduto, CodVenda, quantidade, preco_unitario, Descricao) VALUES
(7, 10, 1, 699.00, 'Teclado Mecânico Keychron K2');

-- ========================================
-- COMPRAS (Purchases) - Sample purchases
-- ========================================
INSERT INTO compras (data_compra, hora_compra, valor_total, forma_pagamento, status_compra, observacoes, CodFornecedor) VALUES
-- Completed purchases (Concluída = 2)
('2025-11-01', '09:00:00', 82475.00, 'Transferência Bancária', 2, 'Reposição de estoque mensal - notebooks', 1),
('2025-11-05', '10:30:00', 29950.00, 'Boleto 30 dias', 2, 'Compra de periféricos Logitech', 3),
('2025-11-08', '14:00:00', 15960.00, 'PIX', 2, 'Monitores Samsung para revenda', 2),

-- Pending purchase (Pendente = 1)
('2025-11-12', '11:00:00', 44940.00, 'Boleto 45 dias', 1, 'Aguardando entrega - componentes de hardware', 5),

-- Purchase order (Pedido = 0)
('2025-11-13', '15:00:00', 22485.00, NULL, 0, 'Pedido de cadeiras e mesas gamer', 4);

-- ========================================
-- ITENSCOMPRA (Purchase Items)
-- ========================================
-- Items for Purchase 1 (Concluída) - Notebooks
INSERT INTO itenscompra (CodProduto, CodCompra, quantidade, preco_unitario, Descricao) VALUES
(1, 1, 25, 3299.00, 'Notebook Dell Inspiron 15');

-- Items for Purchase 2 (Concluída) - Peripherals
INSERT INTO itenscompra (CodProduto, CodCompra, quantidade, preco_unitario, Descricao) VALUES
(5, 2, 60, 499.00, 'Mouse Logitech MX Master 3');

-- Items for Purchase 3 (Concluída) - Monitors
INSERT INTO itenscompra (CodProduto, CodCompra, quantidade, preco_unitario, Descricao) VALUES
(12, 3, 20, 899.00, 'Monitor Samsung 24" Curvo');

-- Items for Purchase 4 (Pendente) - Components
INSERT INTO itenscompra (CodProduto, CodCompra, quantidade, preco_unitario, Descricao) VALUES
(17, 4, 50, 449.00, 'SSD Kingston 1TB'),
(18, 4, 50, 389.00, 'Memória RAM Corsair 16GB');

-- Items for Purchase 5 (Pedido) - Furniture
INSERT INTO itenscompra (CodProduto, CodCompra, quantidade, preco_unitario, Descricao) VALUES
(14, 5, 10, 1499.00, 'Cadeira Gamer DXRacer'),
(15, 5, 15, 599.00, 'Mesa Gamer RGB');

-- ========================================
-- ESTOQUE (Stock Movements)
-- ========================================
-- Note: Stock movements for completed sales and purchases
-- These should align with the faturada sales and concluded purchases above

-- Stock entries from Purchase 1 (notebooks)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(1, 'E', 25, '2025-11-01');

-- Stock entries from Purchase 2 (mice)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(5, 'E', 60, '2025-11-05');

-- Stock entries from Purchase 3 (monitors)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(12, 'E', 20, '2025-11-08');

-- Stock exits from Sale 1 (corporate sale)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(5, 'S', 10, '2025-11-10'),
(7, 'S', 5, '2025-11-10');

-- Stock exit from Sale 2
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(12, 'S', 1, '2025-11-11');

-- Stock exits from Sale 3
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(6, 'S', 2, '2025-11-11'),
(25, 'S', 10, '2025-11-11'),
(26, 'S', 2, '2025-11-11');

-- Stock exit from Sale 4 (MacBook)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(4, 'S', 1, '2025-11-12');

-- Stock exit from Sale 5 (Dell monitor)
INSERT INTO estoque (CodProduto, Tipo, Qtd, Data) VALUES
(13, 'S', 1, '2025-11-12');

-- ========================================
-- Summary Report
-- ========================================
-- After running this script, you will have:
-- - 10 cities across Brazil
-- - 5 suppliers
-- - 25 products across multiple categories
-- - 14 clients (4 corporate + 10 individual)
-- - 7 sellers
-- - 10 sales (5 completed, 2 pending, 2 quotes, 1 cancelled)
-- - 5 purchases (3 completed, 1 pending, 1 order)
-- - Corresponding sale items, purchase items, and stock movements
--
-- This provides a realistic dataset for:
-- - Testing search and filtering functionality
-- - Generating reports
-- - Testing transaction workflows
-- - Demonstrating the system to stakeholders
-- ========================================
