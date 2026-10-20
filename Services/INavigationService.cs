namespace LeltarKezelo.Services;

/// <summary>Navigációs absztrakció, hogy a ViewModelek ne függjenek közvetlenül a Shell-től (tesztelhetőség).</summary>
public interface INavigationService
{
    Task GoToAddProductAsync();

    Task GoToProductDetailsAsync(int productId);

    Task GoBackAsync();
}

public static class Routes
{
    public const string AddProduct = "addproduct";
    public const string ProductDetails = "productdetails";
}

public static class QueryKeys
{
    public const string ProductId = "id";
}

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAddProductAsync() =>
        Shell.Current.GoToAsync(Routes.AddProduct);

    public Task GoToProductDetailsAsync(int productId) =>
        Shell.Current.GoToAsync($"{Routes.ProductDetails}?{QueryKeys.ProductId}={productId}");

    public Task GoBackAsync() =>
        Shell.Current.GoToAsync("..");
}
