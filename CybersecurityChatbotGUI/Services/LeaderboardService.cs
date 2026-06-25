using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;

namespace CybersecurityChatbotGUI.Services
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public DateTime PlayedDate { get; set; }
    }

    public class LeaderboardService
    {
        private readonly string connectionString;

        public LeaderboardService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["ChatbotDB"]?.ConnectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Add a new score to the leaderboard
        public void AddScore(string playerName, int score, int totalQuestions)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Insert new score
                    string insertQuery = @"INSERT INTO Leaderboard (PlayerName, Score, TotalQuestions, Percentage, PlayedDate) 
                                           VALUES (@PlayerName, @Score, @TotalQuestions, @Percentage, NOW())";

                    double percentage = (double)score / totalQuestions * 100;

                    using (var cmd = new MySqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@PlayerName", playerName);
                        cmd.Parameters.AddWithValue("@Score", score);
                        cmd.Parameters.AddWithValue("@TotalQuestions", totalQuestions);
                        cmd.Parameters.AddWithValue("@Percentage", Math.Round(percentage, 2));
                        cmd.ExecuteNonQuery();
                    }

                    // Keep only top 10 scores (plus ties)
                    string cleanupQuery = @"
                        DELETE FROM Leaderboard 
                        WHERE ScoreID NOT IN (
                            SELECT ScoreID FROM (
                                SELECT ScoreID FROM Leaderboard ORDER BY Percentage DESC, PlayedDate DESC LIMIT 10
                            ) AS TopScores
                        )";

                    using (var cmd = new MySqlCommand(cleanupQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show($"Leaderboard error: {ex.Message}", "Database Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Get leaderboard with rankings (ties share the same rank)
        public List<LeaderboardEntry> GetLeaderboard()
        {
            var entries = new List<LeaderboardEntry>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = @"SELECT PlayerName, Score, TotalQuestions, Percentage, PlayedDate 
                                     FROM Leaderboard 
                                     ORDER BY Percentage DESC, PlayedDate DESC 
                                     LIMIT 10";

                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var rawEntries = new List<LeaderboardEntry>();
                        while (reader.Read())
                        {
                            rawEntries.Add(new LeaderboardEntry
                            {
                                PlayerName = reader.GetString("PlayerName"),
                                Score = reader.GetInt32("Score"),
                                TotalQuestions = reader.GetInt32("TotalQuestions"),
                                Percentage = reader.GetDouble("Percentage"),
                                PlayedDate = reader.GetDateTime("PlayedDate")
                            });
                        }

                        // Assign ranks with tie handling
                        if (rawEntries.Count > 0)
                        {
                            int currentRank = 1;
                            double previousPercentage = rawEntries[0].Percentage;

                            for (int i = 0; i < rawEntries.Count; i++)
                            {
                                if (rawEntries[i].Percentage < previousPercentage)
                                {
                                    currentRank = i + 1;
                                    previousPercentage = rawEntries[i].Percentage;
                                }
                                rawEntries[i].Rank = currentRank;
                            }

                            entries = rawEntries;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show($"Leaderboard error: {ex.Message}", "Database Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return entries;
        }
    }
}