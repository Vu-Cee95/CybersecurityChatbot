using System;
using System.Threading;

using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;
using CybersecurityChatbot.Views;

namespace CybersecurityChatbot.Controllers
{
    internal class ChatbotController
    {
        // Random number generator for selecting reactions
        static Random random = new Random();

        // Reaction phrases for password-related responses
        static string[] passwordReactions = new string[]
        {
            "That's a smart topic to start with",
            "Great choice",
            "Password safety is a very important area to understand",
            "Good thinking",
            "That's something everyone should learn properly"
        };

        // Reaction phrases for phishing-related responses
        static string[] phishingReactions = new string[]
        {
            "Good choice to focus on this",
            "Phishing is definitely something worth understanding",
            "I'm glad you picked this topic",
            "That's an important threat to learn about",
            "This is one of the biggest online risks people face"
        };

        // Reaction phrases for browsing-related responses
        static string[] browsingReactions = new string[]
        {
            "That's a really useful topic",
            "Safe browsing is more important than many people realize",
            "Good choice",
            "This can make a big difference to your online safety",
            "I'm glad you want to learn about this"
        };

        // Reaction phrases for help-related responses
        static string[] helpReactions = new string[]
        {
            "Of course",
            "Absolutely",
            "I'd be happy to help",
            "No problem",
            "Let's go through that together"
        };

        // Reaction phrases for general conversational responses
        static string[] generalReactions = new string[]
        {
            "That's a good question",
            "Interesting question",
            "I'm glad you asked",
            "Let's take a look at that",
            "That's worth understanding"
        };

        // Reaction phrases for warning-related responses
        static string[] warningReactions = new string[]
        {
            "This is important to watch out for",
            "Be careful with this one",
            "This can catch people off guard",
            "A lot of people underestimate this",
            "This is something you should take seriously"
        };

        // Reaction phrases for praise-related responses
        static string[] praiseReactions = new string[]
        {
            "Nice work",
            "Well done",
            "Good job",
            "That's the right idea",
            "You're thinking in the right direction"
        };

        // Main entry point that initializes and runs the chatbot application
        public void Run()
        {
            // Initialize core application components
            User user = new User();
            Validator validator = new Validator();
            Responses responses = new Responses();
            AudioPlayer audio = new AudioPlayer();

            // Display the ASCII art welcome banner for the chatbot
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            CenterWriteLine("=======================================================================");
            CenterWriteLine("  ██████╗██╗   ██╗██████╗ ███████╗██████╗    ██████╗  ██████╗ ████████╗");
            CenterWriteLine(" ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗   ██╔══██╗██╔═══██╗╚══██╔══╝");
            CenterWriteLine(" ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝   ██████╔╝██║   ██║   ██║   ");
            CenterWriteLine(" ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗   ██╔══██╗██║   ██║   ██║   ");
            CenterWriteLine(" ╚██████╗   ██║   ██████╔╝███████╗██║  ██║   ██████╔╝╚██████╔╝   ██║   ");
            CenterWriteLine("  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝   ╚═════╝  ╚═════╝    ╚═╝   ");
            CenterWriteLine("=======================================================================\n");
            CenterWriteLine("*********************************************");
            CenterWriteLine("Your friendly cybersecurity awareness chatbot");
            CenterWriteLine("*********************************************");

            Console.ResetColor();

            // Play the greeting audio message
            audio.PlayGreeting();

            // Prompt user for their name with up to 3 attempts
            string name = null;

            for (int attempts = 1; attempts <= 3; attempts++)
            {
                int attemptsLeft = 3 - attempts;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Please enter your name: ");
                Console.ResetColor();

                name = Console.ReadLine();

                if (validator.IsValidName(name))
                {
                    name = name.Trim();
                    break;
                }

                BotReply($"{responses.Default()} Attempts left: {attemptsLeft}.");
            }

            // Exit if valid name was not provided after all attempts
            if (!validator.IsValidName(name))
            {
                BotReply(responses.AttemptsExceeded());
                return;
            }

            user.Name = name;

            // Display loading animation before showing main menu
            ShowLoadingBar();

            // Main application loop that handles menu navigation
            while (true)
            {
                string choice = ShowWelcomeMainMenu(user, validator, responses);
                if (choice == null)
                    return;

                switch (choice)
                {
                    case "1":
                        if (!RunPasswordSection(user, validator, responses))
                            return;
                        break;

                    case "2":
                        if (!RunPhishingSection(user, validator, responses))
                            return;
                        break;

                    case "3":
                        if (!RunBrowsingSection(user, validator, responses))
                            return;
                        break;

                    case "4":
                        if (!RunQuestionSection(user, validator, responses))
                            return;
                        break;

                    case "5":
                        if (!RunWhatShouldIDoSection(user, validator, responses))
                            return;
                        break;

                    case "6":
                        if (!RunDidYouKnowSection(user, validator, responses))
                            return;
                        break;

                    case "7":
                        if (!RunHowAreYouSection(user, validator, responses))
                            return;
                        break;

                    case "8":
                        Console.Clear();
                        UserReply(user.Name, "Tell me your purpose.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.Purpose()));
                        if (!RunSimpleNavigation(user, validator, responses))
                            return;
                        break;

                    case "9":
                        Console.Clear();
                        UserReply(user.Name, "Show me what I can ask you.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.Help()));
                        if (!RunSimpleNavigation(user, validator, responses))
                            return;
                        break;

                    case "10":
                        Console.Clear();
                        UserReply(user.Name, "I'd like to exit now.");
                        BotReply(responses.Exit(user.Name));
                        return;
                }
            }
        }

        // Displays the main menu banner and options, returns user's validated choice
        static string ShowWelcomeMainMenu(User user, Validator validator, Responses responses)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;

            CenterWriteLine("=======================================================================");
            CenterWriteLine("  ██████╗██╗   ██╗██████╗ ███████╗██████╗    ██████╗  ██████╗ ████████╗");
            CenterWriteLine(" ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗   ██╔══██╗██╔═══██╗╚══██╔══╝");
            CenterWriteLine(" ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝   ██████╔╝██║   ██║   ██║   ");
            CenterWriteLine(" ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗   ██╔══██╗██║   ██║   ██║   ");
            CenterWriteLine(" ╚██████╗   ██║   ██████╔╝███████╗██║  ██║   ██████╔╝╚██████╔╝   ██║   ");
            CenterWriteLine("  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝   ╚═════╝  ╚═════╝    ╚═╝   ");
            CenterWriteLine("=======================================================================");
            Console.WriteLine();
            CenterWriteLine("Your friendly cybersecurity awareness chatbot");
            Console.WriteLine();

            Console.ResetColor();

            // Welcome message and introduction
            BotReply($"Welcome back, {user.Name}!");
            BotReply("I am Cyber-Bot, your cybersecurity assistant.");
            BotReply("What would you like to explore today?");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkCyan;

            CenterWriteLine("================================================");
            CenterWriteLine("What Do You Want To Learn About?");
            CenterWriteLine("================================================\n");
            Console.ResetColor();

            // Display all main menu options
            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine("1. I want to learn about password safety");
            CenterWriteLine("2. I want to learn about phishing");
            CenterWriteLine("3. I want to learn about safe browsing");
            CenterWriteLine("4. I want to ask a question or have a conversation");
            CenterWriteLine("5. What should I do in a risky situation?");
            CenterWriteLine("6. Give me a cybersecurity fact");
            CenterWriteLine("7. How are you?");
            CenterWriteLine("8. Tell me your purpose");
            CenterWriteLine("9. Show me what I can ask you");
            CenterWriteLine("10. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            // Get validated menu choice from user
            return GetChoiceWithAttempts(validator, 1, 10, "Choose an option (1-10): ", responses);
        }

        // Handles the "How are you" conversation flow with emotional response detection
        static bool RunHowAreYouSection(User user, Validator validator, Responses responses)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("HOW ARE YOU");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Initial greeting and ask about user's feelings
            UserReply(user.Name, "How are you?");
            BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.HowAreYou(user.Name)));
            Console.WriteLine();

