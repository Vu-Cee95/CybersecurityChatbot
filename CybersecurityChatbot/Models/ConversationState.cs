namespace CybersecurityChatbot.Models
{
    // Tracks the state and progression of an ongoing conversation with the user
    internal class ConversationState
    {
        // The current cybersecurity topic being discussed (password, phishing, browsing, etc.)
        public string CurrentTopic { get; set; }
        // The current step number in the 6-step conversation progression flow
        public int Step { get; set; }
        // The most recent input provided by the user for context tracking
        public string LastUserInput { get; set; }
    }
}