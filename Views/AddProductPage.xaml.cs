using LeltarKezelo.ViewModels;

namespace LeltarKezelo.Views;

public partial class AddProductPage : ContentPage
{
    public AddProductPage(AddProductViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
