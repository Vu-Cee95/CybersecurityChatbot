namespace CybersecurityChatbotGUI.Models
{
    public class ConversationState
    {
        public string CurrentTopic { get; set; } = "";
        public int FollowUpCount { get; set; } = 0;
    }
}