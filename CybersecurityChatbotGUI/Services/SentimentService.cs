// Required for Dictionary and List collections used for sentiment keyword mapping
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    // Service that detects emotional sentiment from user messages
    // Analyzes text for emotional keywords to understand user's emotional state
    // Provides empathetic responses tailored to detected sentiments
    public class SentimentService
    {
        // Dictionary mapping emotional sentiments to their associated keywords
        // Each sentiment has a list of words/phrases that indicate that emotional state
        // Three main sentiments tracked: worried, curious, frustrated
        private readonly Dictionary<string, List<string>> sentimentKeywords = new Dictionary<string, List<string>>
        {
            // Worried sentiment keywords: fear, anxiety, concern about cybersecurity threats
            {
                "worried",
                new List<string>
                {
                    "worried", "scared", "afraid", "nervous", "anxious", "concerned", "fear", "panic"
                }
            },
            // Curious sentiment keywords: interest in learning, seeking understanding
            {
                "curious",
                new List<string>
                {
                    "curious", "interested", "want to know", "teach me", "explain", "learn", "understand"
                }
            },
            // Frustrated sentiment keywords: confusion, anger, difficulty understanding
            {
                "frustrated",
                new List<string>
                {
                    "frustrated", "angry", "annoyed", "confused", "stuck", "irritated", "do not understand", "don't understand"
                }
            }
        };

        // Detects the emotional sentiment in user input
        // Iterates through all sentiment keywords and returns the first matching sentiment
        // Returns empty string if no sentiment is detected
        public string DetectSentiment(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check each sentiment's keywords against the input
            foreach (var sentiment in sentimentKeywords)
            {
                foreach (string keyword in sentiment.Value)
                {
                    // Return first matching sentiment
                    if (lowerInput.Contains(keyword))
                    {
                        return sentiment.Key;
                    }
                }
            }

            // No sentiment detected
            return "";
        }

        // Returns an empathetic response based on detected sentiment
        // Each sentiment has a tailored message that acknowledges the user's feelings
        // Provides reassurance and encouragement appropriate to the emotional state
        // Returns empty string for unrecognized sentiments
        public string GetEmpathyResponse(string sentiment)
        {
            // Select appropriate empathy message based on sentiment type
            switch (sentiment)
            {
                // User is worried/scared: acknowledge fear and offer reassurance
                case "worried":
                    return "It is completely understandable to feel worried. Cyber threats can look convincing, but learning the warning signs will help you stay safer.";

                // User is curious/interested: encourage their learning attitude
                case "curious":
                    return "That is a great attitude. Being curious about cybersecurity is one of the best ways to protect yourself online.";

                // User is frustrated/confused: acknowledge difficulty and offer simplification
                case "frustrated":
                    return "I understand that this can feel frustrating. Cybersecurity can seem confusing at first, but we can break it down into simple steps.";

                // Unknown or empty sentiment: no empathy response needed
                default:
                    return "";
            }
        }
    }
}