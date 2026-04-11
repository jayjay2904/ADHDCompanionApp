using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly List<TaskItem> _tasks = new();

    public Task AddTaskAsync(TaskItem task)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task<List<TaskItem>> GetAllTasksAsync()
    {
        return Task.FromResult(_tasks.ToList());
    }

    public Task CompleteTaskAsync(string taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);

        if (task is not null)
        {
            task.IsCompleted = true;
        }

        return Task.CompletedTask;
    }
    public Task UpdateTaskAsync(TaskItem task)
    {
        var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);

        if (existingTask is not null)
        {
            existingTask.Title = task.Title;
            existingTask.IsCompleted = task.IsCompleted;
        }

        return Task.CompletedTask;
    }
}