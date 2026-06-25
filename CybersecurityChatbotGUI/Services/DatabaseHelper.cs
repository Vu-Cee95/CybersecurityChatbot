using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace CybersecurityChatbotGUI.Services
{
    // Represents a single task record from the database
    public class TaskItem
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // Handles all MySQL database operations for task management
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper()
        {
            // Read connection string from App.config
            connectionString = ConfigurationManager.ConnectionStrings["ChatbotDB"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'ChatbotDB' not found in App.config.");
            }
        }

        // Creates and returns a new MySQL connection
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // CREATE: Inserts a new task into the database
        public bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = @"INSERT INTO Tasks 
                        (Title, Description, ReminderDate, IsCompleted, CreatedDate) 
                        VALUES (@Title, @Description, @ReminderDate, FALSE, NOW())";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description);
                        cmd.Parameters.AddWithValue("@ReminderDate",
                            reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Database Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // READ: Retrieves all tasks ordered by creation date (newest first)
        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = @"SELECT TaskID, Title, Description, ReminderDate, 
                        IsCompleted, CreatedDate FROM Tasks ORDER BY CreatedDate DESC";

                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(MapReaderToTask(reader));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Database Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }

            return tasks;
        }

        // READ: Retrieves only pending (incomplete) tasks
        public List<TaskItem> GetPendingTasks()
        {
            var tasks = new List<TaskItem>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = @"SELECT TaskID, Title, Description, ReminderDate, 
                        IsCompleted, CreatedDate FROM Tasks WHERE IsCompleted = FALSE 
                        ORDER BY CreatedDate DESC";

                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(MapReaderToTask(reader));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Database Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }

            return tasks;
        }

        // UPDATE: Marks a task as completed by its ID
        public bool MarkTaskAsComplete(int taskId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "UPDATE Tasks SET IsCompleted = TRUE WHERE TaskID = @TaskID";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@TaskID", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Database Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // DELETE: Removes a task by its ID
        public bool DeleteTask(int taskId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM Tasks WHERE TaskID = @TaskID";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@TaskID", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Database Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // Tests if the database connection can be established
        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Maps a data reader row to a TaskItem object
        private TaskItem MapReaderToTask(MySqlDataReader reader)
        {
            return new TaskItem
            {
                TaskID = reader.GetInt32("TaskID"),
                Title = reader.GetString("Title"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? string.Empty
                    : reader.GetString("Description"),
                ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate"))
                    ? (DateTime?)null
                    : reader.GetDateTime("ReminderDate"),
                IsCompleted = reader.GetBoolean("IsCompleted"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}