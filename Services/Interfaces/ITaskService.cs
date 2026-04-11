using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface ITaskService
{
    Task AddTaskAsync(TaskItem task);
    Task<List<TaskItem>> GetAllTasksAsync();
    Task CompleteTaskAsync(string taskId);
    Task UpdateTaskAsync(TaskItem task);

}