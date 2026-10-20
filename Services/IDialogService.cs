namespace LeltarKezelo.Services;

public interface IDialogService
{
    Task ShowErrorAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}

public sealed class ShellDialogService : IDialogService
{
    private static Page? CurrentPage =>
        Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;

    public Task ShowErrorAsync(string title, string message) =>
        CurrentPage?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        CurrentPage?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);
}
