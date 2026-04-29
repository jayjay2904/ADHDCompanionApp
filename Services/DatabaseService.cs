using ADHDCompanionApp.Models.Entities;
using SQLite;

namespace ADHDCompanionApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        await _database.CreateTableAsync<TaskItem>();
        await _database.CreateTableAsync<WinEntry>();
        await _database.CreateTableAsync<CheckInEntry>();
    }

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        await InitAsync();
        return await _database!
            .Table<TaskItem>()
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskAsync(string id)
    {
        await InitAsync();
        return await _database!
            .Table<TaskItem>()
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveTaskAsync(TaskItem task)
    {
        await InitAsync();

        if (string.IsNullOrWhiteSpace(task.Id))
        {
            task.Id = Guid.NewGuid().ToString();
        }

        if (task.CreatedUtc == default)
        {
            task.CreatedUtc = DateTime.UtcNow;
        }

        return await _database!.InsertOrReplaceAsync(task);
    }

    public async Task<int> DeleteTaskAsync(TaskItem task)
    {
        await InitAsync();
        return await _database!.DeleteAsync(task);
    }
    public async Task<int> SaveWinAsync(WinEntry win)
    {
        await InitAsync();

        if (string.IsNullOrWhiteSpace(win.Id))
        {
            win.Id = Guid.NewGuid().ToString();
        }

        if (win.TimestampUtc == default)
        {
            win.TimestampUtc = DateTime.UtcNow;
        }

        return await _database!.InsertOrReplaceAsync(win);
    }

    public async Task<List<WinEntry>> GetWinsAsync()
    {
        await InitAsync();

        return await _database!
            .Table<WinEntry>()
            .ToListAsync();
    }
    public async Task<int> SaveCheckInAsync(CheckInEntry entry)
    {
        await InitAsync();

        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            entry.Id = Guid.NewGuid().ToString();
        }

        if (entry.TimestampUtc == default)
        {
            entry.TimestampUtc = DateTime.UtcNow;
        }

        return await _database!.InsertOrReplaceAsync(entry);
    }

    public async Task<List<CheckInEntry>> GetCheckInsAsync()
    {
        await InitAsync();

        return await _database!
            .Table<CheckInEntry>()
            .ToListAsync();
    }
}