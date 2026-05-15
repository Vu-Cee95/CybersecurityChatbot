using System;
using System.Text;
using CybersecurityChatbotGUI.Models;

namespace CybersecurityChatbotGUI.Services
{
    public class CyberSafetyReportService
    {
        public string GenerateReport(
            UserMemory userMemory,
            ConversationState conversationState,
            ChatHistoryService chatHistoryService)
        {
            string mainTopic = chatHistoryService.GetMainTopic();

            if (string.IsNullOrWhiteSpace(mainTopic))
            {
                mainTopic = string.IsNullOrWhiteSpace(userMemory.LastTopic)
                    ? "Not clearly identified"
                    : userMemory.LastTopic;
            }

            string platform = string.IsNullOrWhiteSpace(userMemory.LastPlatform)
                ? "Not detected"
                : userMemory.LastPlatform;

            string detectedIssue = string.IsNullOrWhiteSpace(userMemory.LastDetectedIssue)
                ? "No specific issue detected"
                : userMemory.LastDetectedIssue;

            string mood = string.IsNullOrWhiteSpace(userMemory.LastSentiment)
                ? "Not detected"
                : userMemory.LastSentiment;

            string highestRisk = chatHistoryService.GetHighestRiskLevel();

            StringBuilder report = new StringBuilder();

            report.AppendLine("CYBER SAFETY REPORT");
            report.AppendLine("--------------------------------------------------");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine($"User: {userMemory.UserName}");
            report.AppendLine();
            report.AppendLine("SESSION OVERVIEW");
            report.AppendLine($"• Main topic: {mainTopic}");
            report.AppendLine($"• Detected mood: {mood}");
            report.AppendLine($"• Current risk level: {userMemory.CurrentRiskLevel}");
            report.AppendLine($"• Highest risk level: {highestRisk}");
            report.AppendLine($"• Detected issue: {detectedIssue}");
            report.AppendLine($"• Platform detected: {platform}");
            report.AppendLine($"• Total messages tracked: {chatHistoryService.MessageCount}");
            report.AppendLine();
            report.AppendLine("ADVICE GIVEN");
            report.AppendLine(chatHistoryService.GetAdviceGivenSummary());
            report.AppendLine();
            report.AppendLine("RECENT CONVERSATION CONTEXT");
            report.AppendLine(chatHistoryService.BuildRecentContextSummary());
            report.AppendLine();
            report.AppendLine("RECOMMENDED NEXT STEP");
            report.AppendLine(BuildRecommendedNextStep(highestRisk, detectedIssue, platform));
            report.AppendLine("--------------------------------------------------");
            report.AppendLine("CyberBot note: This report is for awareness and guidance. For serious financial or account compromise, contact the official service provider immediately.");

            return report.ToString();
        }

        private string BuildRecommendedNextStep(string riskLevel, string detectedIssue, string platform)
        {
            switch (riskLevel)
            {
                case "Emergency":
                    return "Act immediately. Change affected passwords, contact your bank or service provider using official contact details, enable 2FA, and monitor account activity.";

                case "High":
                    return "Secure the affected account or device. Change passwords, enable 2FA, avoid the suspicious message or link, and check recent account activity.";

                case "Medium":
                    return "Do not continue interacting with the suspicious message, link, or file. Verify it through an official website, app, or contact number.";

                default:
                    return "Continue learning safe cyber habits. Use strong passwords, enable 2FA, avoid unknown links, and protect your personal information.";
            }
        }
    }
}