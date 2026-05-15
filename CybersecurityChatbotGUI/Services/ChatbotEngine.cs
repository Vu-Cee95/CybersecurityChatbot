using CybersecurityChatbotGUI.Models;

namespace CybersecurityChatbotGUI.Services
{
    public class ChatbotEngine
    {
        private delegate string ResponseHandler(string userInput);

        private readonly ResponseService responseService;
        private readonly KeywordService keywordService;
        private readonly SentimentService sentimentService;
        private readonly PersonalityService personalityService;
        private readonly InputNormaliserService inputNormaliserService;

        private readonly UserMemory userMemory;
        private readonly ConversationState conversationState;

        private readonly ResponseHandler responseHandler;

        public ChatbotEngine()
        {
            responseService = new ResponseService();
            keywordService = new KeywordService();
            sentimentService = new SentimentService();
            personalityService = new PersonalityService();
            inputNormaliserService = new InputNormaliserService();

            userMemory = new UserMemory();
            conversationState = new ConversationState();

            responseHandler = GenerateResponse;
        }

        public string LastTopicDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastTopic) ? "None" : userMemory.LastTopic;
            }
        }

        public string LastSentimentDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastSentiment) ? "Not detected" : userMemory.LastSentiment;
            }
        }

        public string ProcessMessage(string userInput)
        {
            return responseHandler(userInput);
        }

        public void SetUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                userMemory.UserName = name.Trim();
            }
        }

        public void ResetConversationButKeepUser()
        {
            conversationState.CurrentTopic = "";
            conversationState.PreviousTopic = "";
            conversationState.LastIntent = "";
            conversationState.FollowUpCount = 0;
            conversationState.TotalMessages = 0;

            userMemory.LastTopic = "";
            userMemory.LastSentiment = "";
            userMemory.LastEmergencyType = "";
        }

        private string GenerateResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "Please type a message first.";
            }

            userInput = userInput.Trim();

            string normalisedInput = inputNormaliserService.Normalise(userInput);

            conversationState.TotalMessages++;

            string detectedSentiment = sentimentService.DetectSentiment(normalisedInput);
            string detectedTopic = keywordService.DetectTopic(normalisedInput);
            string detectedIntent = keywordService.DetectIntent(normalisedInput);
            string emergencyType = keywordService.DetectEmergencyType(normalisedInput);

            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                userMemory.LastSentiment = detectedSentiment;
            }

            conversationState.LastIntent = detectedIntent;

            string baseResponse;

            if (keywordService.IsHelpRequest(normalisedInput))
            {
                baseResponse = responseService.GetHelpResponse();

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "help",
                    detectedSentiment,
                    baseResponse
                );
            }

            if (detectedIntent == "summary")
            {
                baseResponse = responseService.GetSessionSummary(
                    userMemory.UserName,
                    userMemory.FavouriteTopic,
                    userMemory.LastTopic,
                    userMemory.LastSentiment,
                    conversationState.TotalMessages
                );

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse
                );
            }

            if (IsRecallRequest(normalisedInput))
            {
                baseResponse = GetMemorySummary();

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "recall",
                    detectedSentiment,
                    baseResponse
                );
            }

            if (IsNameIntroduction(normalisedInput))
            {
                baseResponse = SaveUserNameFromConversation(userInput);

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "name",
                    detectedSentiment,
                    baseResponse
                );
            }

            if (detectedIntent == "emergency")
            {
                userMemory.LastEmergencyType = emergencyType;

                if (!string.IsNullOrWhiteSpace(detectedTopic))
                {
                    UpdateTopic(detectedTopic);
                }

                baseResponse = responseService.GetEmergencyResponse(emergencyType);

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse
                );
            }

            if (IsInterestStatement(normalisedInput) && !string.IsNullOrWhiteSpace(detectedTopic))
            {
                baseResponse = SaveFavouriteTopic(detectedTopic, detectedSentiment);

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "interest",
                    detectedSentiment,
                    baseResponse
                );
            }

            if (detectedIntent == "follow-up")
            {
                baseResponse = HandleFollowUp(detectedSentiment);

                return BuildSmartResponse(
                    userInput,
                    conversationState.CurrentTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse
                );
            }

            if (string.IsNullOrWhiteSpace(detectedTopic) &&
                !string.IsNullOrWhiteSpace(conversationState.CurrentTopic) &&
                (detectedIntent == "definition" || detectedIntent == "prevention" || detectedIntent == "example"))
            {
                detectedTopic = conversationState.CurrentTopic;
            }

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                baseResponse = HandleTopicResponse(detectedTopic, detectedIntent, detectedSentiment);

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse
                );
            }

            baseResponse = responseService.GetDefaultResponse();

            return BuildSmartResponse(
                userInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                baseResponse
            );
        }

        private string BuildSmartResponse(
            string userInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment,
            string baseResponse)
        {
            return personalityService.BuildPersonalisedResponse(
                userMemory.UserName,
                userInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                baseResponse,
                conversationState.TotalMessages,
                userMemory.FavouriteTopic,
                conversationState.FollowUpCount
            );
        }

        private string HandleTopicResponse(string detectedTopic, string detectedIntent, string detectedSentiment)
        {
            UpdateTopic(detectedTopic);

            string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
            string response = responseService.GetTopicResponse(detectedTopic, detectedIntent);

            if (!string.IsNullOrWhiteSpace(empathy))
            {
                return $"{empathy}\n\n{response}";
            }

            return response;
        }

        private void UpdateTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(conversationState.CurrentTopic))
            {
                conversationState.PreviousTopic = conversationState.CurrentTopic;
            }

            conversationState.CurrentTopic = topic;
            conversationState.FollowUpCount = 0;

            userMemory.LastTopic = topic;
        }

        private string SaveFavouriteTopic(string detectedTopic, string detectedSentiment)
        {
            userMemory.FavouriteTopic = detectedTopic;
            UpdateTopic(detectedTopic);

            string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
            string tip = responseService.GetTopicResponse(detectedTopic, "general");

            if (!string.IsNullOrWhiteSpace(empathy))
            {
                return $"{empathy}\n\nGreat, {userMemory.UserName}. I will remember that you are interested in {detectedTopic}.\n\n{tip}";
            }

            return $"Great, {userMemory.UserName}. I will remember that you are interested in {detectedTopic}.\n\n{tip}";
        }

        private string HandleFollowUp(string detectedSentiment)
        {
            if (string.IsNullOrWhiteSpace(conversationState.CurrentTopic))
            {
                return "I can explain more, but first tell me which topic you want to learn about: passwords, phishing, scams, privacy, safe browsing, malware, or 2FA.";
            }

            conversationState.FollowUpCount++;

            string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
            string followUpTip = responseService.GetTopicResponse(conversationState.CurrentTopic, "general");

            if (!string.IsNullOrWhiteSpace(empathy))
            {
                return $"{empathy}\n\nHere is more about {conversationState.CurrentTopic}:\n\n{followUpTip}";
            }

            return $"Here is more about {conversationState.CurrentTopic}:\n\n{followUpTip}";
        }

        private bool IsNameIntroduction(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.StartsWith("my name is ") ||
                   lowerInput.StartsWith("call me ") ||
                   lowerInput.StartsWith("you can call me ");
        }

        private string SaveUserNameFromConversation(string userInput)
        {
            string name = userInput;

            name = name.Replace("My name is", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("Call me", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("You can call me", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            {
                return "I could not clearly detect your name. Try typing something like: My name is Vusi.";
            }

            bool hasLetter = false;

            foreach (char character in name)
            {
                if (char.IsLetter(character))
                {
                    hasLetter = true;
                    break;
                }
            }

            if (!hasLetter)
            {
                return "That name does not look valid. Please use a name with at least one letter.";
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
                   lowerInput.Contains("teach me about") ||
                   lowerInput.Contains("i want to know about") ||
                   lowerInput.Contains("i care about");
        }

        private bool IsRecallRequest(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.Contains("what do you remember") ||
                   lowerInput.Contains("what is my name") ||
                   lowerInput.Contains("do you remember my name") ||
                   lowerInput.Contains("my favourite topic") ||
                   lowerInput.Contains("my favorite topic") ||
                   lowerInput.Contains("what have i told you") ||
                   lowerInput.Contains("what topic do i like") ||
                   lowerInput.Contains("what was my last topic");
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

            string response =
                $"I remember that your name is {userMemory.UserName}.\n\n" +
                $"• Favourite cybersecurity topic: {favouriteTopic}\n" +
                $"• Last topic discussed: {lastTopic}\n" +
                $"• Last mood detected: {lastSentiment}";

            if (favouriteTopic != "not set yet")
            {
                response += $"\n\nSince you are interested in {favouriteTopic}, I can keep giving you useful tips about that topic.";
            }

            return response;
        }
    }
}