-- ============================================================================
-- IntuitERP - Unified PostgreSQL Database Initialization Script
-- ============================================================================
-- Purpose: Initialize all core, settings, and financial tables for Supabase/PostgreSQL
-- Version: 1.2.0 (Column names standardized to lowercase snake_case)
-- ============================================================================

-- EXTENSIONS
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. BASE TABLES
-- ============================================================================

-- Cidade
CREATE TABLE IF NOT EXISTS cidade (
    cod_cidade SERIAL PRIMARY KEY,
    cidade     VARCHAR(255),
    uf         VARCHAR(2)
);

-- Usuarios
CREATE TABLE IF NOT EXISTS usuarios (
    cod_usuarios                    SERIAL PRIMARY KEY,
    usuario                         VARCHAR(255) NOT NULL UNIQUE,
    senha                           VARCHAR(255) NOT NULL,
    permissao_produtos_create       INT DEFAULT 0,
    permissao_produtos_read         INT DEFAULT 0,
    permissao_produtos_update       INT DEFAULT 0,
    permissao_produtos_delete       INT DEFAULT 0,
    permissao_vendas_create         INT DEFAULT 0,
    permissao_vendas_read           INT DEFAULT 0,
    permissao_vendas_update         INT DEFAULT 0,
    permissao_vendas_delete         INT DEFAULT 0,
    permissao_relatorios_generate   INT DEFAULT 0,
    permissao_vendedores_create     INT DEFAULT 0,
    permissao_vendedores_read       INT DEFAULT 0,
    permissao_vendedores_update     INT DEFAULT 0,
    permissao_vendedores_delete     INT DEFAULT 0,
    permissao_fornecedores_create   INT DEFAULT 0,
    permissao_fornecedores_read     INT DEFAULT 0,
    permissao_fornecedores_update   INT DEFAULT 0,
    permissao_fornecedores_delete   INT DEFAULT 0,
    permissao_clientes_create       INT DEFAULT 0,
    permissao_clientes_read         INT DEFAULT 0,
    permissao_clientes_update       INT DEFAULT 0,
    permissao_clientes_delete       INT DEFAULT 0
);

-- Vendedor
CREATE TABLE IF NOT EXISTS vendedor (
    cod_vendedor            SERIAL PRIMARY KEY,
    nome_vendedor           VARCHAR(255) NOT NULL,
    comissao                DECIMAL(5,2) DEFAULT 0,
    ativo                   BOOLEAN DEFAULT TRUE,
    qtd_vendas              INT DEFAULT 0,
    qtd_vendas_finalizadas  INT DEFAULT 0
);

-- Fornecedor
CREATE TABLE IF NOT EXISTS fornecedor (
    cod_fornecedor  SERIAL PRIMARY KEY,
    razao_social    VARCHAR(255),
    nome_fantasia   VARCHAR(255),
    cnpj            VARCHAR(20) UNIQUE,
    email           VARCHAR(255),
    telefone        VARCHAR(20),
    endereco        VARCHAR(255),
    numero          VARCHAR(50),
    bairro          VARCHAR(100),
    cod_cidade      INT REFERENCES cidade(cod_cidade),
    cep             VARCHAR(20),
    ativo           BOOLEAN DEFAULT TRUE
);

-- Cliente
CREATE TABLE IF NOT EXISTS cliente (
    cod_cliente         SERIAL PRIMARY KEY,
    cod_cidade          INT REFERENCES cidade(cod_cidade),
    nome                VARCHAR(255),
    email               VARCHAR(255),
    telefone            VARCHAR(20),
    data_nascimento     DATE,
    cpf                 VARCHAR(20),
    endereco            VARCHAR(255),
    numero              VARCHAR(50),
    bairro              VARCHAR(100),
    cep                 VARCHAR(20),
    data_cadastro       TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_ultima_compra  TIMESTAMP,
    ativo               BOOLEAN DEFAULT TRUE
);

-- Produto
CREATE TABLE IF NOT EXISTS produto (
    cod_produto     SERIAL PRIMARY KEY,
    descricao       VARCHAR(255),
    categoria       VARCHAR(100),
    preco_unitario  DECIMAL(10,2),
    saldo_est       INT DEFAULT 0,
    fornecedor_id   INT REFERENCES fornecedor(cod_fornecedor),
    data_cadastro   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    est_minimo      INT DEFAULT 0,
    estoque_id      INT,
    variante_id     INT,
    tipo            VARCHAR(50),
    ativo           BOOLEAN DEFAULT TRUE
);

