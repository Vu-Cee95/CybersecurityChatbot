using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbotGUI.Services
{
    // Represents a single activity log entry
    public class ActivityEntry
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; }
    }

    // Tracks and manages all user actions during the session
    public class ActivityLogger
    {
        private readonly List<ActivityEntry> logEntries;

        public ActivityLogger()
        {
            logEntries = new List<ActivityEntry>();
        }

        // Records a new activity entry
        public void LogActivity(string description, string actionType)
        {
            logEntries.Add(new ActivityEntry
            {
                Description = description,
                Timestamp = DateTime.Now,
                ActionType = actionType
            });
        }

        // Returns all log entries
        public List<ActivityEntry> GetAllEntries()
        {
            return logEntries;
        }

        // Returns the most recent entries (default: last 10)
        public List<ActivityEntry> GetLastEntries(int count = 10)
        {
            return logEntries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .Reverse()
                .ToList();
        }

        // Returns entries filtered by action type
        public List<ActivityEntry> GetEntriesByType(string actionType)
        {
            return logEntries
                .Where(e => e.ActionType == actionType)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        // Generates a formatted string of recent activity
        public string GetFormattedLog(int count = 10)
        {
            var entries = GetLastEntries(count);

            if (entries.Count == 0)
                return "No activities recorded yet in this session.";

            string log = $"Activity Log (Last {entries.Count} actions):\n\n";

            int index = 1;
            foreach (var entry in entries)
            {
                string prefix = entry.ActionType switch
                {
                    "Task" => "[Task]",
                    "Reminder" => "[Reminder]",
                    "Quiz" => "[Quiz]",
                    "NLP" => "[NLP]",
                    "System" => "[System]",
                    _ => "[Action]"
                };

                log += $"{index}. [{entry.Timestamp:HH:mm}] {prefix} {entry.Description}\n";
                index++;
            }

            return log;
        }

        // Clears all log entries
        public void Clear()
        {
            logEntries.Clear();
        }

        // Returns the total number of logged entries
        public int GetCount()
        {
            return logEntries.Count;
        }
    }
}