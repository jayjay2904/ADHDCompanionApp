using System.Collections.Specialized;
using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class ArloPage : ContentPage
{
    private readonly ArloViewModel _viewModel;

    public ArloPage(ArloViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;

        _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMessagesAsync();
    }

    private async void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await ScrollToBottomAsync();
    }

    private async Task ScrollToBottomAsync()
    {
        await Task.Delay(50);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await MessagesScrollView.ScrollToAsync(0, MessagesStack.Height, true);
        });
    }
}