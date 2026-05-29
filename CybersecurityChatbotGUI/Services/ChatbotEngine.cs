// Import the Models namespace for data structures used by the chatbot engine
using CybersecurityChatbotGUI.Models;

namespace CybersecurityChatbotGUI.Services
{
    // Core chatbot engine that processes user messages and generates intelligent responses
    // Uses a delegation pattern with multiple specialized services for different aspects of conversation
    public class ChatbotEngine
    {
        // Delegate type for response generation methods
        // Allows the engine to switch between different response strategies
        private delegate string ResponseHandler(string userInput);

        // Service dependencies injected through constructor
        // Each service handles a specific aspect of the chatbot's intelligence
        private readonly ResponseService responseService;              // Provides topic-specific and general responses
        private readonly KeywordService keywordService;                // Detects keywords, topics, and intents in user input
        private readonly SentimentService sentimentService;            // Analyzes emotional tone of user messages
        private readonly PersonalityService personalityService;        // Personalizes responses based on user context
        private readonly InputNormaliserService inputNormaliserService; // Normalizes user input for consistent processing
        private readonly ContextChoiceService contextChoiceService;    // Detects user choices in follow-up conversations
        private readonly RiskLevelService riskLevelService;            // Assesses cybersecurity risk levels from user input
        private readonly PlatformExampleService platformExampleService; // Provides platform-specific security examples
        private readonly ClarifyingQuestionService clarifyingQuestionService; // Determines when to ask clarifying questions
        private readonly ChatHistoryService chatHistoryService;        // Tracks conversation history and context
        private readonly CyberSafetyReportService cyberSafetyReportService; // Generates comprehensive safety reports

        // State management objects
        private readonly UserMemory userMemory;                // Stores persistent user information across conversation
        private readonly ConversationState conversationState;  // Tracks current conversation flow and state

        // Response handler delegate instance
        // Defaults to GenerateResponse method for processing all user input
        private readonly ResponseHandler responseHandler;

        // Constructor: initializes all services and state objects
        public ChatbotEngine()
        {
            // Initialize all service dependencies
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

            // Initialize state objects
            userMemory = new UserMemory();
            conversationState = new ConversationState();

            // Set default response handler to GenerateResponse method
            responseHandler = GenerateResponse;
        }

