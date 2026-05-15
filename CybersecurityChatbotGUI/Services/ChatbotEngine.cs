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
        private readonly ContextChoiceService contextChoiceService;
        private readonly RiskLevelService riskLevelService;
        private readonly PlatformExampleService platformExampleService;
        private readonly ClarifyingQuestionService clarifyingQuestionService;
        private readonly ChatHistoryService chatHistoryService;
        private readonly CyberSafetyReportService cyberSafetyReportService;

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
            contextChoiceService = new ContextChoiceService();
            riskLevelService = new RiskLevelService();
            platformExampleService = new PlatformExampleService();
            clarifyingQuestionService = new ClarifyingQuestionService();
            chatHistoryService = new ChatHistoryService();
            cyberSafetyReportService = new CyberSafetyReportService();

            userMemory = new UserMemory();
            conversationState = new ConversationState();

            responseHandler = GenerateResponse;
        }

        public string LastTopicDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastTopic)
                    ? "None"
                    : userMemory.LastTopic;
            }
        }

        public string LastSentimentDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastSentiment)
                    ? "Not detected"
                    : userMemory.LastSentiment;
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

            conversationState.IsWaitingForChoice = false;
            conversationState.PendingTopic = "";
            conversationState.PendingQuestionType = "";
            conversationState.PendingOptions = "";

            conversationState.IsWaitingForClarification = false;
            conversationState.ClarificationReason = "";
            conversationState.LastBotOffer = "";

            userMemory.LastTopic = "";
            userMemory.LastSentiment = "";
            userMemory.LastEmergencyType = "";
            userMemory.CurrentRiskLevel = "Low";
            userMemory.HighestRiskLevel = "Low";
            userMemory.LastDetectedIssue = "";
            userMemory.LastPlatform = "";
            userMemory.LastIntentRequested = "";
            userMemory.ReportsGenerated = 0;

            chatHistoryService.Clear();
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
            string riskLevel = riskLevelService.DetectRiskLevel(normalisedInput);
            string detectedIssue = riskLevelService.DetectIssue(normalisedInput);
            string detectedPlatform = platformExampleService.DetectPlatform(normalisedInput);

            riskLevel = chatHistoryService.DetectContextRiskEscalation(normalisedInput, riskLevel);

            chatHistoryService.AddUserMessage(
                userInput,
                detectedTopic,
                detectedIntent,
                riskLevel);

            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                userMemory.LastSentiment = detectedSentiment;
            }

            if (!string.IsNullOrWhiteSpace(detectedPlatform))
            {
                userMemory.LastPlatform = detectedPlatform;
            }

            if (!string.IsNullOrWhiteSpace(detectedIssue))
            {
                userMemory.LastDetectedIssue = detectedIssue;
            }

            userMemory.CurrentRiskLevel = riskLevel;
            userMemory.HighestRiskLevel = chatHistoryService.GetHighestRiskLevel();

            if (!string.IsNullOrWhiteSpace(detectedIntent))
            {
                userMemory.LastIntentRequested = detectedIntent;
            }

            conversationState.LastIntent = detectedIntent;

            if (IsReportRequest(normalisedInput))
            {
                userMemory.ReportsGenerated++;

                string report = cyberSafetyReportService.GenerateReport(
                    userMemory,
                    conversationState,
                    chatHistoryService);

                ClearPendingChoice();

                return BuildSmartResponse(
                    userInput,
                    userMemory.LastTopic,
                    "report",
                    detectedSentiment,
                    report);
            }

            if (conversationState.IsWaitingForClarification)
            {
                string clarificationResponse = HandleClarificationAnswer(
                    userInput,
                    normalisedInput,
                    detectedSentiment,
                    detectedTopic,
                    detectedIntent,
                    riskLevel,
                    detectedIssue);

                if (!string.IsNullOrWhiteSpace(clarificationResponse))
                {
                    return clarificationResponse;
                }
            }

            string contextualChoiceResponse = TryHandleContextualChoice(
                userInput,
                normalisedInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment);

            if (!string.IsNullOrWhiteSpace(contextualChoiceResponse))
            {
                return contextualChoiceResponse;
            }

            if (clarifyingQuestionService.NeedsClarification(
                    normalisedInput,
                    detectedTopic,
                    detectedIntent,
                    riskLevel))
            {
                conversationState.IsWaitingForClarification = true;
                conversationState.ClarificationReason = "vague-risk-or-help-request";

                string clarificationQuestion = clarifyingQuestionService.BuildClarifyingQuestion(
                    userMemory.UserName,
                    riskLevel);

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "clarification",
                    detectedSentiment,
                    clarificationQuestion);
            }

            string baseResponse;

            if (keywordService.IsHelpRequest(normalisedInput))
            {
                baseResponse = responseService.GetHelpResponse();

                SetPendingChoice(
                    conversationState.CurrentTopic,
                    "help",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "help",
                    detectedSentiment,
                    baseResponse);
            }

            if (detectedIntent == "summary")
            {
                baseResponse = responseService.GetSessionSummary(
                    userMemory.UserName,
                    userMemory.FavouriteTopic,
                    userMemory.LastTopic,
                    userMemory.LastSentiment,
                    conversationState.TotalMessages);

                baseResponse += "\n\n" + BuildIntelligenceSnapshot();

                ClearPendingChoice();

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse);
            }

            if (IsRecallRequest(normalisedInput))
            {
                baseResponse = GetMemorySummary();

                SetPendingChoice(
                    userMemory.LastTopic,
                    "recall",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "recall",
                    detectedSentiment,
                    baseResponse);
            }

            if (IsNameIntroduction(normalisedInput))
            {
                baseResponse = SaveUserNameFromConversation(userInput);

                ClearPendingChoice();

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "name",
                    detectedSentiment,
                    baseResponse);
            }

            if (riskLevel == "High" || riskLevel == "Emergency")
            {
                if (!string.IsNullOrWhiteSpace(detectedTopic))
                {
                    UpdateTopic(detectedTopic);
                }

                userMemory.LastEmergencyType = emergencyType;

                baseResponse =
                    riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                    "\n\n" +
                    responseService.GetEmergencyResponse(emergencyType);

                SetPendingChoice(
                    string.IsNullOrWhiteSpace(detectedTopic)
                        ? conversationState.CurrentTopic
                        : detectedTopic,
                    "risk",
                    "checklist, tip, example");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "emergency",
                    detectedSentiment,
                    baseResponse);
            }

            if (detectedIntent == "emergency")
            {
                userMemory.LastEmergencyType = emergencyType;

                if (!string.IsNullOrWhiteSpace(detectedTopic))
                {
                    UpdateTopic(detectedTopic);
                }

                baseResponse =
                    riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                    "\n\n" +
                    responseService.GetEmergencyResponse(emergencyType);

                SetPendingChoice(
                    string.IsNullOrWhiteSpace(detectedTopic)
                        ? conversationState.CurrentTopic
                        : detectedTopic,
                    "emergency",
                    "checklist, tip, example");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse);
            }

            if (IsInterestStatement(normalisedInput) && !string.IsNullOrWhiteSpace(detectedTopic))
            {
                baseResponse = SaveFavouriteTopic(detectedTopic, detectedSentiment);

                SetPendingChoice(
                    detectedTopic,
                    "interest",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    "interest",
                    detectedSentiment,
                    baseResponse);
            }

            if (detectedIntent == "follow-up")
            {
                baseResponse = HandleFollowUp(detectedSentiment);

                SetPendingChoice(
                    conversationState.CurrentTopic,
                    "follow-up",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    conversationState.CurrentTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse);
            }

            if (string.IsNullOrWhiteSpace(detectedTopic) &&
                !string.IsNullOrWhiteSpace(conversationState.CurrentTopic) &&
                (detectedIntent == "definition" ||
                 detectedIntent == "prevention" ||
                 detectedIntent == "example"))
            {
                detectedTopic = conversationState.CurrentTopic;
            }

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                baseResponse = HandleTopicResponse(detectedTopic, detectedIntent, detectedSentiment);

                if (detectedIntent == "example")
                {
                    string platformExample = platformExampleService.GetPlatformExample(
                        detectedTopic,
                        string.IsNullOrWhiteSpace(detectedPlatform)
                            ? userMemory.LastPlatform
                            : detectedPlatform);

                    if (!string.IsNullOrWhiteSpace(platformExample))
                    {
                        baseResponse += "\n\n" + platformExample;
                    }
                }

                if (riskLevel == "Medium")
                {
                    baseResponse =
                        riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                        "\n\n" +
                        baseResponse;
                }

                SetPendingChoice(
                    detectedTopic,
                    "topic",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    detectedTopic,
                    detectedIntent,
                    detectedSentiment,
                    baseResponse);
            }

            if (contextChoiceService.LooksLikeVagueFollowUp(normalisedInput) &&
                !string.IsNullOrWhiteSpace(conversationState.CurrentTopic))
            {
                baseResponse = BuildAllDetailsResponse(conversationState.CurrentTopic);

                SetPendingChoice(
                    conversationState.CurrentTopic,
                    "vague-follow-up",
                    "tip, example, checklist");

                return BuildSmartResponse(
                    userInput,
                    conversationState.CurrentTopic,
                    "follow-up",
                    detectedSentiment,
                    baseResponse);
            }

            baseResponse = BuildSmartDefaultResponse(
                normalisedInput,
                detectedSentiment,
                riskLevel,
                detectedIssue);

            SetPendingChoice(
                conversationState.CurrentTopic,
                "default",
                "tip, example, checklist");

            return BuildSmartResponse(
                userInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                baseResponse);
        }

        private string HandleClarificationAnswer(
            string originalInput,
            string normalisedInput,
            string detectedSentiment,
            string detectedTopic,
            string detectedIntent,
            string riskLevel,
            string detectedIssue)
        {
            conversationState.IsWaitingForClarification = false;

            if (string.IsNullOrWhiteSpace(detectedTopic))
            {
                if (normalisedInput.Contains("clicked") || normalisedInput.Contains("link"))
                {
                    detectedTopic = "phishing";
                }
                else if (normalisedInput.Contains("password"))
                {
                    detectedTopic = "password";
                }
                else if (normalisedInput.Contains("otp") || normalisedInput.Contains("one time password"))
                {
                    detectedTopic = "2fa";
                }
                else if (normalisedInput.Contains("download") || normalisedInput.Contains("file"))
                {
                    detectedTopic = "malware";
                }
                else if (normalisedInput.Contains("message") ||
                         normalisedInput.Contains("sms") ||
                         normalisedInput.Contains("whatsapp") ||
                         normalisedInput.Contains("email"))
                {
                    detectedTopic = "scam";
                }
            }

            if (string.IsNullOrWhiteSpace(detectedTopic))
            {
                conversationState.IsWaitingForClarification = true;

                return BuildSmartResponse(
                    originalInput,
                    "",
                    "clarification",
                    detectedSentiment,
                    "I still need one more detail. Was it about a link, password, OTP, file download, suspicious message, or learning a topic?");
            }

            UpdateTopic(detectedTopic);

            string baseResponse =
                riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                "\n\n" +
                responseService.GetTopicResponse(detectedTopic, "general") +
                "\n\n" +
                BuildChecklistResponse(detectedTopic);

            SetPendingChoice(
                detectedTopic,
                "clarification-result",
                "tip, example, checklist");

            return BuildSmartResponse(
                originalInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                baseResponse);
        }

        private string TryHandleContextualChoice(
            string originalInput,
            string normalisedInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment)
        {
            if (!conversationState.IsWaitingForChoice)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(detectedTopic) &&
                detectedTopic != conversationState.PendingTopic)
            {
                return "";
            }

            string choice = contextChoiceService.DetectChoice(
                normalisedInput,
                conversationState.PendingOptions);

            if (string.IsNullOrWhiteSpace(choice))
            {
                choice = DetectIntentMemoryChoice(normalisedInput);
            }

            if (string.IsNullOrWhiteSpace(choice))
            {
                return "";
            }

            if (choice == "no")
            {
                ClearPendingChoice();

                return BuildSmartResponse(
                    originalInput,
                    conversationState.CurrentTopic,
                    "choice",
                    detectedSentiment,
                    "No problem. You can ask me about another cybersecurity topic whenever you are ready.");
            }

            string topic = !string.IsNullOrWhiteSpace(detectedTopic)
                ? detectedTopic
                : conversationState.PendingTopic;

            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = conversationState.CurrentTopic;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = userMemory.LastTopic;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                return "";
            }

            UpdateTopic(topic);

            string baseResponse;

            if (choice == "yes" || choice == "all")
            {
                baseResponse = BuildAllDetailsResponse(topic);
            }
            else
            {
                baseResponse = BuildSpecificChoiceResponse(topic, choice);
            }

            SetPendingChoice(
                topic,
                "choice-response",
                "tip, example, checklist");

            return BuildSmartResponse(
                originalInput,
                topic,
                choice,
                detectedSentiment,
                baseResponse);
        }

        private string DetectIntentMemoryChoice(string normalisedInput)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "";
            }

            if (normalisedInput.Contains("another") || normalisedInput.Contains("more like that"))
            {
                if (!string.IsNullOrWhiteSpace(userMemory.LastIntentRequested))
                {
                    return userMemory.LastIntentRequested;
                }

                return "example";
            }

            if (normalisedInput.Contains("tips") || normalisedInput.Contains("advice"))
            {
                return "tip";
            }

            if (normalisedInput.Contains("example") || normalisedInput.Contains("scenario"))
            {
                return "example";
            }

            if (normalisedInput.Contains("steps") || normalisedInput.Contains("checklist"))
            {
                return "checklist";
            }

            return "";
        }

        private string BuildSpecificChoiceResponse(string topic, string choice)
        {
            userMemory.LastIntentRequested = choice;

            switch (choice)
            {
                case "tip":
                    return responseService.GetTopicResponse(topic, "prevention");

                case "example":
                    string platformExample = platformExampleService.GetPlatformExample(topic, userMemory.LastPlatform);

                    if (!string.IsNullOrWhiteSpace(platformExample))
                    {
                        return platformExample;
                    }

                    return responseService.GetTopicResponse(topic, "example");

                case "checklist":
                    return BuildChecklistResponse(topic);

                case "definition":
                    return responseService.GetTopicResponse(topic, "definition");

                default:
                    return responseService.GetTopicResponse(topic, "general");
            }
        }

        private string BuildAllDetailsResponse(string topic)
        {
            string definition = responseService.GetTopicResponse(topic, "definition");
            string tip = responseService.GetTopicResponse(topic, "prevention");
            string example = responseService.GetTopicResponse(topic, "example");
            string platformExample = platformExampleService.GetPlatformExample(topic, userMemory.LastPlatform);
            string checklist = BuildChecklistResponse(topic);

            if (!string.IsNullOrWhiteSpace(platformExample))
            {
                example += "\n\n" + platformExample;
            }

            return $"Here is the full breakdown for {topic}:\n\n" +
                   $"1. Meaning\n{definition}\n\n" +
                   $"2. Practical Safety Tip\n{tip}\n\n" +
                   $"3. Real-Life Example\n{example}\n\n" +
                   $"4. Quick Checklist\n{checklist}";
        }

        private string BuildChecklistResponse(string topic)
        {
            switch (topic)
            {
                case "password":
                    return "Password checklist:\n" +
                           "• Use at least 12 characters.\n" +
                           "• Mix letters, numbers, and symbols.\n" +
                           "• Do not reuse the same password on different accounts.\n" +
                           "• Use a password manager if possible.\n" +
                           "• Enable 2FA on important accounts.";

                case "phishing":
                    return "Phishing checklist:\n" +
                           "• Check the sender carefully.\n" +
                           "• Do not click links from unknown messages.\n" +
                           "• Watch for urgent or threatening language.\n" +
                           "• Never share passwords or OTPs.\n" +
                           "• Visit websites by typing the address yourself.";

                case "scam":
                    return "Scam checklist:\n" +
                           "• Be careful of offers that sound too good to be true.\n" +
                           "• Do not pay upfront fees to unknown people.\n" +
                           "• Verify the person or company independently.\n" +
                           "• Do not share banking details or OTPs.\n" +
                           "• Take time to think before responding.";

                case "privacy":
                    return "Privacy checklist:\n" +
                           "• Limit what you post publicly.\n" +
                           "• Review app permissions.\n" +
                           "• Keep social media profiles private where possible.\n" +
                           "• Avoid sharing ID numbers, addresses, or banking details online.\n" +
                           "• Use strong passwords and 2FA.";

                case "safe browsing":
                    return "Safe browsing checklist:\n" +
                           "• Check for HTTPS on websites.\n" +
                           "• Avoid suspicious pop-ups.\n" +
                           "• Do not download files from unknown sites.\n" +
                           "• Keep your browser updated.\n" +
                           "• Be careful with shortened links.";

                case "malware":
                    return "Malware checklist:\n" +
                           "• Do not open unknown attachments.\n" +
                           "• Keep antivirus protection active.\n" +
                           "• Update your device regularly.\n" +
                           "• Avoid pirated software.\n" +
                           "• Scan suspicious files before opening them.";

                case "2fa":
                    return "2FA checklist:\n" +
                           "• Enable 2FA on email, banking, and social media.\n" +
                           "• Use an authenticator app where possible.\n" +
                           "• Never share OTP codes.\n" +
                           "• Save backup codes securely.\n" +
                           "• Review trusted devices regularly.";

                default:
                    return "General cyber safety checklist:\n" +
                           "• Think before clicking links.\n" +
                           "• Use strong passwords.\n" +
                           "• Enable 2FA.\n" +
                           "• Keep your device updated.\n" +
                           "• Never share OTPs or passwords.";
            }
        }

        private string BuildSmartDefaultResponse(
            string normalisedInput,
            string detectedSentiment,
            string riskLevel,
            string detectedIssue)
        {
            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                return "I can tell there may be a concern here, but I need a bit more detail. Are you asking about a password, phishing message, scam, privacy, safe browsing, malware, or 2FA?";
            }

            if (riskLevel == "Medium")
            {
                return riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                       "\n\nTell me what happened next: did you click a link, enter details, download a file, or only receive the message?";
            }

            return responseService.GetDefaultResponse();
        }

        private string BuildIntelligenceSnapshot()
        {
            string platform = string.IsNullOrWhiteSpace(userMemory.LastPlatform)
                ? "Not detected"
                : userMemory.LastPlatform;

            string issue = string.IsNullOrWhiteSpace(userMemory.LastDetectedIssue)
                ? "None detected"
                : userMemory.LastDetectedIssue;

            string mainTopic = string.IsNullOrWhiteSpace(chatHistoryService.GetMainTopic())
                ? "Not detected"
                : chatHistoryService.GetMainTopic();

            return "CyberBot intelligence snapshot:\n" +
                   $"• Current risk level: {userMemory.CurrentRiskLevel}\n" +
                   $"• Highest risk level: {userMemory.HighestRiskLevel}\n" +
                   $"• Main topic: {mainTopic}\n" +
                   $"• Last detected issue: {issue}\n" +
                   $"• Last platform detected: {platform}\n" +
                   $"• Reports generated: {userMemory.ReportsGenerated}";
        }

        private void SetPendingChoice(string topic, string questionType, string options)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = conversationState.CurrentTopic;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = userMemory.LastTopic;
            }

            conversationState.IsWaitingForChoice = !string.IsNullOrWhiteSpace(topic);
            conversationState.PendingTopic = topic;
            conversationState.PendingQuestionType = questionType;
            conversationState.PendingOptions = options;
            conversationState.LastBotOffer = options;
        }

        private void ClearPendingChoice()
        {
            conversationState.IsWaitingForChoice = false;
            conversationState.PendingTopic = "";
            conversationState.PendingQuestionType = "";
            conversationState.PendingOptions = "";
            conversationState.LastBotOffer = "";
        }

        private string BuildSmartResponse(
            string userInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment,
            string baseResponse)
        {
            string smartResponse = personalityService.BuildPersonalisedResponse(
                userMemory.UserName,
                userInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                baseResponse,
                conversationState.TotalMessages,
                userMemory.FavouriteTopic,
                conversationState.FollowUpCount);

            chatHistoryService.AddBotMessage(
                smartResponse,
                detectedTopic,
                detectedIntent,
                userMemory.CurrentRiskLevel);

            return smartResponse;
        }

        private string HandleTopicResponse(
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment)
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

        private bool IsReportRequest(string normalisedInput)
        {
            return normalisedInput.Contains("generate report") ||
                   normalisedInput.Contains("cyber safety report") ||
                   normalisedInput.Contains("safety report") ||
                   normalisedInput.Contains("session report") ||
                   normalisedInput.Contains("create report") ||
                   normalisedInput.Contains("give me a report") ||
                   normalisedInput.Contains("report of this chat");
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

            string platform = string.IsNullOrWhiteSpace(userMemory.LastPlatform)
                ? "not detected yet"
                : userMemory.LastPlatform;

            string issue = string.IsNullOrWhiteSpace(userMemory.LastDetectedIssue)
                ? "not detected yet"
                : userMemory.LastDetectedIssue;

            string mainTopic = string.IsNullOrWhiteSpace(chatHistoryService.GetMainTopic())
                ? "not detected yet"
                : chatHistoryService.GetMainTopic();

            string response =
                $"I remember that your name is {userMemory.UserName}.\n\n" +
                $"• Favourite cybersecurity topic: {favouriteTopic}\n" +
                $"• Main topic in this session: {mainTopic}\n" +
                $"• Last topic discussed: {lastTopic}\n" +
                $"• Last mood detected: {lastSentiment}\n" +
                $"• Current risk level: {userMemory.CurrentRiskLevel}\n" +
                $"• Highest risk level: {userMemory.HighestRiskLevel}\n" +
                $"• Last detected issue: {issue}\n" +
                $"• Last platform detected: {platform}\n" +
                $"• Reports generated: {userMemory.ReportsGenerated}";

            if (favouriteTopic != "not set yet")
            {
                response += $"\n\nSince you are interested in {favouriteTopic}, I can keep giving you useful tips about that topic.";
            }

            return response;
        }
    }
}