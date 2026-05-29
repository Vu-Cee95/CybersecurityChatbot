// Required namespaces for random selection and dictionary collections
using System;
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    // Service that provides all chatbot response content
    // Contains curated cybersecurity educational content organized by topic and response type
    // Handles general responses, definitions, prevention tips, examples, emergency guidance, and session summaries
    public class ResponseService
    {
        // Random number generator for selecting varied responses
        // Prevents the bot from sounding repetitive by choosing randomly from response lists
        private readonly Random random = new Random();

        // Dictionary of general cybersecurity responses organized by topic
        // Each topic contains a list of 3 varied responses that are randomly selected
        // Provides educational content without repeating the exact same message
        private readonly Dictionary<string, List<string>> generalResponses = new Dictionary<string, List<string>>
        {
            // Password safety responses: advice on strong passwords, unique passwords, avoiding personal info
            {
                "password",
                new List<string>
                {
                    "A strong password should be long, unique, and difficult to guess. A passphrase is usually better than a short password.",
                    "Use a different password for every important account. This protects you if one account is leaked.",
                    "Avoid using your name, birthday, school name, or simple words as passwords."
                }
            },
            // Phishing awareness responses: explaining phishing tactics, urgency manipulation, verification tips
            {
                "phishing",
                new List<string>
                {
                    "Phishing happens when criminals pretend to be trusted people or companies to trick you into clicking links or sharing information.",
                    "Be careful with urgent messages that pressure you to act quickly. Scammers often use fear and pressure.",
                    "Always check the sender, the link, and the message wording before clicking anything."
                }
            },
            // Scam awareness responses: recognizing scam patterns, protecting financial information, verification advice
            {
                "scam",
                new List<string>
                {
                    "Online scams often promise prizes, refunds, jobs, or urgent account fixes. Always verify through official channels.",
                    "Never share banking details, passwords, or OTP codes with someone who contacts you unexpectedly.",
                    "If something sounds too good to be true, pause and verify before taking action."
                }
            },
            // Privacy protection responses: controlling personal information, social media safety, privacy settings
            {
                "privacy",
                new List<string>
                {
                    "Privacy means controlling what personal information you share and who can access it.",
                    "Avoid posting sensitive details such as your ID number, address, phone number, or banking information online.",
                    "Review your social media privacy settings regularly so you know who can see your posts."
                }
            },
            // Safe browsing responses: website verification, HTTPS checking, avoiding suspicious downloads
            {
                "safe browsing",
                new List<string>
                {
                    "Safe browsing means checking websites carefully before entering information or downloading files.",
                    "Look for HTTPS, avoid suspicious pop-ups, and do not download files from unknown websites.",
                    "Be careful with shortened links because they can hide the real website destination."
                }
            },
            // Malware awareness responses: understanding malicious software, prevention, keeping systems updated
            {
                "malware",
                new List<string>
                {
                    "Malware is harmful software that can steal information, damage files, or spy on your activity.",
                    "Avoid opening suspicious attachments or downloading cracked software.",
                    "Keep your operating system, browser, and antivirus updated to reduce malware risks."
                }
            },
            // Two-factor authentication responses: explaining 2FA, recommended usage, authenticator app benefits
            {
                "2fa",
                new List<string>
                {
                    "Two-factor authentication adds another security step after your password.",
                    "Use 2FA on important accounts such as email, banking, social media, and cloud storage.",
                    "Authenticator apps are often safer than SMS codes because phone numbers can be targeted through SIM-swap scams."
                }
            }
        };

        // Dictionary of cybersecurity term definitions
        // Provides clear, simple explanations of key cybersecurity concepts
        private readonly Dictionary<string, string> definitions = new Dictionary<string, string>
        {
            { "password", "A password is a secret code used to prove that you are allowed to access an account or system." },
            { "phishing", "Phishing is a cyberattack where criminals pretend to be trusted organisations or people to trick you into sharing sensitive information." },
            { "scam", "A scam is a dishonest trick designed to steal money, personal information, or account access." },
            { "privacy", "Privacy is the protection and control of your personal information, especially online." },
            { "safe browsing", "Safe browsing means using the internet carefully by checking links, websites, downloads, and security warnings before taking action." },
            { "malware", "Malware is malicious software created to harm a device, steal information, or spy on a user." },
            { "2fa", "Two-factor authentication is a login method that requires your password plus a second proof, such as a code or authenticator app." }
        };

        // Dictionary of prevention tips for each cybersecurity topic
        // Provides actionable advice users can immediately apply
        private readonly Dictionary<string, string> preventionTips = new Dictionary<string, string>
        {
            { "password", "Use long unique passwords, avoid personal details, and store them in a trusted password manager." },
            { "phishing", "Check the sender address, avoid urgent links, hover over links before clicking, and visit websites directly instead of using suspicious links." },
            { "scam", "Do not rush, verify the source, never share OTPs, and contact the organisation through official numbers or websites." },
            { "privacy", "Limit what you share online, review app permissions, strengthen privacy settings, and avoid posting sensitive personal details." },
            { "safe browsing", "Use trusted websites, check for HTTPS, avoid suspicious pop-ups, and never download unknown files." },
            { "malware", "Install updates, use antivirus protection, avoid unknown downloads, and do not open suspicious attachments." },
            { "2fa", "Enable 2FA on important accounts and use an authenticator app where possible." }
        };

        // Dictionary of real-world examples for each cybersecurity topic
        // Provides concrete scenarios that users can relate to and learn from
        private readonly Dictionary<string, string> examples = new Dictionary<string, string>
        {
            { "password", "Example: Instead of using 'Vusi123', use a longer passphrase like 'BlueRiver!Drives7Clouds'. It is longer and harder to guess." },
            { "phishing", "Example: You receive an email saying your bank account will be closed unless you click a link immediately. That urgency is a warning sign." },
            { "scam", "Example: Someone says you won a prize but must first pay a release fee. That is a common scam pattern." },
            { "privacy", "Example: Posting your ID number or home address online can allow criminals to impersonate you or target you." },
            { "safe browsing", "Example: A website with many pop-ups and strange download buttons should be treated as suspicious." },
            { "malware", "Example: A free cracked program may secretly install spyware or ransomware on your device." },
            { "2fa", "Example: Even if someone steals your password, they still need your second verification code to log in." }
        };

        // Routes topic requests to the appropriate response type based on intent
        // Handles definition, prevention, example, and general response types
        // Falls back to default response if topic is empty or not found
        public string GetTopicResponse(string topic, string intent)
        {
            // Return default if no topic specified
            if (string.IsNullOrWhiteSpace(topic))
            {
                return GetDefaultResponse();
            }

            // Normalize topic to lowercase for dictionary lookup
            topic = topic.ToLower();

            // Route to definition response
            if (intent == "definition")
            {
                return GetDefinitionResponse(topic);
            }

            // Route to prevention tip response
            if (intent == "prevention")
            {
                return GetPreventionResponse(topic);
            }

            // Route to example response
            if (intent == "example")
            {
                return GetExampleResponse(topic);
            }

            // Default to random general response for the topic
            return GetRandomGeneralResponse(topic);
        }

        // Returns a clear definition for the specified topic
        // Includes a follow-up prompt offering example or safety tip
        public string GetDefinitionResponse(string topic)
        {
            if (definitions.ContainsKey(topic))
            {
                return $"Here is a clear explanation:\n\n{definitions[topic]}\n\nWould you like an example or a safety tip?";
            }

            return GetDefaultResponse();
        }

        // Returns prevention advice for the specified topic
        // Includes a general reminder to pause, check, and verify
        public string GetPreventionResponse(string topic)
        {
            if (preventionTips.ContainsKey(topic))
            {
                return $"Here is how you can protect yourself:\n\n• {preventionTips[topic]}\n\nA good rule is to pause, check, and verify before sharing information or clicking anything.";
            }

            return GetDefaultResponse();
        }

        // Returns a real-world example for the specified topic
        // Includes a follow-up prompt offering another example or prevention tip
        public string GetExampleResponse(string topic)
        {
            if (examples.ContainsKey(topic))
            {
                return $"{examples[topic]}\n\nWould you like another example or a prevention tip?";
            }

            return GetDefaultResponse();
        }

        // Selects a random general response from the topic's response list
        // Provides variety so the bot doesn't repeat the same message
        // Includes a prompt for further exploration (example, tip, more details)
        public string GetRandomGeneralResponse(string topic)
        {
            if (generalResponses.ContainsKey(topic))
            {
                // Get the list of responses for this topic
                List<string> responses = generalResponses[topic];
                // Pick a random index for variety
                int index = random.Next(responses.Count);

                return $"{responses[index]}\n\nYou can ask me for an example, a prevention tip, or more details.";
            }

            return GetDefaultResponse();
        }

        // Returns step-by-step emergency guidance based on the type of security incident
        // Each case provides specific, actionable steps for different emergency scenarios
        // Covers: clicked links, shared OTPs, shared passwords, downloaded files, hacked accounts
        public string GetEmergencyResponse(string emergencyType)
        {
            switch (emergencyType)
            {
                // Guidance for clicking suspicious links
                case "clicked link":
                    return "Thanks for being honest. If you clicked a suspicious link, do this immediately:\n\n• Do not enter any more information on that website.\n• Close the page.\n• Change the password of the affected account from the official website or app.\n• Enable 2FA if it is available.\n• Watch for unusual account activity.\n\nIf it involved banking, contact your bank using the official number immediately.";

                // Guidance for sharing one-time passwords/PINs
                case "shared otp":
                    return "This is serious. If you shared an OTP, act quickly:\n\n• Contact the affected service or bank immediately.\n• Change your password.\n• Check for unauthorised transactions or changes.\n• Do not share any more codes with anyone.\n\nRemember: real support staff should not ask for your OTP.";

                // Guidance for sharing account passwords
                case "shared password":
                    return "If you shared your password, change it immediately:\n\n• Use the official website or app.\n• Choose a new unique password.\n• Sign out of all devices if the option is available.\n• Enable 2FA.\n• Check account recovery details such as phone number and email.";

                // Guidance for downloading suspicious files
                case "downloaded file":
                    return "If you downloaded a suspicious file, take these steps:\n\n• Do not open the file.\n• Delete it if you have not opened it.\n• Run a full antivirus scan.\n• Update your operating system and browser.\n• If the device behaves strangely, disconnect from the internet and get technical support.";

                // Guidance for compromised/hacked accounts
                case "hacked account":
                    return "If you think your account was hacked:\n\n• Change the password immediately.\n• Enable 2FA.\n• Review recent login activity.\n• Remove unknown devices or sessions.\n• Check recovery email and phone number.\n• Warn contacts not to trust strange messages from your account.";

                // Generic emergency guidance when type cannot be determined
                default:
                    return "If something suspicious happened, pause and act carefully:\n\n• Stop interacting with the suspicious message or website.\n• Change affected passwords.\n• Enable 2FA.\n• Contact the official organisation directly.\n• Monitor your accounts for unusual activity.";
            }
        }

        // Generates a session summary with user information and conversation statistics
        // Includes name, favourite topic, last topic, sentiment, message count, and recommendations
        // Handles null/empty values with "not set yet" or "not detected yet" fallbacks
        public string GetSessionSummary(string name, string favouriteTopic, string lastTopic, string lastSentiment, int messageCount)
        {
            // Use fallback text for unset values
            string favourite = string.IsNullOrWhiteSpace(favouriteTopic) ? "not set yet" : favouriteTopic;
            string last = string.IsNullOrWhiteSpace(lastTopic) ? "not discussed yet" : lastTopic;
            string sentiment = string.IsNullOrWhiteSpace(lastSentiment) ? "not detected yet" : lastSentiment;

            return $"Here is your ChatBot session summary:\n\n• Name: {name}\n• Favourite topic: {favourite}\n• Last topic discussed: {last}\n• Last mood detected: {sentiment}\n• Messages processed: {messageCount}\n\nRecommended next step: Ask for a practical example or prevention tip about {last}.";
        }

        // Returns a help message listing example questions users can ask
        // Provides guidance on bot capabilities and conversation starters
        public string GetHelpResponse()
        {
            return "You can ask me questions like:\n\n• What is phishing?\n• How do I create a strong password?\n• How do I avoid scams?\n• Give me an example of malware\n• I clicked a suspicious link\n• What do you remember about me?\n• Summarise this chat";
        }

        // Returns a random default response when input cannot be understood
        // Provides variety with 3 different fallback messages that guide users toward valid topics
        // Helps users understand what the bot can help with
        public string GetDefaultResponse()
        {
            // List of varied default responses to avoid repetition
            List<string> defaultResponses = new List<string>
            {
                "I could not understand that clearly. Try asking about passwords, phishing, scams, privacy, safe browsing, malware, or 2FA.",
                "That sounds interesting, but I am focused on cybersecurity awareness. You can ask me about scams, passwords, privacy, malware, phishing, safe browsing, or 2FA.",
                "I may need you to rephrase that. For example, try: 'What is phishing?' or 'How do I avoid scams?'"
            };

            // Randomly select a default response for variety
            int index = random.Next(defaultResponses.Count);
            return defaultResponses[index];
        }
    }
}