namespace CybersecurityChatbotGUI.Models
{
    public class ConversationState
    {
        public string CurrentTopic { get; set; } = "";
        public string PreviousTopic { get; set; } = "";
        public string LastIntent { get; set; } = "";

        public int FollowUpCount { get; set; } = 0;
        public int TotalMessages { get; set; } = 0;

        public bool IsWaitingForChoice { get; set; } = false;
        public string PendingTopic { get; set; } = "";
        public string PendingQuestionType { get; set; } = "";
        public string PendingOptions { get; set; } = "";

        public bool IsWaitingForClarification { get; set; } = false;
        public string ClarificationReason { get; set; } = "";
        public string LastBotOffer { get; set; } = "";
    }
}