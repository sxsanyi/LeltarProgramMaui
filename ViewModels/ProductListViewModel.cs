using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LeltarKezelo.Messages;
using LeltarKezelo.Models;
using LeltarKezelo.Services;

namespace LeltarKezelo.ViewModels;

/// <summary>
/// Főoldal: lapozva (incremental loading) betöltött, kereshető terméklista.
/// Nagy elemszámnál sem tölt be mindent a memóriába: oldalanként <see cref="PageSize"/> elemet kér le,
/// a CollectionView pedig virtualizál.
/// </summary>
public partial class ProductListViewModel : ObservableObject, IRecipient<ProductChangedMessage>
{
    private const int PageSize = 50;
    private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(300);

    private readonly IProductRepository _repository;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;

    private CancellationTokenSource? _searchCts;
    private string _activeQuery = string.Empty;
    private int _queryVersion;
    private bool _hasMoreItems = true;
    private bool _isLoadingMore;
    private bool _isInitialized;

    public ProductListViewModel(
        IProductRepository repository,
        INavigationService navigation,
        IDialogService dialogs)
    {
        _repository = repository;
        _navigation = navigation;
        _dialogs = dialogs;

        WeakReferenceMessenger.Default.Register<ProductChangedMessage>(this);
    }

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await ReloadAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        // Debounce: csak akkor keresünk, ha a felhasználó 300 ms-ig nem gépelt.
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        _ = SearchDebouncedAsync(_searchCts.Token);
    }

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value is null)
        {
            return;
        }

        SelectedProduct = null; // kijelölés törlése, hogy ugyanarra újra lehessen kattintani
        OpenProductCommand.Execute(value);
    }

    [RelayCommand]
    private Task AddProductAsync() => _navigation.GoToAddProductAsync();

    [RelayCommand]
    private Task OpenProductAsync(Product product) => _navigation.GoToProductDetailsAsync(product.Id);

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            await ReloadAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (_isLoadingMore || !_hasMoreItems || IsBusy)
        {
            return;
        }

        _isLoadingMore = true;
        var version = _queryVersion;

        try
        {
            var page = await _repository.SearchAsync(_activeQuery, Products.Count, PageSize);
            if (version != _queryVersion)
            {
                return; // időközben új keresés indult, ez az eredmény elavult
            }

            foreach (var product in page)
            {
                Products.Add(product);
            }

            _hasMoreItems = page.Count == PageSize;
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Hiba", $"A további termékek betöltése nem sikerült: {ex.Message}");
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    public void Receive(ProductChangedMessage message)
    {
        switch (message.Kind)
        {
            case ProductChangeKind.Added:
                // Az új termék a rendezésnek megfelelő helyen jelenjen meg; ha szűrés van érvényben,
                // azt töröljük, hogy biztosan látható legyen.
                if (string.IsNullOrEmpty(SearchText))
                {
                    _ = ReloadAsync();
                }
                else
                {
                    SearchText = string.Empty;
                }

                break;

            case ProductChangeKind.Updated:
                var updatedIndex = IndexOf(message.Product.Id);
                if (updatedIndex >= 0)
                {
                    Products[updatedIndex] = message.Product; // helyben csere, a görgetési pozíció megmarad
                }

                break;

            case ProductChangeKind.Deleted:
                var deletedIndex = IndexOf(message.Product.Id);
                if (deletedIndex >= 0)
                {
                    Products.RemoveAt(deletedIndex);
                    TotalCount = Math.Max(0, TotalCount - 1);
                }

                break;
        }
    }

    private async Task SearchDebouncedAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(SearchDelay, token);
            await ReloadAsync();
        }
        catch (TaskCanceledException)
        {
            // újabb billentyűleütés jött, ez a keresés elavult
        }
    }

    private async Task ReloadAsync()
    {
        var version = ++_queryVersion;
        var query = SearchText.Trim();
        IsBusy = true;

        try
        {
            var count = await _repository.CountAsync(query);
            var firstPage = await _repository.SearchAsync(query, 0, PageSize);

            if (version != _queryVersion)
            {
                return;
            }

            _activeQuery = query;
            _hasMoreItems = firstPage.Count == PageSize;
            TotalCount = count;
            // Új kollekció → egyetlen változásértesítés a CollectionView felé.
            Products = new ObservableCollection<Product>(firstPage);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Hiba", $"A termékek betöltése nem sikerült: {ex.Message}");
        }
        finally
        {
            if (version == _queryVersion)
            {
                IsBusy = false;
            }
        }
    }

    private int IndexOf(int productId)
    {
        for (var i = 0; i < Products.Count; i++)
        {
            if (Products[i].Id == productId)
            {
                return i;
            }
        }

        return -1;
    }
}
