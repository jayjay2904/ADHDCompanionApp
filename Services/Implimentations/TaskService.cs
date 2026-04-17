using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly DatabaseService _databaseService;

    public TaskService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task AddTaskAsync(TaskItem task)
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
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        var tasks = await _databaseService.GetTasksAsync();

        var cutoff = DateTime.UtcNow.AddHours(-24);

        var oldCompletedTasks = tasks
            .Where(t => t.IsCompleted && t.CompletedUtc.HasValue && t.CompletedUtc.Value < cutoff)
            .ToList();

        foreach (var task in oldCompletedTasks)
        {
            await _databaseService.DeleteTaskAsync(task);
        }

        tasks = await _databaseService.GetTasksAsync();

        return tasks;
    }

    public async Task CompleteTaskAsync(string taskId)
    {
        var tasks = await _databaseService.GetTasksAsync();
        var task = tasks.FirstOrDefault(t => t.Id == taskId);

        if (task is not null)
        {
            task.IsCompleted = true;
            task.CompletedUtc = DateTime.UtcNow;
            await _databaseService.SaveTaskAsync(task);
        }
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        if (task is null)
            return;

        await _databaseService.SaveTaskAsync(task);
    }
}