using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class KeywordService
    {
        private readonly Dictionary<string, List<string>> keywordMap = new Dictionary<string, List<string>>
        {
            {
                "password",
                new List<string>
                {
                    "password", "passcode", "login", "credentials", "strong password", "weak password"
                }
            },
            {
                "phishing",
                new List<string>
                {
                    "phishing", "fake email", "email scam", "suspicious email", "fake message"
                }
            },
            {
                "scam",
                new List<string>
                {
                    "scam", "fraud", "fraudster", "online scam", "otp", "banking scam", "fake prize"
                }
            },
            {
                "privacy",
                new List<string>
                {
                    "privacy", "private information", "personal information", "data", "settings", "social media"
                }
            },
            {
                "safe browsing",
                new List<string>
                {
                    "safe browsing", "browser", "website", "link", "suspicious link", "https", "pop-up", "download"
                }
            },
            {
                "malware",
                new List<string>
                {
                    "malware", "virus", "trojan", "spyware", "ransomware", "infected", "harmful software"
                }
            },
            {
                "2fa",
                new List<string>
                {
                    "2fa", "two factor", "two-factor", "multi factor", "mfa", "authenticator", "verification code"
                }
            }
        };

        public string DetectTopic(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            string lowerInput = userInput.ToLower();

            foreach (var topic in keywordMap)
            {
                foreach (string keyword in topic.Value)
                {
                    if (lowerInput.Contains(keyword))
                    {
                        return topic.Key;
                    }
                }
            }

            return "";
        }

        public bool IsFollowUpQuestion(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            string lowerInput = userInput.ToLower();

            List<string> followUps = new List<string>
            {
                "tell me more",
                "explain more",
                "more",
                "another tip",
                "another one",
                "give me another",
                "i do not understand",
                "i don't understand",
                "explain",
                "continue"
            };

            foreach (string phrase in followUps)
            {
                if (lowerInput.Contains(phrase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsHelpRequest(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            string lowerInput = userInput.ToLower();

            return lowerInput.Contains("help") ||
                   lowerInput.Contains("what can i ask") ||
                   lowerInput.Contains("what do you do") ||
                   lowerInput.Contains("purpose");
        }
    }
}