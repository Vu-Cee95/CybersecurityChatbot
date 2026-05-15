namespace CybersecurityChatbotGUI.Models
{
    public class ConversationState
    {
        public string CurrentTopic { get; set; } = "";
        public string PreviousTopic { get; set; } = "";
        public string LastIntent { get; set; } = "";
        public int FollowUpCount { get; set; } = 0;
        public int TotalMessages { get; set; } = 0;
    }
}