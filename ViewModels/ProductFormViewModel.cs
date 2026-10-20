using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LeltarKezelo.Helpers;
using LeltarKezelo.Messages;
using LeltarKezelo.Models;
using LeltarKezelo.Services;

namespace LeltarKezelo.ViewModels;

/// <summary>
/// A felvétel és a szerkesztés közös logikája: mezők, validáció, mentés, értesítés a listának.
/// A számmezők stringként kötődnek, így a félig begépelt érték (pl. "12,") nem vész el,
/// és pontos hibaüzenetet tudunk adni.
/// </summary>
public abstract partial class ProductFormViewModel : ObservableObject
{
    protected const int MaxNameLength = 200;
    protected const int MaxBarcodeLength = 64;

    protected ProductFormViewModel(
        IProductRepository repository,
        INavigationService navigation,
        IDialogService dialogs)
    {
        Repository = repository;
        Navigation = navigation;
        Dialogs = dialogs;
    }

    protected IProductRepository Repository { get; }

    protected INavigationService Navigation { get; }

    protected IDialogService Dialogs { get; }

    public abstract string Title { get; }

    /// <summary>Szerkesztett termék azonosítója; új terméknél null.</summary>
    protected abstract int? EditedProductId { get; }

    protected abstract ProductChangeKind ChangeKind { get; }

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string barcode = string.Empty;

    [ObservableProperty]
    private string price = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOutOfStock))]
    private string quantity = string.Empty;

    [ObservableProperty]
    private string? nameError;

    [ObservableProperty]
    private string? barcodeError;

    [ObservableProperty]
    private string? priceError;

    [ObservableProperty]
    private string? quantityError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;

    public bool IsOutOfStock => DecimalParser.TryParse(Quantity, out var q) && q <= 0;

    // Gépeléskor az adott mező hibaüzenete eltűnik.
    partial void OnNameChanged(string value) => NameError = null;

    partial void OnBarcodeChanged(string value) => BarcodeError = null;

    partial void OnPriceChanged(string value) => PriceError = null;

    partial void OnQuantityChanged(string value) => QuantityError = null;

    /// <summary>A validált terméket elmenti az adatbázisba.</summary>
    protected abstract Task PersistAsync(Product product);

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var product = await ValidateAsync();
            if (product is null)
            {
                return;
            }

            await PersistAsync(product);

            WeakReferenceMessenger.Default.Send(new ProductChangedMessage(product, ChangeKind));
            await Navigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            await Dialogs.ShowErrorAsync("A mentés nem sikerült", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Betölti egy meglévő termék adatait a mezőkbe.</summary>
    protected void Populate(Product product)
    {
        Name = product.Name;
        Barcode = product.Barcode;
        Price = DecimalParser.Format(product.Price);
        Quantity = DecimalParser.Format(product.Quantity);
    }

    private async Task<Product?> ValidateAsync()
    {
        var trimmedName = Name?.Trim() ?? string.Empty;
        var trimmedBarcode = Barcode?.Trim() ?? string.Empty;

        NameError = trimmedName.Length switch
        {
            0 => "A terméknév megadása kötelező.",
            > MaxNameLength => $"A terméknév legfeljebb {MaxNameLength} karakter lehet.",
            _ => null
        };

        if (trimmedBarcode.Length == 0)
        {
            BarcodeError = "A vonalkód megadása kötelező.";
        }
        else if (trimmedBarcode.Length > MaxBarcodeLength)
        {
            BarcodeError = $"A vonalkód legfeljebb {MaxBarcodeLength} karakter lehet.";
        }
        else if (trimmedBarcode.Any(char.IsWhiteSpace))
        {
            BarcodeError = "A vonalkód nem tartalmazhat szóközt.";
        }
        else if (await Repository.BarcodeExistsAsync(trimmedBarcode, EditedProductId))
        {
            BarcodeError = "Ezzel a vonalkóddal már létezik termék.";
        }
        else
        {
            BarcodeError = null;
        }

        decimal priceValue = 0;
        if (!DecimalParser.TryParse(Price, out priceValue))
        {
            PriceError = "Adj meg érvényes árat (pl. 1290 vagy 1290,50).";
        }
        else if (priceValue < 0)
        {
            PriceError = "Az ár nem lehet negatív.";
        }
        else
        {
            PriceError = null;
        }

        decimal quantityValue = 0;
        if (!DecimalParser.TryParse(Quantity, out quantityValue))
        {
            QuantityError = "Adj meg érvényes mennyiséget (pl. 12 vagy 2,5).";
        }
        else if (quantityValue < 0)
        {
            QuantityError = "A készletmennyiség nem lehet negatív.";
        }
        else
        {
            QuantityError = null;
        }

        if (NameError is not null || BarcodeError is not null || PriceError is not null || QuantityError is not null)
        {
            return null;
        }

        return new Product
        {
            Id = EditedProductId ?? 0,
            Name = trimmedName,
            Barcode = trimmedBarcode,
            Price = priceValue,
            Quantity = quantityValue
        };
    }
}
