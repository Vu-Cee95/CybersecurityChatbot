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
                    "password", "passcode", "login", "credentials", "strong password", "weak password",
                    "pin", "account password"
                }
            },
            {
                "phishing",
                new List<string>
                {
                    "phishing", "fake email", "email scam", "suspicious email", "fake message",
                    "fake link", "click link", "clicked a link", "suspicious link"
                }
            },
            {
                "scam",
                new List<string>
                {
                    "scam", "fraud", "fraudster", "online scam", "otp", "banking scam",
                    "fake prize", "lottery", "giveaway", "money request"
                }
            },
            {
                "privacy",
                new List<string>
                {
                    "privacy", "private information", "personal information", "data", "settings",
                    "social media", "id number", "home address", "phone number"
                }
            },
            {
                "safe browsing",
                new List<string>
                {
                    "safe browsing", "browser", "website", "web page", "download",
                    "https", "pop-up", "popup", "unsafe site"
                }
            },
            {
                "malware",
                new List<string>
                {
                    "malware", "virus", "trojan", "spyware", "ransomware", "infected",
                    "harmful software", "downloaded a file", "unknown file"
                }
            },
            {
                "2fa",
                new List<string>
                {
                    "2fa", "two factor", "two-factor", "multi factor", "mfa",
                    "authenticator", "verification code", "one time pin", "one-time pin"
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

        public string DetectIntent(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            string lowerInput = userInput.ToLower();

            if (lowerInput.Contains("summary") ||
                lowerInput.Contains("summarise") ||
                lowerInput.Contains("summarize") ||
                lowerInput.Contains("recap"))
            {
                return "summary";
            }

            if (IsEmergencyInput(lowerInput))
            {
                return "emergency";
            }

            if (lowerInput.Contains("what is") ||
                lowerInput.Contains("define") ||
                lowerInput.Contains("meaning of") ||
                lowerInput.Contains("explain what"))
            {
                return "definition";
            }

            if (lowerInput.Contains("how do i") ||
                lowerInput.Contains("how can i") ||
                lowerInput.Contains("prevent") ||
                lowerInput.Contains("avoid") ||
                lowerInput.Contains("protect") ||
                lowerInput.Contains("stay safe"))
            {
                return "prevention";
            }

            if (lowerInput.Contains("example") ||
                lowerInput.Contains("show me") ||
                lowerInput.Contains("scenario"))
            {
                return "example";
            }

            if (IsFollowUpQuestion(userInput))
            {
                return "follow-up";
            }

            return "general";
        }

        public bool IsEmergencyInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            string lowerInput = userInput.ToLower();

            return lowerInput.Contains("clicked") ||
                   lowerInput.Contains("gave my password") ||
                   lowerInput.Contains("shared my password") ||
                   lowerInput.Contains("shared my otp") ||
                   lowerInput.Contains("gave my otp") ||
                   lowerInput.Contains("hacked") ||
                   lowerInput.Contains("compromised") ||
                   lowerInput.Contains("downloaded") ||
                   lowerInput.Contains("infected") ||
                   lowerInput.Contains("stolen") ||
                   lowerInput.Contains("lost my phone");
        }

        public string DetectEmergencyType(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            string lowerInput = userInput.ToLower();

            if (lowerInput.Contains("otp") || lowerInput.Contains("one time pin") || lowerInput.Contains("one-time pin"))
            {
                return "shared otp";
            }

            if (lowerInput.Contains("password") || lowerInput.Contains("login") || lowerInput.Contains("credentials"))
            {
                return "shared password";
            }

            if (lowerInput.Contains("clicked") || lowerInput.Contains("link"))
            {
                return "clicked link";
            }

            if (lowerInput.Contains("downloaded") || lowerInput.Contains("virus") || lowerInput.Contains("malware") || lowerInput.Contains("infected"))
            {
                return "downloaded file";
            }

            if (lowerInput.Contains("hacked") || lowerInput.Contains("compromised"))
            {
                return "hacked account";
            }

            return "general emergency";
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