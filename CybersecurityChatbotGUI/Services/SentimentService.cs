using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class SentimentService
    {
        private readonly Dictionary<string, List<string>> sentimentKeywords = new Dictionary<string, List<string>>
        {
            {
                "worried",
                new List<string>
                {
                    "worried", "scared", "afraid", "nervous", "anxious", "concerned", "fear", "panic"
                }
            },
            {
                "curious",
                new List<string>
                {
                    "curious", "interested", "want to know", "teach me", "explain", "learn", "understand"
                }
            },
            {
                "frustrated",
                new List<string>
                {
                    "frustrated", "angry", "annoyed", "confused", "stuck", "irritated", "do not understand", "don't understand"
                }
            }
        };

        public string DetectSentiment(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            string lowerInput = userInput.ToLower();

            foreach (var sentiment in sentimentKeywords)
            {
                foreach (string keyword in sentiment.Value)
                {
                    if (lowerInput.Contains(keyword))
                    {
                        return sentiment.Key;
                    }
                }
            }

            return "";
        }

        public string GetEmpathyResponse(string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return "It is completely understandable to feel worried. Cyber threats can look convincing, but learning the warning signs will help you stay safer.";

                case "curious":
                    return "That is a great attitude. Being curious about cybersecurity is one of the best ways to protect yourself online.";

                case "frustrated":
                    return "I understand that this can feel frustrating. Cybersecurity can seem confusing at first, but we can break it down into simple steps.";

                default:
                    return "";
            }
        }
    }
}