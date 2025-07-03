IntuitERP - Sistema de Gestão Empresarial

IntuitERP é um sistema de gestão de código aberto, desenvolvido como um aplicativo de desktop multiplataforma usando .NET MAUI. O projeto nasceu da necessidade de criar uma alternativa mais intuitiva e amigável aos sistemas ERP tradicionais, que são frequentemente complexos e difíceis de usar. O objetivo é fornecer uma base sólida para um sistema de Ponto de Venda (PDV) e Planejamento de Recursos Empresariais (ERP) que seja poderoso, mas sem sacrificar a experiência do usuário.

🚀 Funcionalidades Principais

O sistema é modularizado para gerenciar diferentes aspectos do negócio, com uma interface centrada no usuário, facilitando a manutenção e a implementação.

    Cadastros:

        Gestão de Clientes: Cadastro completo, incluindo informações de contato, endereço e histórico.

        Gestão de Fornecedores: Gerenciamento de fornecedores com dados cadastrais e de contato.

        Cadastro de Produtos: Manutenção de produtos, incluindo descrição, categoria, preço e controle de estoque.

        Gerenciamento de Vendedores: Cadastro e acompanhamento de vendedores.

        Gerenciamento de Usuários: Controle de acesso ao sistema com usuários e senhas, além de um sistema de permissões detalhado.

        Cadastro de Cidades: Gerenciamento de cidades e UF para uso em outros módulos.

    Operações:

        Lançamento de Vendas: Permite o lançamento de vendas com seleção de cliente, vendedor e múltiplos itens, além de controle de status (orçamento, pendente, faturada).

        Registro de Compras: Registro de compras de fornecedores, com atualização automática do estoque na conclusão.

        Controle de Estoque: Movimentações manuais de entrada e saída, com um registro de todo o histórico.

    Interface e Relatórios:

        Telas de Busca: Interfaces dedicadas para busca e filtragem em todos os módulos principais.

        Geração de Relatórios: Capacidade de gerar relatórios em PDF para os principais módulos, como Vendas, Compras e Produtos.

        Dashboard Intuitivo: Uma tela inicial que apresenta um resumo das atividades recentes e principais indicadores.

🛠️ Tecnologias Utilizadas

    Framework: .NET MAUI (.NET 8) para uma experiência de aplicativo de desktop multiplataforma.

    Linguagem: C#

    Banco de Dados Principal: MySQL.

    Banco de Dados de Configuração: SQLite para armazenar de forma segura a string de conexão do MySQL.

    ORM: Dapper, um micro-ORM de alta performance para acesso a dados.

    Geração de PDF: Biblioteca QuestPDF para a criação de relatórios.

⚙️ Como Executar o Projeto

Siga os passos abaixo para configurar e executar o IntuitERP em seu ambiente de desenvolvimento.

1. Pré-requisitos

    .NET 8 SDK (versão 8.0.411 ou superior).

    Visual Studio 2022 com a carga de trabalho ".NET Multi-platform App UI development" instalada.

    Um servidor de banco de dados MySQL local ou remoto.

2. Configuração do Banco de Dados MySQL

    Crie um novo banco de dados no seu servidor MySQL.

    Utilize o arquivo DatabaseDump(structure).sql para criar a estrutura das tabelas.

    Opcionalmente, use DatabaseDump.sql para popular o banco de dados com dados de exemplo.

3. Configuração da Conexão

O projeto utiliza um aplicativo configurador (DBconfigurator) para gerenciar a string de conexão de forma segura.

    Execute o projeto DBconfigurator primeiro.

    Faça o login com as credenciais padrão:

        Usuário: BbAdmin

        Senha: masterkey

    Na tela de configuração, insira os detalhes do seu banco de dados MySQL:

        Server: O endereço do seu servidor (ex: localhost).

        DataBase: O nome do banco de dados criado (ex: intuiterp_db).

        User: Seu usuário do MySQL.

        Password: Sua senha do MySQL.

    Salve a configuração. Isso irá armazenar a conexão de forma segura em um arquivo SQLite.

4. Executando a Aplicação Principal

    Abra a solução IntuitERP.sln no Visual Studio 2022.

    Defina o projeto IntuitERP como o projeto de inicialização.

    Selecione a plataforma de destino (ex: "Windows Machine").

    Pressione F5 ou o botão de execução para compilar e iniciar a aplicação.

    A tela de login aparecerá, pronta para uso com os usuários cadastrados no banco.

📂 Estrutura do Projeto

    BCK/: Classes para futuras operações de backup (atualmente em desenvolvimento).

    Config/: Contém a classe Configurator, responsável por carregar a conexão do banco de dados a partir do arquivo SQLite.

    DataBase/: Classes de ajuda para a conexão com o MySQL.

    Helpers/: Contém conversores e outras classes auxiliares para a UI.

    models/: Define as entidades do sistema (ex: ClienteModel, ProdutoModel, VendaModel).

    Platforms/: Código específico para cada plataforma suportada pelo .NET MAUI (Windows, Android, iOS, MacCatalyst).

    Properties/: Configurações de inicialização do projeto.

    Resources/: Ativos do aplicativo como imagens, fontes e arquivos brutos.

    Services/: Contém a lógica de negócios e as operações de banco de dados para cada entidade (ex: ClienteService, VendaService).

    validators/: Classes que contêm a lógica para validar os modelos de dados antes de serem salvos no banco.

    Viwes/: Contém as páginas XAML e o code-behind que compõem a interface do usuário.

        Search/: Páginas especializadas para buscar, listar, editar e excluir registros de cada módulo.

        Reports/: Visualizador de relatórios em PDF.

        Modals/: Componentes modais reutilizáveis, como o seletor de itens.
