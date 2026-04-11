using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using System.Collections.ObjectModel;

namespace ADHDCompanionApp.ViewModels;

public partial class SupportViewModel : BaseViewModel
{
    private readonly ISupportService _supportService;

    public ObservableCollection<SupportOption> SupportOptions { get; } = new();

    [ObservableProperty]
    private string selectedSupportText = "Choose what kind of help you need right now.";

    public SupportViewModel(ISupportService supportService)
    {
        _supportService = supportService;
        Title = "Support";
    }

    public async Task LoadSupportOptionsAsync()
    {
        var options = await _supportService.GetSupportOptionsAsync();

        SupportOptions.Clear();

        foreach (var option in options)
        {
            SupportOptions.Add(option);
        }
    }

    [RelayCommand]
    private void SelectSupportOption(SupportOption option)
    {
        if (option is null)
        {
            return;
        }

        SelectedSupportText = option.ResponseText;
    }
}