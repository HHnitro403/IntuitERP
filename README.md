IntuitERP - Sistema de Gestão Empresarial

IntuitERP é um sistema de gestão simples, desenvolvido como um aplicativo desktop multiplataforma usando .NET MAUI. O objetivo do projeto é fornecer uma base para um sistema de Ponto de Venda (PDV) e Planejamento de Recursos Empresariais (ERP), cobrindo as operações essenciais de um pequeno negócio.
🚀 Funcionalidades Principais

O sistema é modularizado para gerenciar diferentes aspectos do negócio:

    Cadastros:
        Gestão completa de Clientes.
        Gestão de Fornecedores.
        Cadastro e manutenção de Produtos.
        Gerenciamento de Vendedores.
        Gerenciamento de Usuários do sistema com senhas.
        Cadastro de Cidades e UF para uso em outros módulos.
    Operações:
        Lançamento de Vendas, com seleção de cliente, vendedor e múltiplos itens.
        Registro de Compras de fornecedores.
        Controle de Estoque com movimentações de entrada e saída.
    Interface:
        Telas de busca dedicadas para cada módulo principal (Clientes, Produtos, Vendas, etc.).
        Formulários intuitivos para criação e edição de registros.
        Interface responsiva construída com .NET MAUI.

🛠️ Tecnologias Utilizadas

    Framework: .NET MAUI (Multi-platform App UI)
    Linguagem: C#
    Interface: XAML
    Banco de Dados Principal: MySQL
    Banco de Dados de Configuração: SQLite (usado para armazenar a string de conexão do MySQL)
    ORM: Dapper (um micro-ORM de alta performance)

⚙️ Como Executar o Projeto

Siga os passos abaixo para configurar e executar o IntuitERP em seu ambiente de desenvolvimento.
1. Pré-requisitos

    .NET 8 SDK (ou superior).
    Visual Studio 2022 com a carga de trabalho ".NET Multi-platform App UI development" instalada.
    Um servidor de banco de dados MySQL instalado e em execução.

2. Configuração do Banco de Dados MySQL

Use o arquivo DatabaseDump(structure).sql para a estrutura e o DatabaseDump.sql que vira com dados Mockup

3. Configuração da Conexão

O projeto utiliza um banco de dados SQLite para armazenar a string de conexão do MySQL. Ao executar o aplicativo pela primeira vez, ele criará um arquivo ConfigsDB.db no diretório de saída (ex: bin/Debug/.../Config/).

Você precisa atualizar a conexão neste arquivo:

    Use uma ferramenta de gerenciamento de SQLite (como o DB Browser for SQLite) para abrir o arquivo ConfigsDB.db(em breve teré uma ferramneta de configuração propria junto do projeto).
    Abra a tabela Connection.
    Edite o único registro (ID = 1) e insira as informações do seu banco de dados MySQL:
        Server: O endereço do seu servidor (ex: localhost ou 127.0.0.1).
        Database: O nome do banco de dados que você criou (ex: intuiterp_db).
        User: Seu usuário do MySQL (ex: root).
        Password: Sua senha do MySQL.
    Salve as alterações.

4. Executando a Aplicação

    Clone este repositório.
    Abra o arquivo da solução (.sln) no Visual Studio 2022.
    Selecione a plataforma de destino (ex: "Windows Machine" ou um emulador Android).
    Pressione F5 ou clique no botão de "play" para compilar e executar o projeto.
    A tela de login deve aparecer, pronta para ser usada.

📂 Estrutura do Projeto

    BCK/: Contém classes para operações de backup (atualmente vazias).
    Config/: Responsável pela configuração da conexão com o banco de dados.
    DataBase/: Classes de ajuda para a conexão com o MySQL.
    Helpers/: Contém conversores e outras classes auxiliares para a UI.
    models/: Define as entidades do sistema (Cliente, Produto, Venda, etc.).
    Platforms/: Código específico para cada plataforma (.NET MAUI).
    Properties/: Configurações de inicialização do projeto.
    Resources/: Ativos do aplicativo como imagens, fontes e arquivos brutos.
    Services/: Lida com a lógica de negócios e as operações de banco de dados para cada entidade.
    validators/: Classes que contêm a lógica para validar os modelos de dados.
    Viwes/: Contém as páginas XAML e o code-behind que compõem a interface do usuário.
        Search/: Páginas especializadas para buscar e listar registros.