-- Venda
CREATE TABLE IF NOT EXISTS venda (
    cod_venda       SERIAL PRIMARY KEY,
    data_venda      DATE DEFAULT CURRENT_DATE,
    hora_venda      TIME DEFAULT CURRENT_TIME,
    cod_cliente     INT NOT NULL REFERENCES cliente(cod_cliente),
    desconto        DECIMAL(10,2) DEFAULT 0,
    cod_vendedor    INT REFERENCES vendedor(cod_vendedor),
    obs             TEXT,
    valor_total     DECIMAL(10,2) NOT NULL,
    forma_pagamento VARCHAR(50),
    status_venda    SMALLINT DEFAULT 1
);

-- Itens Venda
CREATE TABLE IF NOT EXISTS itens_venda (
    id              SERIAL PRIMARY KEY,
    cod_venda       INT NOT NULL REFERENCES venda(cod_venda) ON DELETE CASCADE,
    cod_produto     INT NOT NULL REFERENCES produto(cod_produto),
    quantidade      INT NOT NULL,
    preco_unitario  DECIMAL(10,2) NOT NULL,
    descricao       VARCHAR(255),
    subtotal        DECIMAL(10,2) GENERATED ALWAYS AS (quantidade * preco_unitario) STORED
);

-- Compra
CREATE TABLE IF NOT EXISTS compra (
    cod_compra      SERIAL PRIMARY KEY,
    data_compra     DATE DEFAULT CURRENT_DATE,
    cod_fornecedor  INT NOT NULL REFERENCES fornecedor(cod_fornecedor),
    valor_total     DECIMAL(10,2) NOT NULL,
    status_compra   SMALLINT DEFAULT 1
);

-- Itens Compra
CREATE TABLE IF NOT EXISTS itens_compra (
    id              SERIAL PRIMARY KEY,
    cod_compra      INT NOT NULL REFERENCES compra(cod_compra) ON DELETE CASCADE,
    cod_produto     INT NOT NULL REFERENCES produto(cod_produto),
    quantidade      INT NOT NULL,
    preco_unitario  DECIMAL(10,2) NOT NULL,
    subtotal        DECIMAL(10,2) GENERATED ALWAYS AS (quantidade * preco_unitario) STORED
);

-- Estoque
CREATE TABLE IF NOT EXISTS estoque (
    id          SERIAL PRIMARY KEY,
    cod_produto INT NOT NULL REFERENCES produto(cod_produto),
    tipo        CHAR(1) CHECK (tipo IN ('E', 'S')),
    qtd         INT NOT NULL,
    data        DATE DEFAULT CURRENT_DATE
);

-- 2. SETTINGS & AUDIT TABLES
-- ============================================================================

-- System Settings
CREATE TABLE IF NOT EXISTS system_settings (
    id              SERIAL PRIMARY KEY,
    setting_key     VARCHAR(100) NOT NULL UNIQUE,
    setting_value   TEXT NOT NULL,
    setting_type    VARCHAR(50) NOT NULL,
    category        VARCHAR(50) NOT NULL,
    description     TEXT,
    default_value   TEXT,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by      INT REFERENCES usuarios(cod_usuarios)
);

-- User Settings
CREATE TABLE IF NOT EXISTS user_settings (
    id              SERIAL PRIMARY KEY,
    cod_usuario     INT NOT NULL REFERENCES usuarios(cod_usuarios) ON DELETE CASCADE,
    setting_key     VARCHAR(100) NOT NULL,
    setting_value   TEXT NOT NULL,
    setting_type    VARCHAR(50) NOT NULL,
    category        VARCHAR(50) NOT NULL,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_usuario, setting_key)
);

-- Settings Audit Log
CREATE TABLE IF NOT EXISTS settings_audit_log (
    id                  SERIAL PRIMARY KEY,
    setting_type        VARCHAR(20) CHECK (setting_type IN ('system', 'user')),
    setting_key         VARCHAR(100) NOT NULL,
    old_value           TEXT,
    new_value           TEXT,
    changed_by          INT NOT NULL REFERENCES usuarios(cod_usuarios),
    changed_for         INT REFERENCES usuarios(cod_usuarios),
    change_timestamp    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address          VARCHAR(45),
    user_agent          VARCHAR(255)
);

-- 3. FINANCIAL TABLES
-- ============================================================================

