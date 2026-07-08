namespace ADHDCompanionApp.Services;

public static class ReminderConstants
{
    public const string MedicationReminderKey = "medication_daily";
    public const int MedicationNotificationId = 1001;

    public static string GetTaskReminderKey(string taskId)
    {
        return $"task_{taskId}";
    }

    public static int GetTaskNotificationId(string taskId)
    {
        return Math.Abs(taskId.GetHashCode());
    }

    public const int ReEngagementNotificationIdBase = 900000;

    public static string GetReEngagementReminderKey(int days)
    {
        return $"arlo_reengagement_{days}_days";
    }
}