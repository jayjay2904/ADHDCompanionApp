using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace ADHDCompanionApp.Models.Entities;

public partial class TaskItem : ObservableObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int SortOrder { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private bool reminderEnabled;

    [ObservableProperty]
    private DateTime? reminderDateTime;

    [ObservableProperty]
    private bool isEditingReminder;

    [ObservableProperty]
    private DateTime reminderDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan reminderTime = DateTime.Now.AddMinutes(10).TimeOfDay;
}