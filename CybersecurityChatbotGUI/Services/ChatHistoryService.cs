using System.Collections.Generic;
using System.Linq;
using System.Text;
using CybersecurityChatbotGUI.Models;

namespace CybersecurityChatbotGUI.Services
{
    public class ChatHistoryService
    {
        private readonly List<ChatHistoryItem> chatHistory = new List<ChatHistoryItem>();

        public int MessageCount
        {
            get
            {
                return chatHistory.Count;
            }
        }

        public void AddUserMessage(string message, string topic, string intent, string riskLevel)
        {
            chatHistory.Add(new ChatHistoryItem
            {
                Sender = "User",
                Message = message,
                Topic = topic,
                Intent = intent,
                RiskLevel = riskLevel
            });
        }

        public void AddBotMessage(string message, string topic, string intent, string riskLevel)
        {
            chatHistory.Add(new ChatHistoryItem
            {
                Sender = "CyberBot",
                Message = message,
                Topic = topic,
                Intent = intent,
                RiskLevel = riskLevel
            });
        }

        public void Clear()
        {
            chatHistory.Clear();
        }

        public List<ChatHistoryItem> GetAllMessages()
        {
            return chatHistory.ToList();
        }

        public List<ChatHistoryItem> GetRecentMessages(int count)
        {
            return chatHistory
                .TakeLast(count)
                .ToList();
        }

        public string BuildRecentContextSummary()
        {
            List<ChatHistoryItem> recentMessages = GetRecentMessages(6);

            if (recentMessages.Count == 0)
            {
                return "No recent chat history available yet.";
            }

            StringBuilder summary = new StringBuilder();

            foreach (ChatHistoryItem item in recentMessages)
            {
                summary.AppendLine($"• {item.Sender}: {item.Message}");
            }

            return summary.ToString().Trim();
        }

        public string DetectContextRiskEscalation(string normalisedInput, string currentRiskLevel)
        {
            string recentContext = string.Join(" ",
                chatHistory
                    .TakeLast(8)
                    .Select(item => item.Message.ToLower()));

            string combinedContext = recentContext + " " + normalisedInput.ToLower();

            if (ContainsAny(combinedContext,
                    "clicked a link",
                    "opened a link",
                    "suspicious link") &&
                ContainsAny(combinedContext,
                    "entered my password",
                    "gave my password",
                    "shared my password",
                    "typed my password"))
            {
                return "High";
            }

            if (ContainsAny(combinedContext,
                    "clicked a link",
                    "opened a link",
                    "suspicious link") &&
                ContainsAny(combinedContext,
                    "otp",
                    "one time password",
                    "pin",
                    "banking password",
                    "money is gone",
                    "money was taken"))
            {
                return "Emergency";
            }

            if (ContainsAny(combinedContext,
                    "downloaded a file",
                    "installed something",
                    "opened an attachment") &&
                ContainsAny(combinedContext,
                    "device slow",
                    "pop up",
                    "strange app",
                    "account hacked"))
            {
                return "High";
            }

            return currentRiskLevel;
        }

        public string GetMainTopic()
        {
            string topic = chatHistory
                .Where(item => !string.IsNullOrWhiteSpace(item.Topic))
                .GroupBy(item => item.Topic)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault() ?? "";

            return topic;
        }

        public string GetHighestRiskLevel()
        {
            if (chatHistory.Any(item => item.RiskLevel == "Emergency"))
            {
                return "Emergency";
            }

            if (chatHistory.Any(item => item.RiskLevel == "High"))
            {
                return "High";
            }

            if (chatHistory.Any(item => item.RiskLevel == "Medium"))
            {
                return "Medium";
            }

            return "Low";
        }

        public string GetAdviceGivenSummary()
        {
            List<string> advice = new List<string>();

            string fullHistory = string.Join(" ", chatHistory.Select(item => item.Message.ToLower()));

            if (fullHistory.Contains("password"))
            {
                advice.Add("Use strong, unique passwords and avoid reusing them across accounts.");
            }

            if (fullHistory.Contains("2fa") || fullHistory.Contains("otp") || fullHistory.Contains("one time password"))
            {
                advice.Add("Enable two-factor authentication and never share OTP codes.");
            }

            if (fullHistory.Contains("phishing") || fullHistory.Contains("link"))
            {
                advice.Add("Avoid clicking unknown links and verify senders before responding.");
            }

            if (fullHistory.Contains("bank") || fullHistory.Contains("money") || fullHistory.Contains("payment"))
            {
                advice.Add("Contact your bank through official channels if banking details or money are involved.");
            }

            if (fullHistory.Contains("malware") || fullHistory.Contains("download") || fullHistory.Contains("file"))
            {
                advice.Add("Avoid unknown downloads and scan your device with trusted security software.");
            }

            if (advice.Count == 0)
            {
                advice.Add("Think before clicking, protect personal information, and verify suspicious messages.");
            }

            StringBuilder builder = new StringBuilder();

            foreach (string item in advice.Distinct())
            {
                builder.AppendLine($"• {item}");
            }

            return builder.ToString().Trim();
        }

        private bool ContainsAny(string input, params string[] phrases)
        {
            foreach (string phrase in phrases)
            {
                if (input.Contains(phrase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}