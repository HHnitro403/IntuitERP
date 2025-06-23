using IntuitERP.Viwes;
using IntuitERP.Viwes.Search;

namespace IntuitERP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MaenuPage), typeof(MaenuPage));
            Routing.RegisterRoute(nameof(CadastrodeCidade), typeof(CadastrodeCidade));
            Routing.RegisterRoute(nameof(CadastrodeCliente), typeof(CadastrodeCliente));
            Routing.RegisterRoute(nameof(CadastrodeCompra), typeof(CadastrodeCompra));
            Routing.RegisterRoute(nameof(CadastrodeFornecedor), typeof(CadastrodeFornecedor));
            Routing.RegisterRoute(nameof(CadastroProduto), typeof(CadastroProduto));
            Routing.RegisterRoute(nameof(CadastrodeVendedor), typeof(CadastrodeVendedor));
            Routing.RegisterRoute(nameof(CadstroEstoque), typeof(CadstroEstoque));
            Routing.RegisterRoute(nameof(CadastrodeVenda), typeof(CadastrodeVenda));
            Routing.RegisterRoute(nameof(CadastrodeCompra), typeof(CadastrodeCompra));
            Routing.RegisterRoute(nameof(CadastrodeUsuario), typeof(CadastrodeUsuario));
            Routing.RegisterRoute(nameof(ClienteSearch), typeof(ClienteSearch));
            Routing.RegisterRoute(nameof(FornecedorSearch), typeof(FornecedorSearch));
            Routing.RegisterRoute(nameof(ProdutoSearch), typeof(ProdutoSearch));
            Routing.RegisterRoute(nameof(VendedorSearch), typeof(VendedorSearch));
            Routing.RegisterRoute(nameof(UsuarioSearch), typeof(UsuarioSearch));
            Routing.RegisterRoute(nameof(VendaSearch), typeof(VendaSearch));
            Routing.RegisterRoute(nameof(CompraSearch), typeof(CompraSearch));
        }
    }
}
