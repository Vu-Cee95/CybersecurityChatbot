// Models namespace contains data structures used across the chatbot application
namespace CybersecurityChatbotGUI.Models
{
    // Stores persistent user information throughout a chat session
    // Acts as the bot's "memory" of the current user, tracking preferences, emotional state,
    // risk levels, detected issues, and conversation statistics
    // All properties have sensible defaults to handle new or unset values gracefully
    public class UserMemory
    {
        // The user's name for personalized responses
        // Defaults to "Cyber Friend" until explicitly set (via welcome page or in-chat introduction)
        public string UserName { get; set; } = "Cyber Friend";

        // The user's favourite cybersecurity topic (if expressed during conversation)
        // Set when user says "I'm interested in..." or similar interest statements
        // Empty string indicates no favourite topic has been set yet
        public string FavouriteTopic { get; set; } = "";

        // The most recently discussed cybersecurity topic
        // Updated every time a new topic is detected or conversation focus changes
        // Used for context tracking and summary generation
        public string LastTopic { get; set; } = "";

        // The last detected emotional sentiment from user input
        // Possible values: "worried", "curious", "frustrated", or empty string
        // Empty string indicates no sentiment has been detected yet
        public string LastSentiment { get; set; } = "";

        // The most recent emergency type detected (if any)
        // Possible values: "clicked link", "shared otp", "shared password", "downloaded file", 
        // "hacked account", "general emergency", or empty string
        // Empty string indicates no emergency has been detected
        public string LastEmergencyType { get; set; } = "";

        // The current cybersecurity risk level for this conversation
        // Possible values: "Low", "Medium", "High", "Emergency"
        // Defaults to "Low" - updated dynamically as new risks are detected
        public string CurrentRiskLevel { get; set; } = "Low";

        // The highest risk level reached during the entire conversation
        // Tracks peak risk regardless of current level returning to normal
        // Used for session summaries and risk trend analysis
        public string HighestRiskLevel { get; set; } = "Low";

        // The most recently detected cybersecurity issue or threat
        // Examples: "suspicious link", "password sharing", "OTP disclosure"
        // Empty string indicates no specific issue has been detected
        public string LastDetectedIssue { get; set; } = "";

        // The most recently mentioned digital platform or service
        // Examples: "WhatsApp", "Facebook", "email", "banking app"
        // Used to provide platform-specific safety examples and guidance
        public string LastPlatform { get; set; } = "";

        // The last type of content the user explicitly requested
        // Possible values: "tip", "example", "checklist", "definition", or empty string
        // Helps the bot remember what kind of response the user prefers
        public string LastIntentRequested { get; set; } = "";

        // Counter tracking how many safety reports have been generated in this session
        // Incremented each time user requests a report ("generate report", "cyber safety report")
        // Defaults to 0 at session start
        public int ReportsGenerated { get; set; } = 0;
    }
}