-- Contas Receber
CREATE TABLE IF NOT EXISTS contas_receber (
    id              SERIAL PRIMARY KEY,
    cod_venda       INT NOT NULL UNIQUE REFERENCES venda(cod_venda),
    cod_cliente     INT NOT NULL REFERENCES cliente(cod_cliente),
    data_emissao    DATE NOT NULL,
    valor_total     DECIMAL(10,2) NOT NULL,
    valor_pago      DECIMAL(10,2) DEFAULT 0,
    valor_pendente  DECIMAL(10,2) NOT NULL,
    num_parcelas    INT DEFAULT 1,
    status          VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado')),
    observacoes     TEXT,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parcelas Receber
CREATE TABLE IF NOT EXISTS parcelas_receber (
    id                  SERIAL PRIMARY KEY,
    cod_conta_receber   INT NOT NULL REFERENCES contas_receber(id) ON DELETE CASCADE,
    numero_parcela      INT NOT NULL,
    data_vencimento     DATE NOT NULL,
    valor_parcela       DECIMAL(10,2) NOT NULL,
    valor_pago          DECIMAL(10,2) DEFAULT 0,
    data_pagamento      DATE,
    forma_pagamento     VARCHAR(50),
    status              VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Pago', 'Vencido', 'Cancelado')),
    juros               DECIMAL(10,2) DEFAULT 0,
    multa               DECIMAL(10,2) DEFAULT 0,
    desconto            DECIMAL(10,2) DEFAULT 0,
    valor_total         DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
    observacoes         TEXT,
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_conta_receber, numero_parcela)
);

-- Contas Pagar
CREATE TABLE IF NOT EXISTS contas_pagar (
    id              SERIAL PRIMARY KEY,
    cod_compra      INT NOT NULL UNIQUE REFERENCES compra(cod_compra),
    cod_fornecedor  INT NOT NULL REFERENCES fornecedor(cod_fornecedor),
    data_emissao    DATE NOT NULL,
    valor_total     DECIMAL(10,2) NOT NULL,
    valor_pago      DECIMAL(10,2) DEFAULT 0,
    valor_pendente  DECIMAL(10,2) NOT NULL,
    num_parcelas    INT DEFAULT 1,
    status          VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado')),
    observacoes     TEXT,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parcelas Pagar
CREATE TABLE IF NOT EXISTS parcelas_pagar (
    id                  SERIAL PRIMARY KEY,
    cod_conta_pagar     INT NOT NULL REFERENCES contas_pagar(id) ON DELETE CASCADE,
    numero_parcela      INT NOT NULL,
    data_vencimento     DATE NOT NULL,
    valor_parcela       DECIMAL(10,2) NOT NULL,
    valor_pago          DECIMAL(10,2) DEFAULT 0,
    data_pagamento      DATE,
    forma_pagamento     VARCHAR(50),
    status              VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Pago', 'Vencido', 'Cancelado')),
    juros               DECIMAL(10,2) DEFAULT 0,
    multa               DECIMAL(10,2) DEFAULT 0,
    desconto            DECIMAL(10,2) DEFAULT 0,
    valor_total         DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
    observacoes         TEXT,
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_conta_pagar, numero_parcela)
);

-- 4. INITIAL DATA
-- ============================================================================

-- Default Admin User (Password: admin123)
INSERT INTO usuarios (
    usuario, senha,
    permissao_produtos_create, permissao_produtos_read, permissao_produtos_update, permissao_produtos_delete,
    permissao_vendas_create, permissao_vendas_read, permissao_vendas_update, permissao_vendas_delete,
    permissao_relatorios_generate,
    permissao_vendedores_create, permissao_vendedores_read, permissao_vendedores_update, permissao_vendedores_delete,
    permissao_fornecedores_create, permissao_fornecedores_read, permissao_fornecedores_update, permissao_fornecedores_delete,
    permissao_clientes_create, permissao_clientes_read, permissao_clientes_update, permissao_clientes_delete
) VALUES (
    'admin', '$2a$12$R9h/lIPzHZ5fJ1B1pI.LReXqE.9K7X/uX7E3j9H7L9G7I7H7I7H7',
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
) ON CONFLICT (usuario) DO NOTHING;

-- Default Settings
INSERT INTO system_settings (setting_key, setting_value, setting_type, category, description, default_value) VALUES
('session_timeout_minutes', '30',    'int',     'security',       'Automatic logout after N minutes of inactivity', '30'),
('bcrypt_work_factor',       '12',   'int',     'security',       'BCrypt work factor for password hashing',        '12'),
('currency_symbol',          'R$',   'string',  'formatting',     'Currency symbol',                                'R$'),
('allow_negative_stock',     'false','boolean', 'business_rules', 'Allow products to have negative stock balance',  'false')
ON CONFLICT (setting_key) DO NOTHING;

-- Financial Settings
INSERT INTO system_settings (setting_key, setting_value, setting_type, category, description, default_value) VALUES
('contas_juros_mensal_percent',    '1.0', 'decimal', 'financial', 'Monthly interest rate for overdue accounts (%)', '1.0'),
('contas_multa_atraso_percent',    '2.0', 'decimal', 'financial', 'Late payment penalty percentage (%)',            '2.0'),
('contas_intervalo_parcelas_dias', '30',  'int',     'financial', 'Default days between installments',              '30')
ON CONFLICT (setting_key) DO NOTHING;