            BotReply(responses.AskUserHowTheyAre());
            string userFeeling = GetMeaningfulInput(user, validator, responses, $"{user.Name}: ");
            if (userFeeling == null)
                return false;

            Console.WriteLine();

            // Branch based on positive user response
            if (validator.IsPositiveFeeling(userFeeling))
            {
                BotReply(responses.UserFeelingPositive(user.Name));
                Console.WriteLine();
                BotReply(responses.UserFeelingPositiveFollowUp());
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Give me a quick cybersecurity tip");
                CenterWriteLine("2. Show me a phishing warning sign");
                CenterWriteLine("3. Give me a password safety reminder");
                CenterWriteLine("4. Go to Main Menu");
                CenterWriteLine("5. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string positiveChoice = GetChoiceWithAttempts(validator, 1, 5, "Choose an option (1-5): ", responses);
                if (positiveChoice == null)
                    return false;

                Console.Clear();

                switch (positiveChoice)
                {
                    case "1":
                        UserReply(user.Name, "Give me a quick cybersecurity tip.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.RandomDidYouKnow()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "2":
                        UserReply(user.Name, "Show me a phishing warning sign.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingSigns()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "3":
                        UserReply(user.Name, "Give me a password safety reminder.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.PasswordTips()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "4":
                        return true;

                    default:
                        return ExitNow(user, responses);
                }
            }
            // Branch based on negative user response
            else if (validator.IsNegativeFeeling(userFeeling))
            {
                BotReply(responses.UserFeelingNegative(user.Name));
                Console.WriteLine();
                BotReply(responses.UserFeelingNegativeFollowUp());
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Help me with suspicious emails");
                CenterWriteLine("2. Help me with password safety");
                CenterWriteLine("3. Help me with unsafe websites");
                CenterWriteLine("4. Go to Main Menu");
                CenterWriteLine("5. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string negativeChoice = GetChoiceWithAttempts(validator, 1, 5, "Choose an option (1-5): ", responses);
                if (negativeChoice == null)
                    return false;

                Console.Clear();

                switch (negativeChoice)
                {
                    case "1":
                        UserReply(user.Name, "Help me with suspicious emails.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperSuspiciousEmail()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "2":
                        UserReply(user.Name, "Help me with password safety.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.HelperWeakPassword()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "3":
                        UserReply(user.Name, "Help me with unsafe websites.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperUnsafeWebsite()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "4":
                        return true;

                    default:
                        return ExitNow(user, responses);
                }
            }
            // Branch for neutral or unrecognized response
            else
            {
                BotReply(responses.UserFeelingUnknown(user.Name));
                Console.WriteLine();
                BotReply(responses.UserFeelingUnknownFollowUp());
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Give me a cybersecurity tip");
                CenterWriteLine("2. I want to ask a question");
                CenterWriteLine("3. Go to Main Menu");
                CenterWriteLine("4. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string neutralChoice = GetChoiceWithAttempts(validator, 1, 4, "Choose an option (1-4): ", responses);
                if (neutralChoice == null)
                    return false;

                Console.Clear();

                switch (neutralChoice)
                {
                    case "1":
                        UserReply(user.Name, "Give me a cybersecurity tip.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.RandomDidYouKnow()));
                        return RunSimpleNavigation(user, validator, responses);

                    case "2":
                        return RunQuestionSection(user, validator, responses);

                    case "3":
                        return true;

                    default:
                        return ExitNow(user, responses);
                }
            }
        }

        // Manages the password safety education section with multiple learning options
        static bool RunPasswordSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("PASSWORD SAFETY");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to password safety topic
                UserReply(user.Name, "I want to learn about password safety.");
                BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.PasswordIntro()));
                Console.WriteLine();

                // Display password safety submenu options
                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Explain what a strong password is");
                CenterWriteLine("2. Give me password tips");
                CenterWriteLine("3. Show me common password mistakes");
                CenterWriteLine("4. Explain why a password manager helps");
                CenterWriteLine("5. Give me a quick password knowledge check");
                CenterWriteLine("6. Show me a real-life password scenario");
                CenterWriteLine("7. Give me a password fact");
                CenterWriteLine("8. What should I do if I have a password problem?");
                CenterWriteLine("9. Go to Main Menu");
                CenterWriteLine("10. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string choice = GetChoiceWithAttempts(validator, 1, 10, "Choose an option (1-10): ", responses);
                if (choice == null)
                    return false;

                if (choice == "9")
                    return true;

                if (choice == "10")
                    return ExitNow(user, responses);

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("PASSWORD SAFETY");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Handle password safety submenu selections
                switch (choice)
                {
                    case "1":
                        UserReply(user.Name, "Explain what a strong password is.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.StrongPassword()));
                        break;

                    case "2":
                        UserReply(user.Name, "Give me password tips.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.PasswordTips()));
                        break;

                    case "3":
                        UserReply(user.Name, "Show me common password mistakes.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.PasswordMistakes()));
                        break;

                    case "4":
                        UserReply(user.Name, "Explain why a password manager helps.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.PasswordManager()));
                        break;

                    case "5":
                        UserReply(user.Name, "Give me a quick password knowledge check.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PasswordQuizQuestion()));
                        {
                            string quiz = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (quiz == null)
                                return false;

                            if (quiz == "2")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.PasswordQuizCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.PasswordQuizWrong()));
                        }
                        break;

                    case "6":
                        UserReply(user.Name, "Show me a real-life password scenario.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PasswordScenarioQuestion()));
                        {
                            string scenario = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (scenario == null)
                                return false;

                            if (scenario == "3")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.PasswordScenarioCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.PasswordScenarioWrong()));
                        }
                        break;

                    case "7":
                        UserReply(user.Name, "Give me a password fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PasswordDidYouKnow()));
                        break;

                    case "8":
                        if (!RunPasswordHelper(user, validator, responses))
                            return false;
                        continue;
                }

                // Handle navigation after content display
                int passwordNav = RunFollowUpNavigation(user, validator, responses, "Password Safety");
                if (passwordNav == 1)
                    continue;
                if (passwordNav == 2)
                    return true;
                return false;
            }
        }

        // Manages the phishing awareness education section with multiple learning options
        static bool RunPhishingSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("PHISHING AWARENESS");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to phishing awareness topic
                UserReply(user.Name, "I want to learn about phishing.");
                BotReply(BuildNaturalResponse(GetRandomReaction(phishingReactions), responses.PhishingIntro()));
                Console.WriteLine();

                // Display phishing awareness submenu options
                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Show me signs of phishing");
                CenterWriteLine("2. Give me tips to avoid phishing");
                CenterWriteLine("3. Tell me what to do if I suspect phishing");
                CenterWriteLine("4. Give me a quick phishing knowledge check");
                CenterWriteLine("5. Show me a real-life phishing scenario");
                CenterWriteLine("6. Give me a phishing fact");
                CenterWriteLine("7. What should I do if I receive a suspicious message?");
                CenterWriteLine("8. Go to Main Menu");
                CenterWriteLine("9. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string choice = GetChoiceWithAttempts(validator, 1, 9, "Choose an option (1-9): ", responses);
                if (choice == null)
                    return false;

                if (choice == "8")
                    return true;

                if (choice == "9")
                    return ExitNow(user, responses);

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("PHISHING AWARENESS");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Handle phishing awareness submenu selections
                switch (choice)
                {
                    case "1":
                        UserReply(user.Name, "Show me signs of phishing.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingSigns()));
                        break;

                    case "2":
                        UserReply(user.Name, "Give me tips to avoid phishing.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(phishingReactions), responses.PhishingTips()));
                        break;

                    case "3":
                        UserReply(user.Name, "Tell me what to do if I suspect phishing.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingAction()));
                        break;

                    case "4":
                        UserReply(user.Name, "Give me a quick phishing knowledge check.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PhishingQuizQuestion()));
                        {
                            string quiz = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (quiz == null)
                                return false;

                            if (quiz == "2")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.PhishingQuizCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.PhishingQuizWrong()));
                        }
                        break;

                    case "5":
                        UserReply(user.Name, "Show me a real-life phishing scenario.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PhishingScenarioQuestion()));
                        {
                            string scenario = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (scenario == null)
                                return false;

                            if (scenario == "2")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.PhishingScenarioCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.PhishingScenarioWrong()));
                        }
                        break;

                    case "6":
                        UserReply(user.Name, "Give me a phishing fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PhishingDidYouKnow()));
                        break;

                    case "7":
                        if (!RunPhishingHelper(user, validator, responses))
                            return false;
                        continue;
                }

                // Handle navigation after content display
                int phishingNav = RunFollowUpNavigation(user, validator, responses, "Phishing Awareness");
                if (phishingNav == 1)
                    continue;
                if (phishingNav == 2)
                    return true;
                return false;
            }
        }

        // Manages the safe browsing education section with multiple learning options
        static bool RunBrowsingSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("SAFE BROWSING");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to safe browsing topic
                UserReply(user.Name, "I want to learn about safe browsing.");
                BotReply(BuildNaturalResponse(GetRandomReaction(browsingReactions), responses.BrowsingIntro()));
                Console.WriteLine();

                // Display safe browsing submenu options
                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Give me safe browsing tips");
                CenterWriteLine("2. Explain the risks of unsafe browsing");
                CenterWriteLine("3. Give me safe downloading advice");
                CenterWriteLine("4. Give me a quick safe browsing knowledge check");
                CenterWriteLine("5. Show me a real-life browsing scenario");
                CenterWriteLine("6. Give me a browsing fact");
                CenterWriteLine("7. What should I do if a website looks unsafe?");
                CenterWriteLine("8. Go to Main Menu");
                CenterWriteLine("9. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string choice = GetChoiceWithAttempts(validator, 1, 9, "Choose an option (1-9): ", responses);
                if (choice == null)
                    return false;

                if (choice == "8")
                    return true;

                if (choice == "9")
                    return ExitNow(user, responses);

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("SAFE BROWSING");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Handle safe browsing submenu selections
                switch (choice)
                {
                    case "1":
                        UserReply(user.Name, "Give me safe browsing tips.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(browsingReactions), responses.BrowsingTips()));
                        break;

                    case "2":
                        UserReply(user.Name, "Explain the risks of unsafe browsing.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.UnsafeBrowsing()));
                        break;

                    case "3":
                        UserReply(user.Name, "Give me safe downloading advice.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(browsingReactions), responses.SafeDownloads()));
                        break;

                    case "4":
                        UserReply(user.Name, "Give me a quick safe browsing knowledge check.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.BrowsingQuizQuestion()));
                        {
                            string quiz = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (quiz == null)
                                return false;

                            if (quiz == "1")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.BrowsingQuizCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.BrowsingQuizWrong()));
                        }
                        break;

                    case "5":
                        UserReply(user.Name, "Show me a real-life browsing scenario.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.BrowsingScenarioQuestion()));
                        {
                            string scenario = GetChoiceWithAttempts(validator, 1, 3, "Your answer (1-3): ", responses);
                            if (scenario == null)
                                return false;

                            if (scenario == "2")
                                BotReply(BuildNaturalResponse(GetRandomReaction(praiseReactions), responses.BrowsingScenarioCorrect()));
                            else
                                BotReply(BuildNaturalResponse("Not quite", responses.BrowsingScenarioWrong()));
                        }
                        break;

                    case "6":
                        UserReply(user.Name, "Give me a browsing fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.BrowsingDidYouKnow()));
                        break;

                    case "7":
                        if (!RunBrowsingHelper(user, validator, responses))
                            return false;
                        continue;
                }

                // Handle navigation after content display
                int browsingNav = RunFollowUpNavigation(user, validator, responses, "Safe Browsing");
                if (browsingNav == 1)
                    continue;
                if (browsingNav == 2)
                    return true;
                return false;
            }
        }

        // Manages the open-ended question and conversation section with topic detection
        static bool RunQuestionSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                // Initialize conversation state tracking
                ConversationState state = new ConversationState();
                state.CurrentTopic = "general";
                state.Step = 0;
                state.LastUserInput = "";

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("ASK A QUESTION");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to conversation mode
                UserReply(user.Name, "I want to ask a question or have a conversation.");
                BotReply("You can have a deeper conversation with me here. After every response, you can ask a follow-up question, go to Main Menu, or exit the chatbot.");
                Console.WriteLine();

                // Conversation loop with 6-step progression
                while (state.Step < 6)
                {
                    string input = GetMeaningfulInput(user, validator, responses, "Ask your question: ");
                    if (input == null)
                        return false;

                    state.LastUserInput = input;

                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    CenterWriteLine("========================================");
                    CenterWriteLine("ASK A QUESTION");
                    CenterWriteLine("========================================");
                    Console.ResetColor();
                    Console.WriteLine();

                    UserReply(user.Name, input);
                    BotReply(GetConversationResponse(input, user, responses, state));
                    Console.WriteLine();

                    // Handle end of 6-step conversation
                    if (state.Step >= 6)
                    {
                        BotReply("We've now had a full 6-step conversation. You can start a new conversation, go to Main Menu, or exit the chatbot.");
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        CenterWriteLine("1. Start a new conversation");
                        CenterWriteLine("2. Go to Main Menu");
                        CenterWriteLine("3. Exit the chatbot");
                        Console.ResetColor();
                        Console.WriteLine();

                        string endChoice = GetChoiceWithAttempts(validator, 1, 3, "Choose an option (1-3): ", responses);
                        if (endChoice == null)
                            return false;

                        if (endChoice == "1")
                            break;

                        if (endChoice == "2")
                            return true;

                        return ExitNow(user, responses);
                    }

                    // Navigation options between conversation steps
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    CenterWriteLine("----------------------------------------");
                    Console.ResetColor();

                    BotReply("What would you like to do next?");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    CenterWriteLine("1. Ask a follow-up question");
                    CenterWriteLine("2. Go to Main Menu");
                    CenterWriteLine("3. Exit the chatbot");
                    Console.ResetColor();
                    Console.WriteLine();

                    string navChoice = GetChoiceWithAttempts(validator, 1, 3, "Choose an option (1-3): ", responses);
                    if (navChoice == null)
                        return false;

                    if (navChoice == "1")
                        continue;

                    if (navChoice == "2")
                        return true;

                    return ExitNow(user, responses);
                }
            }
        }

        // Provides guidance for users facing risky cybersecurity situations
        static bool RunWhatShouldIDoSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("WHAT SHOULD I DO?");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to risk guidance section
                UserReply(user.Name, "What should I do in a risky situation?");
                BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.WhatShouldIDoIntro()));
                Console.WriteLine();

                // Display common risky scenario options
                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. I received a suspicious email");
                CenterWriteLine("2. I think my password is weak");
                CenterWriteLine("3. A website looks unsafe");
                CenterWriteLine("4. I downloaded a strange file");
                CenterWriteLine("5. Someone asked me for an OTP or verification code");
                CenterWriteLine("6. Go to Main Menu");
                CenterWriteLine("7. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string choice = GetChoiceWithAttempts(validator, 1, 7, "Choose an option (1-7): ", responses);
                if (choice == null)
                    return false;

                if (choice == "6")
                    return true;

                if (choice == "7")
                    return ExitNow(user, responses);

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("WHAT SHOULD I DO?");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Provide appropriate guidance based on selected scenario
                switch (choice)
                {
                    case "1":
                        UserReply(user.Name, "I received a suspicious email.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperSuspiciousEmail()));
                        break;

                    case "2":
                        UserReply(user.Name, "I think my password is weak.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(passwordReactions), responses.HelperWeakPassword()));
                        break;

                    case "3":
                        UserReply(user.Name, "A website looks unsafe.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperUnsafeWebsite()));
                        break;

                    case "4":
                        UserReply(user.Name, "I downloaded a strange file.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperDownloadedFile()));
                        break;

                    case "5":
                        UserReply(user.Name, "Someone asked me for an OTP or verification code.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.HelperOtpCode()));
                        break;
                }

                if (!RunSimpleNavigation(user, validator, responses))
                    return false;
            }
        }

        // Provides cybersecurity facts and interesting information to users
        static bool RunDidYouKnowSection(User user, Validator validator, Responses responses)
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("DID YOU KNOW?");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Introduction to facts section
                UserReply(user.Name, "Give me a cybersecurity fact.");
                BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.DidYouKnowIntro()));
                Console.WriteLine();

                // Display fact category options
                Console.ForegroundColor = ConsoleColor.Yellow;
                CenterWriteLine("1. Tell me a password fact");
                CenterWriteLine("2. Tell me a phishing fact");
                CenterWriteLine("3. Tell me a browsing fact");
                CenterWriteLine("4. Give me a random fact");
                CenterWriteLine("5. Go to Main Menu");
                CenterWriteLine("6. Exit the chatbot");
                Console.ResetColor();
                Console.WriteLine();

                string choice = GetChoiceWithAttempts(validator, 1, 6, "Choose an option (1-6): ", responses);
                if (choice == null)
                    return false;

                if (choice == "5")
                    return true;

                if (choice == "6")
                    return ExitNow(user, responses);

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterWriteLine("========================================");
                CenterWriteLine("DID YOU KNOW?");
                CenterWriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                // Provide fact based on selected category
                switch (choice)
                {
                    case "1":
                        UserReply(user.Name, "Tell me a password fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PasswordDidYouKnow()));
                        break;
                    case "2":
                        UserReply(user.Name, "Tell me a phishing fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.PhishingDidYouKnow()));
                        break;
                    case "3":
                        UserReply(user.Name, "Tell me a browsing fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.BrowsingDidYouKnow()));
                        break;
                    case "4":
                        UserReply(user.Name, "Give me a random fact.");
                        BotReply(BuildNaturalResponse(GetRandomReaction(generalReactions), responses.RandomDidYouKnow()));
                        break;
                }

                if (!RunSimpleNavigation(user, validator, responses))
                    return false;
            }
        }

        // Provides specific help for password-related problems and concerns
        static bool RunPasswordHelper(User user, Validator validator, Responses responses)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("PASSWORD HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Introduction to password help section
            UserReply(user.Name, "What should I do if I have a password problem?");
            BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.PasswordHelperIntro()));
            Console.WriteLine();

            // Display common password problem options
            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine("1. I forgot my password");
            CenterWriteLine("2. I reused the same password on several accounts");
            CenterWriteLine("3. I think someone knows my password");
            CenterWriteLine("4. Return to Password Safety");
            CenterWriteLine("5. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            string choice = GetChoiceWithAttempts(validator, 1, 5, "Choose an option (1-5): ", responses);
            if (choice == null)
                return false;

            if (choice == "4")
                return true;

            if (choice == "5")
                return ExitNow(user, responses);

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("PASSWORD HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Provide specific help based on password problem
            switch (choice)
            {
                case "1":
                    UserReply(user.Name, "I forgot my password.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.PasswordHelperForgot()));
                    break;
                case "2":
                    UserReply(user.Name, "I reused the same password on several accounts.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PasswordHelperReused()));
                    break;
                case "3":
                    UserReply(user.Name, "I think someone knows my password.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PasswordHelperCompromised()));
                    break;
            }

            return RunSimpleNavigation(user, validator, responses);
        }

        // Provides specific help for phishing-related concerns and suspicious messages
        static bool RunPhishingHelper(User user, Validator validator, Responses responses)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("PHISHING HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Introduction to phishing help section
            UserReply(user.Name, "What should I do if I receive a suspicious message?");
            BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.PhishingHelperIntro()));
            Console.WriteLine();

            // Display common phishing concern options
            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine("1. It looks urgent and asks me to click a link");
            CenterWriteLine("2. It asks for my banking details");
            CenterWriteLine("3. It looks like it came from my workplace");
            CenterWriteLine("4. Return to Phishing Awareness");
            CenterWriteLine("5. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            string choice = GetChoiceWithAttempts(validator, 1, 5, "Choose an option (1-5): ", responses);
            if (choice == null)
                return false;

            if (choice == "4")
                return true;

            if (choice == "5")
                return ExitNow(user, responses);

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("PHISHING HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Provide specific help based on phishing concern
            switch (choice)
            {
                case "1":
                    UserReply(user.Name, "It looks urgent and asks me to click a link.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingHelperUrgentLink()));
                    break;
                case "2":
                    UserReply(user.Name, "It asks for my banking details.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingHelperBankDetails()));
                    break;
                case "3":
                    UserReply(user.Name, "It looks like it came from my workplace.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.PhishingHelperWorkMessage()));
                    break;
            }

            return RunSimpleNavigation(user, validator, responses);
        }

        // Provides specific help for browsing-related safety concerns and suspicious websites
        static bool RunBrowsingHelper(User user, Validator validator, Responses responses)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("SAFE BROWSING HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Introduction to safe browsing help section
            UserReply(user.Name, "What should I do if a website looks unsafe?");
            BotReply(BuildNaturalResponse(GetRandomReaction(helpReactions), responses.BrowsingHelperIntro()));
            Console.WriteLine();

            // Display common browsing concern options
            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine("1. The website has strange pop-ups");
            CenterWriteLine("2. The address looks misspelled");
            CenterWriteLine("3. It asks me to download something unexpectedly");
            CenterWriteLine("4. Return to Safe Browsing");
            CenterWriteLine("5. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            string choice = GetChoiceWithAttempts(validator, 1, 5, "Choose an option (1-5): ", responses);
            if (choice == null)
                return false;

            if (choice == "4")
                return true;

            if (choice == "5")
                return ExitNow(user, responses);

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            CenterWriteLine("========================================");
            CenterWriteLine("SAFE BROWSING HELP");
            CenterWriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            // Provide specific help based on browsing concern
            switch (choice)
            {
                case "1":
                    UserReply(user.Name, "The website has strange pop-ups.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.BrowsingHelperPopups()));
                    break;
                case "2":
                    UserReply(user.Name, "The address looks misspelled.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.BrowsingHelperMisspelledUrl()));
                    break;
                case "3":
                    UserReply(user.Name, "It asks me to download something unexpectedly.");
                    BotReply(BuildNaturalResponse(GetRandomReaction(warningReactions), responses.BrowsingHelperUnexpectedDownload()));
                    break;
            }

            return RunSimpleNavigation(user, validator, responses);
        }

        // Provides simple two-option navigation (Main Menu or Exit) after content display
        static bool RunSimpleNavigation(User user, Validator validator, Responses responses)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            CenterWriteLine("----------------------------------------");
            Console.ResetColor();

            BotReply("What would you like to do next?");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine("1. Go to Main Menu");
            CenterWriteLine("2. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            string navChoice = GetChoiceWithAttempts(validator, 1, 2, "Choose an option (1-2): ", responses);
            if (navChoice == null)
                return false;

            if (navChoice == "1")
                return true;

            return ExitNow(user, responses);
        }

        // Provides three-option navigation (Stay, Main Menu, or Exit) for section continuity
        static int RunFollowUpNavigation(User user, Validator validator, Responses responses, string topicName)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            CenterWriteLine("----------------------------------------");
            Console.ResetColor();

            BotReply("What would you like to do next?");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWriteLine($"1. Stay in {topicName}");
            CenterWriteLine("2. Go to Main Menu");
            CenterWriteLine("3. Exit the chatbot");
            Console.ResetColor();
            Console.WriteLine();

            string navChoice = GetChoiceWithAttempts(validator, 1, 3, "Choose an option (1-3): ", responses);
            if (navChoice == null)
                return 3;

            if (navChoice == "1")
                return 1;

            if (navChoice == "2")
                return 2;

            ExitNow(user, responses);
            return 3;
        }

        // Gets non-empty user input with up to 3 attempts for open-ended questions
        static string GetMeaningfulInput(User user, Validator validator, Responses responses, string prompt)
        {
            for (int attempts = 1; attempts <= 3; attempts++)
            {
                int attemptsLeft = 3 - attempts;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prompt);
                Console.ResetColor();

                string input = Console.ReadLine();

                if (validator.IsMeaningfulInput(input))
                    return input.Trim();

                BotReply($"{responses.Default()} Attempts left: {attemptsLeft}.");
            }

            BotReply(responses.AttemptsExceeded());
            return null;
        }

        // Generates contextual responses based on user input, topic detection, and conversation stage
        static string GetConversationResponse(string input, User user, Responses responses, ConversationState state)
        {
            string lowered = input.ToLower().Trim();

            // Handle common conversational intents
            if (lowered.Contains("how are you"))
                return BuildNaturalResponse(GetRandomReaction(generalReactions), responses.HowAreYou(user.Name));

            if (lowered.Contains("purpose"))
                return BuildNaturalResponse(GetRandomReaction(helpReactions), responses.Purpose());

            if (lowered.Contains("help") || lowered.Contains("what can i ask"))
                return BuildNaturalResponse(GetRandomReaction(helpReactions), responses.Help());

            // Detect and update conversation topic
            string detectedTopic = DetectTopic(lowered, state.CurrentTopic);
            if (!string.IsNullOrEmpty(detectedTopic))
                state.CurrentTopic = detectedTopic;

            // Check for specific intent responses
            string directAnswer = GetDirectIntentResponse(lowered, state.CurrentTopic, responses);

            state.Step++;

            if (!string.IsNullOrEmpty(directAnswer))
                return directAnswer;

            // Fall back to stage-based response
            return GetConversationStageResponse(state.CurrentTopic, state.Step, responses);
        }

        // Analyzes user input to determine the cybersecurity topic being discussed
        static string DetectTopic(string lowered, string currentTopic)
        {
            // Check for password-related keywords
            if (lowered.Contains("password") ||
                lowered.Contains("passcode") ||
                lowered.Contains("login") ||
                lowered.Contains("2fa") ||
                lowered.Contains("two factor") ||
                lowered.Contains("two-factor") ||
                lowered.Contains("authenticator"))
                return "password";

            // Check for phishing-related keywords
            if (lowered.Contains("phishing") ||
                lowered.Contains("scam") ||
                lowered.Contains("fake email") ||
                lowered.Contains("suspicious email") ||
                lowered.Contains("fake link") ||
                lowered.Contains("clicked a link") ||
                lowered.Contains("otp") ||
                lowered.Contains("verification code"))
                return "phishing";

            // Check for browsing-related keywords
            if (lowered.Contains("browsing") ||
                lowered.Contains("website") ||
                lowered.Contains("download") ||
                lowered.Contains("popup") ||
                lowered.Contains("pop-up") ||
                lowered.Contains("browser") ||
                lowered.Contains("unsafe site"))
                return "browsing";

            return currentTopic;
        }

        // Provides specific answers based on detected intent within the current topic context
        static string GetDirectIntentResponse(string lowered, string currentTopic, Responses responses)
        {
            // Password topic intent responses
            if (currentTopic == "password")
            {
                if (lowered.Contains("2fa") || lowered.Contains("two factor") || lowered.Contains("two-factor"))
                    return BuildNaturalResponse(GetRandomReaction(passwordReactions),
                        "2FA means two-factor authentication. It adds a second layer of security after your password, such as a code from an authenticator app, fingerprint, or trusted device. Even if someone steals your password, 2FA can still help block them.");

                if (lowered.Contains("password manager"))
                    return BuildNaturalResponse(GetRandomReaction(helpReactions),
                        "A password manager stores your passwords securely and helps generate strong unique ones for each account. That means you do not need to memorize every password, and you reduce the danger of reusing the same one everywhere.");

                if (lowered.Contains("forgot") && lowered.Contains("password"))
                    return BuildNaturalResponse(GetRandomReaction(helpReactions),
                        "If you forgot your password, use the official password reset option on the real website or app. Do not trust reset links sent by random messages unless you requested them yourself.");

                if (lowered.Contains("reuse") || lowered.Contains("reused") || lowered.Contains("same password"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Reusing the same password is risky because one breached account can lead attackers to your other accounts. Start by changing your email password first, then your banking and social accounts.");

                if (lowered.Contains("strong password") || lowered.Contains("make a strong password") || lowered.Contains("create a strong password"))
                    return BuildNaturalResponse(GetRandomReaction(passwordReactions),
                        "A strong password should be long, unique, and hard to guess. A good approach is to use a passphrase with a mix of words, numbers, and symbols, rather than one short complicated word.");

                if (lowered.Contains("someone knows my password") || lowered.Contains("my password was stolen") || lowered.Contains("password leaked"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "If you think someone knows your password, change it immediately, sign out of other sessions if the account allows it, and enable 2FA right away. Also check whether the same password was used on any other account.");

                if (lowered.Contains("authenticator"))
                    return BuildNaturalResponse(GetRandomReaction(helpReactions),
                        "An authenticator app creates time-based codes on your device for login verification. It is usually safer than receiving codes by SMS because text messages can sometimes be intercepted or socially engineered.");
            }

            // Phishing topic intent responses
            if (currentTopic == "phishing")
            {
                if (lowered.Contains("fake link") || lowered.Contains("spot a fake link") || lowered.Contains("suspicious link"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "To spot a fake link, look carefully for misspellings, extra words, strange numbers, or unusual endings in the address. A phishing link often looks close to the real site, but not exactly the same.");

                if (lowered.Contains("clicked") && lowered.Contains("link"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "If you already clicked a suspicious link, stop interacting with the page immediately. Do not enter any details, close the page, run a security scan if needed, and change your password if you typed it anywhere.");

                if (lowered.Contains("entered") && (lowered.Contains("password") || lowered.Contains("details") || lowered.Contains("information")))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "If you entered your details on a suspicious page, treat the account as compromised. Change the password immediately from the official website, enable 2FA, and monitor the account for unusual activity.");

                if (lowered.Contains("otp") || lowered.Contains("verification code"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Never share an OTP or verification code with anyone. Real companies usually do not call or message you asking for the code that was sent to your phone. That code is meant for you only.");

                if (lowered.Contains("phishing email") || lowered.Contains("fake email") || lowered.Contains("suspicious email"))
                    return BuildNaturalResponse(GetRandomReaction(phishingReactions),
                        "A phishing email often creates urgency, asks you to click quickly, uses threatening language, or requests personal details. Always check the sender address carefully, not just the display name.");

                if (lowered.Contains("bank") || lowered.Contains("banking"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Be extra careful with messages that claim to be from your bank. Instead of using the message link, open your banking app or type the bank website address manually yourself.");

                if (lowered.Contains("attachment"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Suspicious attachments can contain malware or fake login documents. If you were not expecting the file, verify it with the sender through a trusted contact method before opening it.");
            }

            // Browsing topic intent responses
            if (currentTopic == "browsing")
            {
                if (lowered.Contains("safe site") || lowered.Contains("safe website") || lowered.Contains("how do i know if a site is safe"))
                    return BuildNaturalResponse(GetRandomReaction(browsingReactions),
                        "A safer website usually has a correct web address, uses HTTPS, looks professional, and does not pressure you with strange pop-ups or urgent download messages. Even then, you should still stay cautious.");

                if (lowered.Contains("popup") || lowered.Contains("pop-up"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "If a website shows aggressive pop-ups, do not click random close or download buttons inside the page. Close the browser tab carefully and avoid interacting with the pop-up content.");

                if (lowered.Contains("download"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Only download files from official or trusted websites. Fake download buttons are very common, especially on free streaming, cracked software, and random converter websites.");

                if (lowered.Contains("https"))
                    return BuildNaturalResponse(GetRandomReaction(helpReactions),
                        "HTTPS means the connection between your browser and the website is encrypted. It is a good sign, but it does not automatically mean the website itself is trustworthy.");

                if (lowered.Contains("misspelled") || lowered.Contains("wrong url") || lowered.Contains("fake website"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "A misspelled web address is a major warning sign. Attackers often create addresses that look nearly real, hoping you will not notice the tiny difference.");

                if (lowered.Contains("browser update"))
                    return BuildNaturalResponse(GetRandomReaction(helpReactions),
                        "Keeping your browser updated helps protect you from known security flaws. Updates often fix vulnerabilities that attackers try to exploit on unsafe websites.");

                if (lowered.Contains("public wifi") || lowered.Contains("public wi-fi"))
                    return BuildNaturalResponse(GetRandomReaction(warningReactions),
                        "Be careful when browsing on public Wi-Fi. Avoid logging into banking or sensitive accounts unless necessary, and prefer trusted networks or mobile data for important activity.");
            }

            return null;
        }

        // Returns staged conversational responses that build knowledge progressively across steps
        static string GetConversationStageResponse(string topic, int step, Responses responses)
        {
            // Password topic staged responses
            switch (topic)
            {
                case "password":
                    switch (step)
                    {
                        case 1:
                            return BuildNaturalResponse(GetRandomReaction(passwordReactions), "Let's start simple. Password safety means creating passwords that are hard to guess and not reusing the same one everywhere.");
                        case 2:
                            return BuildNaturalResponse(GetRandomReaction(passwordReactions), "Why this matters is that weak or reused passwords make it easier for attackers to enter more than one of your accounts.");
                        case 3:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "A real-life example is when someone uses the same password for email, Facebook, and banking. If one site is breached, the attacker may try that same password on everything else.");
                        case 4:
                            return BuildNaturalResponse(GetRandomReaction(helpReactions), "What you should do next is use a long password or passphrase, make every important account unique, and start with your email account first because it protects many other accounts.");
                        case 5:
                            return BuildNaturalResponse(GetRandomReaction(praiseReactions), "A pro tip is to use a password manager. It helps you create and store strong unique passwords without needing to memorize every one.");
                        default:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "To sum it up: strong passwords should be long, unique, and never reused. If you want, start a new conversation and ask me something more specific about passwords.");
                    }

                // Phishing topic staged responses
                case "phishing":
                    switch (step)
                    {
                        case 1:
                            return BuildNaturalResponse(GetRandomReaction(phishingReactions), "Let's begin with the basics. Phishing is when a scammer pretends to be a trusted person or company to trick you into clicking, downloading, or sharing private information.");
                        case 2:
                            return BuildNaturalResponse(GetRandomReaction(warningReactions), "Why it matters is that phishing attacks often look urgent and believable, which causes people to panic before they think.");
                        case 3:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "A real-life example is a fake bank email saying your account will be blocked in 10 minutes unless you click a link. The message looks serious, but the goal is to steal your login details.");
                        case 4:
                            return BuildNaturalResponse(GetRandomReaction(helpReactions), "What you should do is stop, check the sender carefully, avoid clicking links, and contact the company through its real website or official phone number.");
                        case 5:
                            return BuildNaturalResponse(GetRandomReaction(warningReactions), "A pro tip is to watch for urgency, spelling errors, strange links, and requests for passwords, OTPs, or banking details. Those are classic phishing signs.");
                        default:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "To summarize: phishing tries to rush or scare you into acting. The safest habit is always verify first and click later, or not at all.");
                    }

                // Browsing topic staged responses
                case "browsing":
                    switch (step)
                    {
                        case 1:
                            return BuildNaturalResponse(GetRandomReaction(browsingReactions), "Let's start simple. Safe browsing means being careful about the websites you open, the files you download, and the pop-ups you trust.");
                        case 2:
                            return BuildNaturalResponse(GetRandomReaction(warningReactions), "Why this matters is that one unsafe site can lead to malware, fake forms, stolen passwords, or scams.");
                        case 3:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "A real-life example is visiting a fake shopping site with a slightly misspelled address. It may look real, but once you enter your card details, they can be stolen.");
                        case 4:
                            return BuildNaturalResponse(GetRandomReaction(helpReactions), "What you should do is check the web address carefully, look for HTTPS, avoid strange download buttons, and leave any page that feels suspicious.");
                        case 5:
                            return BuildNaturalResponse(GetRandomReaction(browsingReactions), "A pro tip is to download apps and software only from official sources. Random download sites often hide fake buttons and dangerous files.");
                        default:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "To sum it up: trust the address bar, not just the design of the website. A page can look real and still be dangerous.");
                    }

                // General topic staged responses (default)
                default:
                    switch (step)
                    {
                        case 1:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "I can help with passwords, phishing, safe browsing, suspicious links, OTP scams, and risky downloads. Start with any one of those.");
                        case 2:
                            return BuildNaturalResponse(GetRandomReaction(helpReactions), "A good way to stay safe online is to slow down before clicking anything unexpected, especially emails, links, and pop-ups.");
                        case 3:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "In real life, most cyber incidents start with a simple mistake like trusting a fake message or reusing a weak password.");
                        case 4:
                            return BuildNaturalResponse(GetRandomReaction(helpReactions), "A smart next step is to focus on one topic at a time. Password safety, phishing awareness, and safe browsing are the best starting points.");
                        case 5:
                            return BuildNaturalResponse(GetRandomReaction(praiseReactions), "You're asking the right kinds of questions. Learning the warning signs early makes a big difference in real situations.");
                        default:
                            return BuildNaturalResponse(GetRandomReaction(generalReactions), "That's a solid conversation so far. You can start a new one and go deeper into passwords, phishing, or safe browsing.");
                    }
            }
        }

        // Gets validated numeric menu choice from user with up to 3 attempts
        static string GetChoiceWithAttempts(Validator validator, int min, int max, string prompt, Responses responses)
        {
            for (int attempts = 1; attempts <= 3; attempts++)
            {
                int attemptsLeft = 3 - attempts;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prompt);
                Console.ResetColor();

                string choice = Console.ReadLine();

                if (validator.IsValidMenuChoice(choice, min, max))
                    return choice;

                BotReply($"{responses.Default()} Attempts left: {attemptsLeft}.");
            }

            BotReply(responses.AttemptsExceeded());
            return null;
        }

        // Handles graceful exit from the chatbot with farewell message
        static bool ExitNow(User user, Responses responses)
        {
            UserReply(user.Name, "I'd like to exit now.");
            BotReply(responses.Exit(user.Name));
            return false;
        }

        // Returns a random reaction phrase from the specified array
        static string GetRandomReaction(string[] reactionArray)
        {
            int index = random.Next(reactionArray.Length);
            return reactionArray[index];
        }

        // Combines a reaction phrase with a message for natural-sounding responses
        static string BuildNaturalResponse(string reaction, string message)
        {
            return $"{reaction} — {message}";
        }

        // Displays an animated loading bar to enhance user experience
        static void ShowLoadingBar()
        {
            ConsoleView.ShowLoadingBar();
        }

        // Displays a bot message using the console view formatting
        static void BotReply(string message)
        {
            ConsoleView.BotReply(message);
        }

        // Displays a user message with the user's name using console view formatting
        static void UserReply(string userName, string message)
        {
            ConsoleView.UserReply(userName, message);
        }

        // Displays text with a typing animation effect for enhanced engagement
        static void TypeText(string text)
        {
            ConsoleView.TypeText(text);
        }

        // Centers and writes text horizontally in the console window
        static void CenterWriteLine(string text)
        {
            ConsoleView.CenterWriteLine(text);
        }
    }
}