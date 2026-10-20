using CommunityToolkit.Mvvm.Input;
using LeltarKezelo.Messages;
using LeltarKezelo.Models;
using LeltarKezelo.Services;

namespace LeltarKezelo.ViewModels;

public sealed partial class AddProductViewModel : ProductFormViewModel
{
    public AddProductViewModel(
        IProductRepository repository,
        INavigationService navigation,
        IDialogService dialogs)
        : base(repository, navigation, dialogs)
    {
        Quantity = "0";
    }

    public override string Title => "Új termék";

    protected override int? EditedProductId => null;

    protected override ProductChangeKind ChangeKind => ProductChangeKind.Added;

    protected override Task PersistAsync(Product product) => Repository.AddAsync(product);

    [RelayCommand]
    private Task CancelAsync() => Navigation.GoBackAsync();
}
