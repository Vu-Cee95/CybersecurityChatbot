using System;
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class ResponseService
    {
        private readonly Random random = new Random();

        private readonly Dictionary<string, List<string>> topicResponses = new Dictionary<string, List<string>>
        {
            {
                "password",
                new List<string>
                {
                    "A strong password should be long, unique, and difficult to guess. Avoid using your name, birthday, or simple words.",
                    "Use a different password for each account. If one account is hacked, the others will still be safer.",
                    "A good password can include uppercase letters, lowercase letters, numbers, and symbols. You can also use a passphrase that is easy for you to remember but hard for others to guess."
                }
            },
            {
                "phishing",
                new List<string>
                {
                    "Phishing is when criminals pretend to be trusted organisations to trick you into sharing private information.",
                    "Always check the sender’s email address before clicking links or opening attachments.",
                    "Be careful of urgent messages that pressure you to act quickly. Scammers often use fear to make people click without thinking."
                }
            },
            {
                "scam",
                new List<string>
                {
                    "Online scams often promise prizes, refunds, or urgent account fixes. Always verify before sharing information.",
                    "Never send banking details, passwords, or OTP codes to someone who contacts you unexpectedly.",
                    "If an offer sounds too good to be true, pause and verify it through official channels first."
                }
            },
            {
                "privacy",
                new List<string>
                {
                    "Protect your privacy by limiting the personal information you share online.",
                    "Check your social media privacy settings regularly so you know who can see your posts and personal details.",
                    "Avoid posting sensitive information such as your ID number, home address, phone number, or banking details."
                }
            },
            {
                "safe browsing",
                new List<string>
                {
                    "Safe browsing means checking websites carefully before entering personal information.",
                    "Look for HTTPS and avoid downloading files from websites you do not trust.",
                    "Avoid clicking random pop-ups, shortened links, or adverts that look suspicious."
                }
            },
            {
                "malware",
                new List<string>
                {
                    "Malware is harmful software that can damage your device, steal information, or spy on your activity.",
                    "Do not download files from unknown websites, and avoid opening suspicious attachments.",
                    "Keep your antivirus software and operating system updated to reduce malware risks."
                }
            },
            {
                "2fa",
                new List<string>
                {
                    "Two-factor authentication adds an extra layer of security by requiring a second step after your password.",
                    "Use 2FA on important accounts such as email, banking, social media, and cloud storage.",
                    "Authenticator apps are often safer than SMS codes because SIM-swap scams can target phone numbers."
                }
            }
        };

        public string GetRandomResponse(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return GetDefaultResponse();
            }

            topic = topic.ToLower();

            if (topicResponses.ContainsKey(topic))
            {
                List<string> responses = topicResponses[topic];
                int index = random.Next(responses.Count);
                return responses[index];
            }

            return GetDefaultResponse();
        }

        public string GetDefaultResponse()
        {
            List<string> defaultResponses = new List<string>
            {
                "I’m not sure I understand. Can you try rephrasing your question?",
                "I did not quite catch that. You can ask me about passwords, phishing, scams, privacy, safe browsing, malware, or 2FA.",
                "That sounds interesting, but I may need you to rephrase it using a cybersecurity topic like password, scam, or privacy."
            };

            int index = random.Next(defaultResponses.Count);
            return defaultResponses[index];
        }

        public string GetHelpResponse()
        {
            return "You can ask me things like: 'Tell me about password safety', 'I am worried about scams', 'Give me a phishing tip', 'How do I protect my privacy?', or 'Tell me more'.";
        }

        public string GetWelcomeResponse()
        {
            return "Hello! Welcome to Cyber-Bot, your Cybersecurity Awareness Assistant. I can help you learn about passwords, phishing, scams, privacy, safe browsing, malware, and 2FA.";
        }
    }
}