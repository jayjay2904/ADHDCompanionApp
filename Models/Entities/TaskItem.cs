using CommunityToolkit.Mvvm.ComponentModel;

namespace ADHDCompanionApp.Models.Entities;

public partial class TaskItem : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isCompleted;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}