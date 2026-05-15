using System;
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class ResponseService
    {
        private readonly Random random = new Random();

        private readonly Dictionary<string, List<string>> generalResponses = new Dictionary<string, List<string>>
        {
            {
                "password",
                new List<string>
                {
                    "A strong password should be long, unique, and difficult to guess. A passphrase is usually better than a short password.",
                    "Use a different password for every important account. This protects you if one account is leaked.",
                    "Avoid using your name, birthday, school name, or simple words as passwords."
                }
            },
            {
                "phishing",
                new List<string>
                {
                    "Phishing happens when criminals pretend to be trusted people or companies to trick you into clicking links or sharing information.",
                    "Be careful with urgent messages that pressure you to act quickly. Scammers often use fear and pressure.",
                    "Always check the sender, the link, and the message wording before clicking anything."
                }
            },
            {
                "scam",
                new List<string>
                {
                    "Online scams often promise prizes, refunds, jobs, or urgent account fixes. Always verify through official channels.",
                    "Never share banking details, passwords, or OTP codes with someone who contacts you unexpectedly.",
                    "If something sounds too good to be true, pause and verify before taking action."
                }
            },
            {
                "privacy",
                new List<string>
                {
                    "Privacy means controlling what personal information you share and who can access it.",
                    "Avoid posting sensitive details such as your ID number, address, phone number, or banking information online.",
                    "Review your social media privacy settings regularly so you know who can see your posts."
                }
            },
            {
                "safe browsing",
                new List<string>
                {
                    "Safe browsing means checking websites carefully before entering information or downloading files.",
                    "Look for HTTPS, avoid suspicious pop-ups, and do not download files from unknown websites.",
                    "Be careful with shortened links because they can hide the real website destination."
                }
            },
            {
                "malware",
                new List<string>
                {
                    "Malware is harmful software that can steal information, damage files, or spy on your activity.",
                    "Avoid opening suspicious attachments or downloading cracked software.",
                    "Keep your operating system, browser, and antivirus updated to reduce malware risks."
                }
            },
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

        public string GetTopicResponse(string topic, string intent)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return GetDefaultResponse();
            }

            topic = topic.ToLower();

            if (intent == "definition")
            {
                return GetDefinitionResponse(topic);
            }

            if (intent == "prevention")
            {
                return GetPreventionResponse(topic);
            }

            if (intent == "example")
            {
                return GetExampleResponse(topic);
            }

            return GetRandomGeneralResponse(topic);
        }

        public string GetDefinitionResponse(string topic)
        {
            if (definitions.ContainsKey(topic))
            {
                return $"Here is a clear explanation:\n\n{definitions[topic]}\n\nWould you like an example or a safety tip?";
            }

            return GetDefaultResponse();
        }

        public string GetPreventionResponse(string topic)
        {
            if (preventionTips.ContainsKey(topic))
            {
                return $"Here is how you can protect yourself:\n\n• {preventionTips[topic]}\n\nA good rule is to pause, check, and verify before sharing information or clicking anything.";
            }

            return GetDefaultResponse();
        }

        public string GetExampleResponse(string topic)
        {
            if (examples.ContainsKey(topic))
            {
                return $"{examples[topic]}\n\nWould you like another example or a prevention tip?";
            }

            return GetDefaultResponse();
        }

        public string GetRandomGeneralResponse(string topic)
        {
            if (generalResponses.ContainsKey(topic))
            {
                List<string> responses = generalResponses[topic];
                int index = random.Next(responses.Count);

                return $"{responses[index]}\n\nYou can ask me for an example, a prevention tip, or more details.";
            }

            return GetDefaultResponse();
        }

        public string GetEmergencyResponse(string emergencyType)
        {
            switch (emergencyType)
            {
                case "clicked link":
                    return "Thanks for being honest. If you clicked a suspicious link, do this immediately:\n\n• Do not enter any more information on that website.\n• Close the page.\n• Change the password of the affected account from the official website or app.\n• Enable 2FA if it is available.\n• Watch for unusual account activity.\n\nIf it involved banking, contact your bank using the official number immediately.";

                case "shared otp":
                    return "This is serious. If you shared an OTP, act quickly:\n\n• Contact the affected service or bank immediately.\n• Change your password.\n• Check for unauthorised transactions or changes.\n• Do not share any more codes with anyone.\n\nRemember: real support staff should not ask for your OTP.";

                case "shared password":
                    return "If you shared your password, change it immediately:\n\n• Use the official website or app.\n• Choose a new unique password.\n• Sign out of all devices if the option is available.\n• Enable 2FA.\n• Check account recovery details such as phone number and email.";

                case "downloaded file":
                    return "If you downloaded a suspicious file, take these steps:\n\n• Do not open the file.\n• Delete it if you have not opened it.\n• Run a full antivirus scan.\n• Update your operating system and browser.\n• If the device behaves strangely, disconnect from the internet and get technical support.";

                case "hacked account":
                    return "If you think your account was hacked:\n\n• Change the password immediately.\n• Enable 2FA.\n• Review recent login activity.\n• Remove unknown devices or sessions.\n• Check recovery email and phone number.\n• Warn contacts not to trust strange messages from your account.";

                default:
                    return "If something suspicious happened, pause and act carefully:\n\n• Stop interacting with the suspicious message or website.\n• Change affected passwords.\n• Enable 2FA.\n• Contact the official organisation directly.\n• Monitor your accounts for unusual activity.";
            }
        }

        public string GetSessionSummary(string name, string favouriteTopic, string lastTopic, string lastSentiment, int messageCount)
        {
            string favourite = string.IsNullOrWhiteSpace(favouriteTopic) ? "not set yet" : favouriteTopic;
            string last = string.IsNullOrWhiteSpace(lastTopic) ? "not discussed yet" : lastTopic;
            string sentiment = string.IsNullOrWhiteSpace(lastSentiment) ? "not detected yet" : lastSentiment;

            return $"Here is your ChatBot session summary:\n\n• Name: {name}\n• Favourite topic: {favourite}\n• Last topic discussed: {last}\n• Last mood detected: {sentiment}\n• Messages processed: {messageCount}\n\nRecommended next step: Ask for a practical example or prevention tip about {last}.";
        }

        public string GetHelpResponse()
        {
            return "You can ask me questions like:\n\n• What is phishing?\n• How do I create a strong password?\n• How do I avoid scams?\n• Give me an example of malware\n• I clicked a suspicious link\n• What do you remember about me?\n• Summarise this chat";
        }

        public string GetDefaultResponse()
        {
            List<string> defaultResponses = new List<string>
            {
                "I could not understand that clearly. Try asking about passwords, phishing, scams, privacy, safe browsing, malware, or 2FA.",
                "That sounds interesting, but I am focused on cybersecurity awareness. You can ask me about scams, passwords, privacy, malware, phishing, safe browsing, or 2FA.",
                "I may need you to rephrase that. For example, try: 'What is phishing?' or 'How do I avoid scams?'"
            };

            int index = random.Next(defaultResponses.Count);
            return defaultResponses[index];
        }
    }
}