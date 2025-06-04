using IntuitERP.Viwes;

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
        }
    }
}
