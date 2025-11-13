-- ========================================
-- IntuitERP Settings System Database Schema
-- ========================================
-- This script creates the necessary tables for system-wide and user-specific settings

-- ========================================
-- SYSTEM SETTINGS TABLE
-- ========================================
-- Stores system-wide settings that affect the entire application
-- Only administrators can modify these settings

CREATE TABLE IF NOT EXISTS `system_settings` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `setting_key` VARCHAR(100) NOT NULL UNIQUE COMMENT 'Unique identifier for the setting',
  `setting_value` TEXT NOT NULL COMMENT 'JSON-encoded value or simple string',
  `setting_type` VARCHAR(50) NOT NULL COMMENT 'Data type: string, int, decimal, boolean, json',
  `category` VARCHAR(50) NOT NULL COMMENT 'Category: security, business_rules, formatting, performance, reports',
  `description` TEXT COMMENT 'Human-readable description of what this setting does',
  `default_value` TEXT COMMENT 'Default value if setting is reset',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `updated_by` INT COMMENT 'CodUsuario who last updated this setting',
  INDEX idx_category (`category`),
  INDEX idx_setting_key (`setting_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='System-wide settings (administrator only)';

-- ========================================
-- USER SETTINGS TABLE
-- ========================================
-- Stores per-user preferences and settings
-- Users can modify their own settings

CREATE TABLE IF NOT EXISTS `user_settings` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `cod_usuario` INT NOT NULL COMMENT 'Foreign key to usuario table',
  `setting_key` VARCHAR(100) NOT NULL COMMENT 'Setting identifier',
  `setting_value` TEXT NOT NULL COMMENT 'JSON-encoded value or simple string',
  `setting_type` VARCHAR(50) NOT NULL COMMENT 'Data type: string, int, decimal, boolean, json',
  `category` VARCHAR(50) NOT NULL COMMENT 'Category: ui, workflow, reports, accessibility',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY unique_user_setting (`cod_usuario`, `setting_key`),
  FOREIGN KEY (`cod_usuario`) REFERENCES `usuario`(`CodUsuario`) ON DELETE CASCADE,
  INDEX idx_usuario (`cod_usuario`),
  INDEX idx_category (`category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Per-user settings and preferences';

-- ========================================
-- SETTINGS AUDIT LOG TABLE
-- ========================================
-- Tracks all changes to system and user settings for security and compliance

CREATE TABLE IF NOT EXISTS `settings_audit_log` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `setting_type` ENUM('system', 'user') NOT NULL COMMENT 'Type of setting changed',
  `setting_key` VARCHAR(100) NOT NULL,
  `old_value` TEXT COMMENT 'Previous value before change',
  `new_value` TEXT COMMENT 'New value after change',
  `changed_by` INT NOT NULL COMMENT 'CodUsuario who made the change',
  `changed_for` INT COMMENT 'CodUsuario affected (null for system settings)',
  `change_timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ip_address` VARCHAR(45) COMMENT 'IP address of the user making the change',
  `user_agent` VARCHAR(255) COMMENT 'Browser/client information',
  INDEX idx_changed_by (`changed_by`),
  INDEX idx_changed_for (`changed_for`),
  INDEX idx_timestamp (`change_timestamp`),
  FOREIGN KEY (`changed_by`) REFERENCES `usuario`(`CodUsuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail for all settings changes';

-- ========================================
-- INSERT DEFAULT SYSTEM SETTINGS
-- ========================================
-- High-priority settings from requirements

-- SECURITY & SESSION SETTINGS
INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('session_timeout_minutes', '30', 'int', 'security', 'Automatic logout after N minutes of inactivity (0 = no timeout)', '30'),
('idle_timeout_minutes', '15', 'int', 'security', 'Idle timeout before warning user (0 = no idle timeout)', '15'),
('max_login_attempts', '5', 'int', 'security', 'Maximum failed login attempts before account lockout', '5'),
('lockout_duration_minutes', '15', 'int', 'security', 'Duration of account lockout after max failed attempts', '15'),
('password_min_length', '8', 'int', 'security', 'Minimum password length requirement', '8'),
('password_expiration_days', '90', 'int', 'security', 'Days before password must be changed (0 = never expires)', '90'),
('bcrypt_work_factor', '12', 'int', 'security', 'BCrypt work factor for password hashing (10-14)', '12');

-- BUSINESS RULES SETTINGS
INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('payment_methods', '["Dinheiro", "Cartão de Crédito", "Cartão de Débito", "PIX", "Boleto Bancário", "Transferência", "Cheque", "Crediário"]', 'json', 'business_rules', 'Allowed payment methods (JSON array)', '["Dinheiro", "Cartão de Crédito", "Cartão de Débito", "PIX", "Boleto Bancário"]'),
('max_sale_value', '9999999.99', 'decimal', 'business_rules', 'Maximum allowed sale value', '9999999.99'),
('max_purchase_value', '9999999.99', 'decimal', 'business_rules', 'Maximum allowed purchase value', '9999999.99'),
('min_transaction_year', '2000', 'int', 'business_rules', 'Minimum year for sales/purchase dates', '2000'),
('min_client_age', '18', 'int', 'business_rules', 'Minimum age requirement for clients', '18'),
('allow_negative_stock', 'false', 'boolean', 'business_rules', 'Allow products to have negative stock balance', 'false'),
('require_client_for_sales', 'true', 'boolean', 'business_rules', 'Require client selection for all sales', 'true');

-- FORMATTING & LOCALIZATION SETTINGS
INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('date_format', 'dd/MM/yyyy', 'string', 'formatting', 'Date display format', 'dd/MM/yyyy'),
('time_format', 'HH:mm:ss', 'string', 'formatting', 'Time display format', 'HH:mm:ss'),
('currency_symbol', 'R$', 'string', 'formatting', 'Currency symbol', 'R$'),
('currency_decimal_places', '2', 'int', 'formatting', 'Number of decimal places for currency', '2'),
('currency_decimal_separator', ',', 'string', 'formatting', 'Decimal separator for currency', ','),
('currency_thousands_separator', '.', 'string', 'formatting', 'Thousands separator for currency', '.'),
('number_decimal_places', '2', 'int', 'formatting', 'Default decimal places for numbers', '2');

-- PERFORMANCE & PAGINATION SETTINGS
INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('recent_orders_limit', '5', 'int', 'performance', 'Number of recent orders to display on dashboard', '5'),
('default_page_size', '20', 'int', 'performance', 'Default number of records per page', '20'),
('max_records_per_request', '1000', 'int', 'performance', 'Maximum records returned in a single database query', '1000'),
('search_results_limit', '100', 'int', 'performance', 'Maximum search results to display', '100');

-- REPORT SETTINGS
INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('report_page_size', 'A4', 'string', 'reports', 'Default PDF page size (A4, A3, Letter, Legal)', 'A4'),
('report_page_margins_cm', '2,2,2,2', 'string', 'reports', 'PDF page margins in cm (top,right,bottom,left)', '2,2,2,2'),
('report_header_font_size', '20', 'int', 'reports', 'Report header font size in points', '20'),
('report_body_font_size', '12', 'int', 'reports', 'Report body font size in points', '12'),
('report_include_timestamp', 'true', 'boolean', 'reports', 'Include generation timestamp on reports', 'true'),
('report_include_page_numbers', 'true', 'boolean', 'reports', 'Include page numbers on reports', 'true');

-- ========================================
-- INSERT DEFAULT USER SETTINGS (TEMPLATES)
-- ========================================
-- These are default values that will be used when creating new users
-- Each user will get their own copy of these settings

INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('user_default_theme', 'Auto', 'string', 'ui', 'Default theme for new users (Light, Dark, Auto)', 'Auto'),
('user_default_dashboard_view', 'Overview', 'string', 'ui', 'Default dashboard view for new users', 'Overview'),
('user_default_items_per_page', '20', 'int', 'ui', 'Default items per page for new users', '20');

-- ========================================
-- VERIFICATION QUERIES
-- ========================================
-- Run these to verify the tables were created correctly:

-- SELECT * FROM system_settings ORDER BY category, setting_key;
-- SELECT * FROM user_settings;
-- SELECT * FROM settings_audit_log ORDER BY change_timestamp DESC;

-- Check total settings count:
-- SELECT category, COUNT(*) as setting_count FROM system_settings GROUP BY category;