        // Property: Returns formatted display text for the last topic discussed
        // Shows "None" if no topic has been set
        public string LastTopicDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastTopic)
                    ? "None"
                    : userMemory.LastTopic;
            }
        }

        // Property: Returns formatted display text for the last detected sentiment
        // Shows "Not detected" if no sentiment has been detected
        public string LastSentimentDisplay
        {
            get
            {
                return string.IsNullOrWhiteSpace(userMemory.LastSentiment)
                    ? "Not detected"
                    : userMemory.LastSentiment;
            }
        }

        // Main entry point for processing user messages
        // Delegates to the response handler for generating appropriate responses
        public string ProcessMessage(string userInput)
        {
            return responseHandler(userInput);
        }

        // Sets the user's name in memory for personalized responses
        // Only sets if the provided name is not empty or whitespace
        public void SetUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                userMemory.UserName = name.Trim();
            }
        }

        // Resets conversation state while preserving the user's name
        // Clears topics, intents, follow-up counts, pending choices, and history
        public void ResetConversationButKeepUser()
        {
            // Reset conversation state properties
            conversationState.CurrentTopic = "";
            conversationState.PreviousTopic = "";
            conversationState.LastIntent = "";
            conversationState.FollowUpCount = 0;
            conversationState.TotalMessages = 0;

            // Clear pending choice state (for contextual follow-up questions)
            conversationState.IsWaitingForChoice = false;
            conversationState.PendingTopic = "";
            conversationState.PendingQuestionType = "";
            conversationState.PendingOptions = "";

            // Clear clarification state
            conversationState.IsWaitingForClarification = false;
            conversationState.ClarificationReason = "";
            conversationState.LastBotOffer = "";

            // Reset user memory (except user name which is preserved by caller)
            userMemory.LastTopic = "";
            userMemory.LastSentiment = "";
            userMemory.LastEmergencyType = "";
            userMemory.CurrentRiskLevel = "Low";
            userMemory.HighestRiskLevel = "Low";
            userMemory.LastDetectedIssue = "";
            userMemory.LastPlatform = "";
            userMemory.LastIntentRequested = "";
            userMemory.ReportsGenerated = 0;

            // Clear chat history
            chatHistoryService.Clear();
        }

        // Core response generation logic
        // Analyzes user input through multiple services and builds an appropriate response
        // Handles various conversation scenarios: emergencies, topics, intents, clarifications, reports
        private string GenerateResponse(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "Please type a message first.";
            }

            // Trim whitespace from input
            userInput = userInput.Trim();

            // Normalize input for consistent processing (lowercase, remove extra spaces, etc.)
            string normalisedInput = inputNormaliserService.Normalise(userInput);

            // Increment total message counter
            conversationState.TotalMessages++;

            // Analyze user input through various detection services
            string detectedSentiment = sentimentService.DetectSentiment(normalisedInput);      // Detect emotional tone
            string detectedTopic = keywordService.DetectTopic(normalisedInput);                // Detect cybersecurity topic
            string detectedIntent = keywordService.DetectIntent(normalisedInput);              // Detect user's intent
            string emergencyType = keywordService.DetectEmergencyType(normalisedInput);        // Detect emergency scenario
            string riskLevel = riskLevelService.DetectRiskLevel(normalisedInput);              // Assess risk level
            string detectedIssue = riskLevelService.DetectIssue(normalisedInput);              // Detect specific issue
            string detectedPlatform = platformExampleService.DetectPlatform(normalisedInput);  // Detect platform mentioned

            // Check for context-based risk escalation (repeated high-risk mentions)
            riskLevel = chatHistoryService.DetectContextRiskEscalation(normalisedInput, riskLevel);

            // Record user message in chat history for context tracking
            chatHistoryService.AddUserMessage(
                userInput,
                detectedTopic,
                detectedIntent,
                riskLevel);

            // Update user memory with detected information (only if detected)
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

            // Update risk levels in user memory
            userMemory.CurrentRiskLevel = riskLevel;
            userMemory.HighestRiskLevel = chatHistoryService.GetHighestRiskLevel();

            if (!string.IsNullOrWhiteSpace(detectedIntent))
            {
                userMemory.LastIntentRequested = detectedIntent;
            }

            conversationState.LastIntent = detectedIntent;

            // CHECK 1: Report generation request
            // Handles "generate report", "cyber safety report", etc.
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

            // CHECK 2: Handle pending clarification
            // If bot is waiting for user to clarify a vague message
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

            // CHECK 3: Handle contextual choice
            // If bot offered options and user is responding with a choice
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

            // CHECK 4: Needs clarification?
            // Detects vague inputs like "help" or "I have a problem" without specifics
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

            // CHECK 5: Help request
            // User explicitly asks for help or guidance
            if (keywordService.IsHelpRequest(normalisedInput))
            {
                baseResponse = responseService.GetHelpResponse();

                // Set up pending choice for follow-up options
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

            // CHECK 6: Summary request
            // User asks for session summary or overview
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

            // CHECK 7: Recall/memory request
            // User asks what the bot remembers about them
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

            // CHECK 8: Name introduction
            // User introduces themselves during conversation
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

            // CHECK 9: High risk or emergency detected
            // Immediate escalation for dangerous situations
            if (riskLevel == "High" || riskLevel == "Emergency")
            {
                if (!string.IsNullOrWhiteSpace(detectedTopic))
                {
                    UpdateTopic(detectedTopic);
                }

                userMemory.LastEmergencyType = emergencyType;

                // Build risk warning + emergency response
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

            // CHECK 10: Emergency intent (user expressing urgent concern)
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

            // CHECK 11: Interest statement
            // User expresses interest in a specific cybersecurity topic
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

            // CHECK 12: Follow-up intent
            // User asks for more information on current topic
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

            // CHECK 13: Intent without explicit topic
            // Use current conversation topic if user asks for definition/example/prevention
            if (string.IsNullOrWhiteSpace(detectedTopic) &&
                !string.IsNullOrWhiteSpace(conversationState.CurrentTopic) &&
                (detectedIntent == "definition" ||
                 detectedIntent == "prevention" ||
                 detectedIntent == "example"))
            {
                detectedTopic = conversationState.CurrentTopic;
            }

            // CHECK 14: Topic detected
            // Respond with topic-specific information
            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                baseResponse = HandleTopicResponse(detectedTopic, detectedIntent, detectedSentiment);

                // Add platform-specific example if user asked for examples
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

                // Add risk warning for medium risk situations
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

            // CHECK 15: Vague follow-up with known topic
            // User says "tell me more" without specifying topic
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

            // DEFAULT RESPONSE
            // Fallback when no specific intent or topic is detected
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

        // Handles user's answer to a clarifying question
        // Attempts to extract topic from the clarification answer
        // Falls back to keyword detection if topic cannot be inferred
        private string HandleClarificationAnswer(
            string originalInput,
            string normalisedInput,
            string detectedSentiment,
            string detectedTopic,
            string detectedIntent,
            string riskLevel,
            string detectedIssue)
        {
            // Clear clarification flag since user has responded
            conversationState.IsWaitingForClarification = false;

            // If no topic detected, try to infer from keywords in clarification answer
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

            // If still no topic, ask for more specific detail
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

            // Topic identified from clarification - provide full response
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

        // Attempts to detect and handle when user is responding to a pending choice offer
        // Returns empty string if no choice is pending or no valid choice is detected
        private string TryHandleContextualChoice(
            string originalInput,
            string normalisedInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment)
        {
            // Only process if bot is waiting for a choice
            if (!conversationState.IsWaitingForChoice)
            {
                return "";
            }

            // If user changed topic, don't treat as choice response
            if (!string.IsNullOrWhiteSpace(detectedTopic) &&
                detectedTopic != conversationState.PendingTopic)
            {
                return "";
            }

            // Detect which choice the user made
            string choice = contextChoiceService.DetectChoice(
                normalisedInput,
                conversationState.PendingOptions);

            // If no direct choice, try memory-based intent detection
            if (string.IsNullOrWhiteSpace(choice))
            {
                choice = DetectIntentMemoryChoice(normalisedInput);
            }

            // If still no choice, cannot handle
            if (string.IsNullOrWhiteSpace(choice))
            {
                return "";
            }

            // User declined the offer
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

            // Determine which topic to respond about
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

            // Build response based on choice type
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

        // Detects user intent from memory when explicit choice keywords aren't found
        // Falls back to common intent patterns like "tips", "examples", "steps"
        private string DetectIntentMemoryChoice(string normalisedInput)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "";
            }

            // User wants more of the same type of content
            if (normalisedInput.Contains("another") || normalisedInput.Contains("more like that"))
            {
                if (!string.IsNullOrWhiteSpace(userMemory.LastIntentRequested))
                {
                    return userMemory.LastIntentRequested;
                }

                return "example";
            }

            // Direct intent keyword matching
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

        // Builds response for a specific user choice (tip, example, checklist, definition)
        private string BuildSpecificChoiceResponse(string topic, string choice)
        {
            userMemory.LastIntentRequested = choice;

            switch (choice)
            {
                case "tip":
                    return responseService.GetTopicResponse(topic, "prevention");

                case "example":
                    // Try platform-specific example first, fall back to generic
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

        // Builds comprehensive response covering all aspects of a topic
        // Includes definition, safety tips, examples, platform examples, and checklist
        private string BuildAllDetailsResponse(string topic)
        {
            string definition = responseService.GetTopicResponse(topic, "definition");
            string tip = responseService.GetTopicResponse(topic, "prevention");
            string example = responseService.GetTopicResponse(topic, "example");
            string platformExample = platformExampleService.GetPlatformExample(topic, userMemory.LastPlatform);
            string checklist = BuildChecklistResponse(topic);

            // Append platform example if available
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

        // Builds a topic-specific safety checklist with actionable steps
        // Each case returns a formatted checklist for the given cybersecurity topic
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

        // Builds a smart default response when no specific topic or intent is detected
        // Uses sentiment and risk level to provide contextually appropriate fallback
        private string BuildSmartDefaultResponse(
            string normalisedInput,
            string detectedSentiment,
            string riskLevel,
            string detectedIssue)
        {
            // If sentiment detected, acknowledge concern and ask for topic
            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                return "I can tell there may be a concern here, but I need a bit more detail. Are you asking about a password, phishing message, scam, privacy, safe browsing, malware, or 2FA?";
            }

            // Medium risk situations prompt for more details
            if (riskLevel == "Medium")
            {
                return riskLevelService.BuildRiskResponse(riskLevel, detectedIssue) +
                       "\n\nTell me what happened next: did you click a link, enter details, download a file, or only receive the message?";
            }

            // Generic fallback response
            return responseService.GetDefaultResponse();
        }

        // Builds a summary snapshot of the current conversation intelligence
        // Shows risk levels, topics, detected issues, platforms, and report count
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

        // Sets up a pending choice state for contextual follow-up questions
        // Enables the bot to offer choices like "tip, example, checklist" and wait for user response
        private void SetPendingChoice(string topic, string questionType, string options)
        {
            // Fallback topic resolution (use current topic or last topic if none provided)
            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = conversationState.CurrentTopic;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = userMemory.LastTopic;
            }

            // Only set pending choice if a topic exists
            conversationState.IsWaitingForChoice = !string.IsNullOrWhiteSpace(topic);
            conversationState.PendingTopic = topic;
            conversationState.PendingQuestionType = questionType;
            conversationState.PendingOptions = options;
            conversationState.LastBotOffer = options;
        }

        // Clears all pending choice state
        // Called when user declines offer or choice is processed
        private void ClearPendingChoice()
        {
            conversationState.IsWaitingForChoice = false;
            conversationState.PendingTopic = "";
            conversationState.PendingQuestionType = "";
            conversationState.PendingOptions = "";
            conversationState.LastBotOffer = "";
        }

        // Builds final response by passing through personality service for personalization
        // Adds user context, name, preferences, and conversation history to the base response
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

            // Record bot response in chat history
            chatHistoryService.AddBotMessage(
                smartResponse,
                detectedTopic,
                detectedIntent,
                userMemory.CurrentRiskLevel);

            return smartResponse;
        }

        // Handles topic-specific responses with empathy layer
        // Combines sentiment-based empathy with topic-specific information
        private string HandleTopicResponse(
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment)
        {
            UpdateTopic(detectedTopic);

            string empathy = sentimentService.GetEmpathyResponse(detectedSentiment);
            string response = responseService.GetTopicResponse(detectedTopic, detectedIntent);

            // Prepend empathy if detected
            if (!string.IsNullOrWhiteSpace(empathy))
            {
                return $"{empathy}\n\n{response}";
            }

            return response;
        }

        // Updates current conversation topic and tracks topic transitions
        // Saves previous topic and resets follow-up count for new topic
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

        // Saves user's favourite topic when they express interest
        // Returns acknowledgment with empathy and topic introduction
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

        // Handles follow-up requests for more information on current topic
        // Increments follow-up counter and provides additional information
        private string HandleFollowUp(string detectedSentiment)
        {
            // No current topic to follow up on
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

        // Detects if user is introducing themselves with their name
        // Matches patterns like "My name is...", "Call me...", "You can call me..."
        private bool IsNameIntroduction(string userInput)
        {
            string lowerInput = userInput.ToLower();

            return lowerInput.StartsWith("my name is ") ||
                   lowerInput.StartsWith("call me ") ||
                   lowerInput.StartsWith("you can call me ");
        }

        // Extracts and saves user name from a name introduction message
        // Validates the extracted name has sufficient length and contains letters
        private string SaveUserNameFromConversation(string userInput)
        {
            string name = userInput;

            // Remove known name introduction prefixes
            name = name.Replace("My name is", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("Call me", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Replace("You can call me", "", System.StringComparison.OrdinalIgnoreCase);
            name = name.Trim();

            // Validate name length
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            {
                return "I could not clearly detect your name. Try typing something like: My name is Vusi.";
            }

            // Validate name contains at least one letter
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

            // Save valid name
            userMemory.UserName = name;

            return $"Nice to meet you, {userMemory.UserName}. I will remember your name during this chat.";
        }

        // Detects if user is expressing interest in a cybersecurity topic
        // Matches patterns like "interested in", "I like", "teach me about"
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

        // Detects if user is asking what the bot remembers about them
        // Matches patterns like "what do you remember", "my favourite topic"
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

        // Detects if user is requesting a cyber safety report
        // Matches patterns like "generate report", "cyber safety report"
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

        // Builds comprehensive memory summary of everything the bot remembers
        // Includes user name, favourite topic, risk levels, detected issues, and more
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

            // Add encouragement related to favourite topic
            if (favouriteTopic != "not set yet")
            {
                response += $"\n\nSince you are interested in {favouriteTopic}, I can keep giving you useful tips about that topic.";
            }

            return response;
        }
    }
}