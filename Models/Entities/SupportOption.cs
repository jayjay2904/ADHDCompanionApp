using CommunityToolkit.Mvvm.ComponentModel;

namespace ADHDCompanionApp.Models.Entities;

public partial class SupportOption : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string validationText = string.Empty;

    [ObservableProperty]
    private string immediateActionText = string.Empty;

    [ObservableProperty]
    private string nextStepText = string.Empty;

    [ObservableProperty]
    private string alternateImmediateActionText = string.Empty;

    [ObservableProperty]
    private string alternateNextStepText = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}