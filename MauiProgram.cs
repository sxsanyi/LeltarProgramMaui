using LeltarKezelo.Services;
using LeltarKezelo.ViewModels;
using LeltarKezelo.Views;
using Microsoft.Extensions.Logging;

namespace LeltarKezelo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Szolgáltatások
        builder.Services.AddSingleton<IProductRepository, ProductRepository>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();

        // Főoldal: egész életciklus alatt egy példány (állapot: keresés, betöltött lap, görgetés)
        builder.Services.AddSingleton<ProductListViewModel>();
        builder.Services.AddSingleton<ProductListPage>();

        // Részoldalak: minden navigációnál friss példány, tiszta állapottal
        builder.Services.AddTransient<AddProductViewModel>();
        builder.Services.AddTransient<AddProductPage>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<ProductDetailPage>();

        return builder.Build();
    }
}
