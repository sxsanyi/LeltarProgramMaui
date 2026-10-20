using LeltarKezelo.Services;
using LeltarKezelo.Views;

namespace LeltarKezelo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Részoldalak (push navigáció); DI-ből oldódnak fel.
        Routing.RegisterRoute(Routes.AddProduct, typeof(AddProductPage));
        Routing.RegisterRoute(Routes.ProductDetails, typeof(ProductDetailPage));
    }
}
