using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly DatabaseService _databaseService;
    private readonly IReminderEngine _reminderEngine;

    public TaskService(
        DatabaseService databaseService,
        IReminderEngine reminderEngine)
    {
        _databaseService = databaseService;
        _reminderEngine = reminderEngine;
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            if (string.IsNullOrWhiteSpace(task.Id))
            {
                task.Id = Guid.NewGuid().ToString();
            }

            if (task.CreatedUtc == default)
            {
                task.CreatedUtc = DateTime.UtcNow;
            }

            await _databaseService.SaveTaskAsync(task);
            await _reminderEngine.ScheduleTaskReminderAsync(task);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TaskService] AddTaskAsync failed: {ex}");
        }
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        try
        {
            var tasks = await _databaseService.GetTasksAsync();

            var cutoff = DateTime.UtcNow.AddHours(-24);

            var oldCompletedTasks = tasks
                .Where(t => t.IsCompleted && t.CompletedUtc.HasValue && t.CompletedUtc.Value < cutoff)
                .ToList();

            foreach (var task in oldCompletedTasks)
            {
                await _reminderEngine.CancelTaskReminderAsync(task);
                await _databaseService.DeleteTaskAsync(task);
            }

            tasks = await _databaseService.GetTasksAsync();

            return tasks;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TaskService] GetAllTasksAsync failed: {ex}");
            return new List<TaskItem>();
        }
    }

    public async Task CompleteTaskAsync(string taskId)
    {
        try
        {
            var tasks = await _databaseService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task is not null)
            {
                task.IsCompleted = true;
                task.CompletedUtc = DateTime.UtcNow;

                await _databaseService.SaveTaskAsync(task);
                await _reminderEngine.CancelTaskReminderAsync(task);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TaskService] CompleteTaskAsync failed: {ex}");
        }
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            await _databaseService.SaveTaskAsync(task);
            await _reminderEngine.ScheduleTaskReminderAsync(task);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TaskService] UpdateTaskAsync failed: {ex}");
        }
    }
}