using System;

namespace CybersecurityChatbotGUI.Models
{
    public class ChatHistoryItem
    {
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string Topic { get; set; } = "";
        public string Intent { get; set; } = "";
        public string RiskLevel { get; set; } = "";
    }
}