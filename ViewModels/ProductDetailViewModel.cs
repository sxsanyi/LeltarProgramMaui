using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LeltarKezelo.Messages;
using LeltarKezelo.Models;
using LeltarKezelo.Services;

namespace LeltarKezelo.ViewModels;

/// <summary>
/// Részletező / szerkesztő oldal. Az azonosítót Shell query paraméterként kapja,
/// és mindig friss adatot olvas az adatbázisból (nem a lista elemét módosítja).
/// </summary>
public sealed partial class ProductDetailViewModel : ProductFormViewModel, IQueryAttributable
{
    public ProductDetailViewModel(
        IProductRepository repository,
        INavigationService navigation,
        IDialogService dialogs)
        : base(repository, navigation, dialogs)
    {
    }

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private bool isLoaded;

    public override string Title => "Termék részletei";

    protected override int? EditedProductId => ProductId;

    protected override ProductChangeKind ChangeKind => ProductChangeKind.Updated;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(QueryKeys.ProductId, out var raw) &&
            int.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), out var id))
        {
            _ = LoadAsync(id);
        }
    }

    protected override Task PersistAsync(Product product) => Repository.UpdateAsync(product);

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!IsLoaded || IsBusy)
        {
            return;
        }

        var confirmed = await Dialogs.ConfirmAsync(
            "Termék törlése",
            $"Biztosan törlöd a(z) „{Name}” terméket?",
            "Törlés",
            "Mégse");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await Repository.DeleteAsync(ProductId);
            WeakReferenceMessenger.Default.Send(
                new ProductChangedMessage(new Product { Id = ProductId }, ProductChangeKind.Deleted));
            await Navigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            await Dialogs.ShowErrorAsync("A törlés nem sikerült", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync(int id)
    {
        IsBusy = true;
        try
        {
            var product = await Repository.GetByIdAsync(id);
            if (product is null)
            {
                await Dialogs.ShowErrorAsync("Hiba", "A termék nem található.");
                await Navigation.GoBackAsync();
                return;
            }

            ProductId = product.Id;
            Populate(product);
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            await Dialogs.ShowErrorAsync("Hiba", $"A termék betöltése nem sikerült: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
