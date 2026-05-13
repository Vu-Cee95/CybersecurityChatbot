using CybersecurityChatbotGUI.Models;

namespace CybersecurityChatbotGUI.Services
{
    public class ChatbotEngine
    {
        // Delegate used to process the user's input.
        // This supports the Part 2 requirement for delegates.
        public delegate string ResponseHandler(string userInput);

        private readonly ResponseService responseService;
        private readonly KeywordService keywordService;
        private readonly SentimentService sentimentService;

        private readonly UserMemory userMemory;
        private readonly ConversationState conversationState;

        public ResponseHandler ProcessUserMessage;

        public ChatbotEngine()
        {
            responseService = new ResponseService();
            keywordService = new KeywordService();
            sentimentService = new SentimentService();

            userMemory = new UserMemory();
            conversationState = new ConversationState();

            ProcessUserMessage = GenerateResponse;
        }

        private string GenerateResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "Please type something first so I can help you.";
            }

            userInput = userInput.Trim();

            string detectedSentiment = sentimentService.DetectSentiment(userInput);
            string detectedTopic = keywordService.DetectTopic(userInput);

            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                userMemory.LastSentiment = detectedSentiment;
            }

            if (keywordService.IsHelpRequest(userInput))
            {
                return responseService.GetHelpResponse();
            }

            if (IsNameIntroduction(userInput))
            {
                return SaveUserName(userInput);
            }

            if (IsInterestStatement(userInput) && !string.IsNullOrWhiteSpace(detectedTopic))
            {
                userMemory.FavouriteTopic = detectedTopic;
                userMemory.LastTopic = detectedTopic;
                conversationState.CurrentTopic = detectedTopic;

                string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
                string tip = responseService.GetRandomResponse(detectedTopic);

                if (!string.IsNullOrWhiteSpace(empathy))
                {
                    return $"{empathy}\n\nGreat, {userMemory.UserName}. I will remember that you are interested in {detectedTopic}.\n\n{tip}";
                }

                return $"Great, {userMemory.UserName}. I will remember that you are interested in {detectedTopic}.\n\n{tip}";
            }

            if (keywordService.IsFollowUpQuestion(userInput))
            {
                return HandleFollowUp(detectedSentiment);
            }

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                userMemory.LastTopic = detectedTopic;
                conversationState.CurrentTopic = detectedTopic;
                conversationState.FollowUpCount = 0;

                string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
                string topicResponse = responseService.GetRandomResponse(detectedTopic);

                if (!string.IsNullOrWhiteSpace(empathy))
                {
                    return $"{empathy}\n\n{topicResponse}";
                }

                return topicResponse;
            }

            if (IsRecallRequest(userInput))
            {
                return GetMemorySummary();
            }

            return responseService.GetDefaultResponse();
        }

        private string HandleFollowUp(string detectedSentiment)
        {
            if (string.IsNullOrWhiteSpace(conversationState.CurrentTopic))
            {
                return "I can explain more, but first tell me which topic you want to learn about: passwords, phishing, scams, privacy, safe browsing, malware, or 2FA.";
            }

            conversationState.FollowUpCount++;

            string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
            string followUpTip = responseService.GetRandomResponse(conversationState.CurrentTopic);

            if (!string.IsNullOrWhiteSpace(empathy))
            {
                return $"{empathy}\n\nHere is another tip about {conversationState.CurrentTopic}:\n\n{followUpTip}";
            }

            return $"Here is another tip about {conversationState.CurrentTopic}:\n\n{followUpTip}";
        }

        private bool IsNameIntroduction(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.StartsWith("my name is ") ||
                   lowerInput.StartsWith("i am ") ||
                   lowerInput.StartsWith("i'm ");
        }

        private string SaveUserName(string userInput)
        {
            string name = userInput;

            name = name.Replace("My name is", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("I am", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("I'm", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                return "I could not clearly detect your name. Try typing something like: My name is Vusi.";
            }

            // Avoid saving emotional statements as names.
            string lowerName = name.ToLower();

            if (lowerName.Contains("worried") ||
                lowerName.Contains("scared") ||
                lowerName.Contains("curious") ||
                lowerName.Contains("confused") ||
                lowerName.Contains("frustrated") ||
                lowerName.Contains("interested"))
            {
                return "I noticed you may be sharing how you feel instead of your name. You can say something like: My name is Vusi.";
            }

            userMemory.UserName = name;

            return $"Nice to meet you, {userMemory.UserName}. I will remember your name during this chat.";
        }

        private bool IsInterestStatement(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.Contains("interested in") ||
                   lowerInput.Contains("i like") ||
                   lowerInput.Contains("i want to learn about") ||
                   lowerInput.Contains("teach me about");
        }

        private bool IsRecallRequest(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.Contains("what do you remember") ||
                   lowerInput.Contains("what is my name") ||
                   lowerInput.Contains("my favourite topic") ||
                   lowerInput.Contains("what have i told you");
        }

        private string GetMemorySummary()
        {
            string favouriteTopic = string.IsNullOrWhiteSpace(userMemory.FavouriteTopic)
                ? "not set yet"
                : userMemory.FavouriteTopic;

            string lastTopic = string.IsNullOrWhiteSpace(userMemory.LastTopic)
                ? "not discussed yet"
                : userMemory.LastTopic;

            string lastSentiment = string.IsNullOrWhiteSpace(userMemory.LastSentiment)
                ? "not detected yet"
                : userMemory.LastSentiment;

            return $"Here is what I remember:\n\nName: {userMemory.UserName}\nFavourite topic: {favouriteTopic}\nLast topic discussed: {lastTopic}\nLast detected sentiment: {lastSentiment}";
        }
    }
}