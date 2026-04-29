using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using System.Collections.ObjectModel;

namespace ADHDCompanionApp.ViewModels;

public partial class SupportViewModel : BaseViewModel
{
    private readonly ISupportService _supportService;
    private SupportOption? _currentOption;
    private bool _showingAlternateStep;
    private int _quickResetIndex = 0;

    private readonly List<string> _quickResetPrompts = new()
    {
        "Take one slow breath in. Hold for a moment. Breathe out longer than you breathed in.",
        "Drop your shoulders. Unclench your jaw. Take one slow breath. You only need the next small step.",
        "Put both feet on the floor. Look around you. Name one thing you can see and one thing you can hear.",
        "You do not need to fix the whole day. Just soften this moment a little."
    };

    public ObservableCollection<SupportOption> SupportOptions { get; } = new();

    [ObservableProperty]
    private string selectedSupportTitle = string.Empty;

    [ObservableProperty]
    private string selectedValidationText = string.Empty;

    [ObservableProperty]
    private string selectedImmediateActionText = string.Empty;

    [ObservableProperty]
    private string selectedNextStepText = string.Empty;

    [ObservableProperty]
    private bool hasSelectedOption = false;

    [ObservableProperty]
    private string quickResetText = "Take one slow breath in. Hold for a moment. Breathe out longer than you breathed in.";

    public SupportViewModel(ISupportService supportService)
    {
        _supportService = supportService;
        Title = "Support";
    }

    public async Task LoadSupportOptionsAsync()
    {
        if (SupportOptions.Count > 0)
        {
            return;
        }

        var options = await _supportService.GetSupportOptionsAsync();

        SupportOptions.Clear();

        foreach (var option in options)
        {
            option.IsSelected = false;
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

        foreach (var supportOption in SupportOptions)
        {
            supportOption.IsSelected = false;
        }

        option.IsSelected = true;

        _currentOption = option;
        _showingAlternateStep = false;

        SelectedSupportTitle = option.Title;
        SelectedValidationText = option.ValidationText;
        SelectedImmediateActionText = option.ImmediateActionText;
        SelectedNextStepText = option.NextStepText;
        HasSelectedOption = true;
    }

    [RelayCommand]
    private void TryAnotherStep()
    {
        if (_currentOption is null)
        {
            return;
        }

        _showingAlternateStep = !_showingAlternateStep;

        SelectedImmediateActionText = _showingAlternateStep
            ? _currentOption.AlternateImmediateActionText
            : _currentOption.ImmediateActionText;

        SelectedNextStepText = _showingAlternateStep
            ? _currentOption.AlternateNextStepText
            : _currentOption.NextStepText;
    }

    [RelayCommand]
    private void ClearSupportPlan()
    {
        _currentOption = null;
        _showingAlternateStep = false;

        foreach (var supportOption in SupportOptions)
        {
            supportOption.IsSelected = false;
        }

        SelectedSupportTitle = string.Empty;
        SelectedValidationText = string.Empty;
        SelectedImmediateActionText = string.Empty;
        SelectedNextStepText = string.Empty;
        HasSelectedOption = false;
    }

    [RelayCommand]
    private void ResetQuickSupport()
    {
        _quickResetIndex++;

        if (_quickResetIndex >= _quickResetPrompts.Count)
        {
            _quickResetIndex = 0;
        }

        QuickResetText = _quickResetPrompts[_quickResetIndex];
    }
}