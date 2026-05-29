// Required for Dictionary and List collections used for keyword mapping
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    // Service that detects cybersecurity topics, intents, and emergency types from user input
    // Uses keyword matching against predefined dictionaries to classify user messages
    public class KeywordService
    {
        // Dictionary mapping cybersecurity topics to their associated keywords
        // Each topic has a list of keywords/phrases that indicate that topic
        // Used for topic detection in user messages
        private readonly Dictionary<string, List<string>> keywordMap = new Dictionary<string, List<string>>
        {
            // Password-related keywords: password safety, credentials, PINs, account passwords
            {
                "password",
                new List<string>
                {
                    "password", "passcode", "login", "credentials", "strong password", "weak password",
                    "pin", "account password"
                }
            },
            // Phishing-related keywords: fake emails, suspicious links, clicked links
            {
                "phishing",
                new List<string>
                {
                    "phishing", "fake email", "email scam", "suspicious email", "fake message",
                    "fake link", "click link", "clicked a link", "suspicious link"
                }
            },
            // Scam-related keywords: fraud, banking scams, fake prizes, money requests
            {
                "scam",
                new List<string>
                {
                    "scam", "fraud", "fraudster", "online scam", "otp", "banking scam",
                    "fake prize", "lottery", "giveaway", "money request"
                }
            },
            // Privacy-related keywords: personal information, data, social media, ID numbers
            {
                "privacy",
                new List<string>
                {
                    "privacy", "private information", "personal information", "data", "settings",
                    "social media", "id number", "home address", "phone number"
                }
            },
            // Safe browsing keywords: browser security, websites, downloads, HTTPS, pop-ups
            {
                "safe browsing",
                new List<string>
                {
                    "safe browsing", "browser", "website", "web page", "download",
                    "https", "pop-up", "popup", "unsafe site"
                }
            },
            // Malware-related keywords: viruses, trojans, ransomware, infected files
            {
                "malware",
                new List<string>
                {
                    "malware", "virus", "trojan", "spyware", "ransomware", "infected",
                    "harmful software", "downloaded a file", "unknown file"
                }
            },
            // Two-factor authentication keywords: 2FA, MFA, authenticator apps, verification codes
            {
                "2fa",
                new List<string>
                {
                    "2fa", "two factor", "two-factor", "multi factor", "mfa",
                    "authenticator", "verification code", "one time pin", "one-time pin"
                }
            }
        };

        // Detects the cybersecurity topic in user input
        // Iterates through all topic keywords and returns the first matching topic
        // Returns empty string if no topic is detected
        public string DetectTopic(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check each topic's keywords against the input
            foreach (var topic in keywordMap)
            {
                foreach (string keyword in topic.Value)
                {
                    // Return first matching topic
                    if (lowerInput.Contains(keyword))
                    {
                        return topic.Key;
                    }
                }
            }

            // No topic detected
            return "";
        }

        // Detects the user's intent from their message
        // Classifies into: summary, emergency, definition, prevention, example, follow-up, or general
        // Intent determines the type of response the bot should provide
        public string DetectIntent(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check for summary/recap requests (highest priority)
            if (lowerInput.Contains("summary") ||
                lowerInput.Contains("summarise") ||
                lowerInput.Contains("summarize") ||
                lowerInput.Contains("recap"))
            {
                return "summary";
            }

            // Check for emergency situations (high priority)
            if (IsEmergencyInput(lowerInput))
            {
                return "emergency";
            }

            // Check for definition requests: "what is", "define", "meaning of"
            if (lowerInput.Contains("what is") ||
                lowerInput.Contains("define") ||
                lowerInput.Contains("meaning of") ||
                lowerInput.Contains("explain what"))
            {
                return "definition";
            }

            // Check for prevention/safety advice requests
            if (lowerInput.Contains("how do i") ||
                lowerInput.Contains("how can i") ||
                lowerInput.Contains("prevent") ||
                lowerInput.Contains("avoid") ||
                lowerInput.Contains("protect") ||
                lowerInput.Contains("stay safe"))
            {
                return "prevention";
            }

            // Check for example/scenario requests
            if (lowerInput.Contains("example") ||
                lowerInput.Contains("show me") ||
                lowerInput.Contains("scenario"))
            {
                return "example";
            }

            // Check for follow-up questions (tell me more, explain more, etc.)
            if (IsFollowUpQuestion(userInput))
            {
                return "follow-up";
            }

            // Default intent when no specific pattern is matched
            return "general";
        }

        // Detects if user input describes an emergency cybersecurity situation
        // Emergency indicators: clicked links, shared passwords/OTPs, hacked accounts, lost devices
        // Returns true if any emergency keyword is found
        public bool IsEmergencyInput(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check for common emergency indicators
            return lowerInput.Contains("clicked") ||           // Clicked suspicious link
                   lowerInput.Contains("gave my password") ||  // Shared password
                   lowerInput.Contains("shared my password") || // Shared password (alternative)
                   lowerInput.Contains("shared my otp") ||     // Shared one-time PIN
                   lowerInput.Contains("gave my otp") ||       // Shared one-time PIN (alternative)
                   lowerInput.Contains("hacked") ||            // Account hacked
                   lowerInput.Contains("compromised") ||       // Account compromised
                   lowerInput.Contains("downloaded") ||        // Downloaded suspicious file
                   lowerInput.Contains("infected") ||          // Device infected
                   lowerInput.Contains("stolen") ||            // Device/data stolen
                   lowerInput.Contains("lost my phone");       // Lost device
        }

        // Identifies the specific type of emergency from user input
        // Categorizes into: shared OTP, shared password, clicked link, downloaded file, hacked account
        // Returns the emergency type string or "general emergency" if type cannot be determined
        public string DetectEmergencyType(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "";
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check for shared OTP (one-time password/pin)
            if (lowerInput.Contains("otp") || lowerInput.Contains("one time pin") || lowerInput.Contains("one-time pin"))
            {
                return "shared otp";
            }

            // Check for shared password or login credentials
            if (lowerInput.Contains("password") || lowerInput.Contains("login") || lowerInput.Contains("credentials"))
            {
                return "shared password";
            }

            // Check for clicked suspicious link
            if (lowerInput.Contains("clicked") || lowerInput.Contains("link"))
            {
                return "clicked link";
            }

            // Check for downloaded malicious file
            if (lowerInput.Contains("downloaded") || lowerInput.Contains("virus") || lowerInput.Contains("malware") || lowerInput.Contains("infected"))
            {
                return "downloaded file";
            }

            // Check for hacked or compromised account
            if (lowerInput.Contains("hacked") || lowerInput.Contains("compromised"))
            {
                return "hacked account";
            }

            // Fallback when specific type cannot be determined
            return "general emergency";
        }

        // Detects if user is asking a follow-up question (requesting more information)
        // Matches phrases like "tell me more", "explain more", "another tip", "I don't understand"
        // Returns true if any follow-up phrase is found
        public bool IsFollowUpQuestion(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // List of common follow-up phrases
            List<string> followUps = new List<string>
            {
                "tell me more",        // Request for elaboration
                "explain more",        // Request for deeper explanation
                "more",                // Short request for more information
                "another tip",         // Request for additional tips
                "another one",         // Request for another example/tip
                "give me another",     // Request for another item
                "i do not understand", // Expression of confusion
                "i don't understand",  // Expression of confusion (contraction)
                "explain",             // Request for explanation
                "continue"             // Request to continue topic
            };

            // Check if any follow-up phrase is contained in the input
            foreach (string phrase in followUps)
            {
                if (lowerInput.Contains(phrase))
                {
                    return true;
                }
            }

            // No follow-up phrase detected
            return false;
        }

        // Detects if user is explicitly requesting help or asking about bot capabilities
        // Matches phrases like "help", "what can I ask", "what do you do", "purpose"
        // Returns true if a help request is detected
        public bool IsHelpRequest(string userInput)
        {
            // Validate input is not empty
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return false;
            }

            // Convert to lowercase for case-insensitive matching
            string lowerInput = userInput.ToLower();

            // Check for help-related keywords
            return lowerInput.Contains("help") ||           // Direct help request
                   lowerInput.Contains("what can i ask") || // Question about capabilities
                   lowerInput.Contains("what do you do") || // Question about bot purpose
                   lowerInput.Contains("purpose");          // Question about bot purpose
        }
    }
}