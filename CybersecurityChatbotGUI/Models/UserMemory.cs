namespace CybersecurityChatbotGUI.Models
{
    public class UserMemory
    {
        public string UserName { get; set; } = "Cyber Friend";
        public string FavouriteTopic { get; set; } = "";
        public string LastTopic { get; set; } = "";
        public string LastSentiment { get; set; } = "";
        public string LastEmergencyType { get; set; } = "";

        public string CurrentRiskLevel { get; set; } = "Low";
        public string LastDetectedIssue { get; set; } = "";
        public string LastPlatform { get; set; } = "";
        public string LastIntentRequested { get; set; } = "";
    }
}