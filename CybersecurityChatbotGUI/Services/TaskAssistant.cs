using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CybersecurityChatbotGUI.Services
{
    // Manages task operations and coordinates between NLP, database, and activity logging
    public class TaskAssistant
    {
        private readonly DatabaseHelper dbHelper;
        private readonly ActivityLogger activityLogger;

        public TaskAssistant(DatabaseHelper databaseHelper, ActivityLogger logger)
        {
            dbHelper = databaseHelper;
            activityLogger = logger;
        }

        // Adds a new task with optional description and reminder date
        public string AddTask(string title, string description = "", DateTime? reminderDate = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Please provide a title for the task.";

            bool success = dbHelper.AddTask(title, description, reminderDate);

            if (success)
            {
                string reminderText = reminderDate.HasValue
                    ? $" (Reminder set for {reminderDate.Value:yyyy-MM-dd HH:mm})"
                    : string.Empty;

                activityLogger.LogActivity(
                    $"Task added: '{title}'{reminderText}", "Task");

                return $"Task '{title}' added successfully.{reminderText}";
            }

            return "Failed to add task. Please try again.";
        }

        // Returns a formatted list of all tasks
        public string ViewAllTasks()
        {
            var tasks = dbHelper.GetAllTasks();

            if (tasks.Count == 0)
                return "You have no tasks yet. Type 'Add task to [description]' to create one.";

            string result = "Your Tasks:\n\n";

            foreach (var task in tasks)
            {
                string status = task.IsCompleted ? "[X]" : "[ ]";
                string reminder = task.ReminderDate.HasValue
                    ? $" | Reminder: {task.ReminderDate.Value:yyyy-MM-dd HH:mm}"
                    : string.Empty;

                result += $"{status} [Task {task.TaskID}] {task.Title}{reminder}\n";

                if (!string.IsNullOrWhiteSpace(task.Description))
                    result += $"    Description: {task.Description}\n";
            }

            result += "\nType 'Complete task [number]' or 'Delete task [number]' to manage tasks.";

            return result;
        }

        // Marks a task as completed by its ID
        public string CompleteTask(int taskId)
        {
            var tasks = dbHelper.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.TaskID == taskId);

            if (task == null)
                return $"Task {taskId} not found.";

            if (task.IsCompleted)
                return $"Task '{task.Title}' is already marked as complete.";

            bool success = dbHelper.MarkTaskAsComplete(taskId);

            if (success)
            {
                activityLogger.LogActivity($"Task completed: '{task.Title}'", "Task");
                return $"Task '{task.Title}' marked as complete.";
            }

            return "Failed to mark task as complete.";
        }

        // Deletes a task by its ID
        public string DeleteTask(int taskId)
        {
            var tasks = dbHelper.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.TaskID == taskId);

            if (task == null)
                return $"Task {taskId} not found.";

            bool success = dbHelper.DeleteTask(taskId);

            if (success)
            {
                activityLogger.LogActivity($"Task deleted: '{task.Title}'", "Task");
                return $"Task '{task.Title}' deleted successfully.";
            }

            return "Failed to delete task.";
        }

        // Parses reminder date from user input
        // Supports: "in X days", "in X weeks", "tomorrow"
        public DateTime? ParseReminderDate(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            // Match "Remind me in X days"
            var daysMatch = Regex.Match(userInput, @"remind me in (\d+)\s*days?", RegexOptions.IgnoreCase);
            if (daysMatch.Success)
            {
                int days = int.Parse(daysMatch.Groups[1].Value);
                return DateTime.Now.AddDays(days);
            }

            // Match "Remind me in X weeks"
            var weeksMatch = Regex.Match(userInput, @"remind me in (\d+)\s*weeks?", RegexOptions.IgnoreCase);
            if (weeksMatch.Success)
            {
                int weeks = int.Parse(weeksMatch.Groups[1].Value);
                return DateTime.Now.AddDays(weeks * 7);
            }

            // Match "Remind me tomorrow"
            if (Regex.IsMatch(userInput, @"remind me tomorrow", RegexOptions.IgnoreCase))
                return DateTime.Now.AddDays(1);

            // Match "Remind me on [date]"
            var dateMatch = Regex.Match(userInput, @"remind me on (\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase);
            if (dateMatch.Success)
            {
                if (DateTime.TryParse(dateMatch.Groups[1].Value, out DateTime parsedDate))
                    return parsedDate;
            }

            return null;
        }
    }
}