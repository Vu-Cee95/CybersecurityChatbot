using System;
using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class PersonalityService
    {
        private readonly Random random = new Random();

        private int lastOpeningIndex = -1;
        private int lastFollowUpIndex = -1;

        public string BuildPersonalisedResponse(
            string userName,
            string userInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment,
            string baseResponse,
            int totalMessages,
            string favouriteTopic,
            int followUpCount)
        {
            if (string.IsNullOrWhiteSpace(baseResponse))
            {
                return "I want to help, but I need a bit more detail from you first.";
            }

            string opening = GetOpeningReaction(
                userName,
                userInput,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                totalMessages,
                favouriteTopic,
                followUpCount
            );

            string followUp = GetSmartFollowUp(
                userName,
                detectedTopic,
                detectedIntent,
                detectedSentiment,
                favouriteTopic
            );

            if (string.IsNullOrWhiteSpace(opening) && string.IsNullOrWhiteSpace(followUp))
            {
                return baseResponse;
            }

            if (string.IsNullOrWhiteSpace(opening))
            {
                return $"{baseResponse}\n\n{followUp}";
            }

            if (string.IsNullOrWhiteSpace(followUp))
            {
                return $"{opening}\n\n{baseResponse}";
            }

            return $"{opening}\n\n{baseResponse}\n\n{followUp}";
        }

        private string GetOpeningReaction(
            string userName,
            string userInput,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment,
            int totalMessages,
            string favouriteTopic,
            int followUpCount)
        {
            List<string> openings = new List<string>();

            if (!string.IsNullOrWhiteSpace(detectedSentiment))
            {
                openings.AddRange(GetSentimentOpenings(userName, detectedSentiment));
            }

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                openings.AddRange(GetTopicOpenings(userName, detectedTopic));
            }

            if (!string.IsNullOrWhiteSpace(detectedIntent))
            {
                openings.AddRange(GetIntentOpenings(userName, detectedIntent, detectedTopic));
            }

            if (followUpCount > 0 && !string.IsNullOrWhiteSpace(detectedTopic))
            {
                openings.Add($"Good follow-up, {userName}. You are building the idea properly now.");
                openings.Add($"Nice, {userName}. Since we are still on {detectedTopic}, let me take it one level deeper.");
                openings.Add($"I like that follow-up. It shows you are not just reading — you are actually thinking about {detectedTopic}.");
            }

            if (!string.IsNullOrWhiteSpace(favouriteTopic) &&
                !string.IsNullOrWhiteSpace(detectedTopic) &&
                favouriteTopic.Equals(detectedTopic, StringComparison.OrdinalIgnoreCase))
            {
                openings.Add($"This connects nicely with your favourite topic, {userName}: {favouriteTopic}.");
                openings.Add($"I remember you are interested in {favouriteTopic}, so this is a good one to focus on.");
            }

            if (openings.Count == 0)
            {
                openings.AddRange(GetGeneralOpenings(userName, totalMessages));
            }

            return PickDifferent(openings, ref lastOpeningIndex);
        }

        private List<string> GetSentimentOpenings(string userName, string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return new List<string>
                    {
                        $"I hear you, {userName}. This sounds worrying, but we can handle it step by step.",
                        $"That is a valid concern, {userName}. Cyber threats can feel scary when they are not clear.",
                        $"No panic, {userName}. Let’s slow it down and look at what is really happening.",
                        $"I get why that would make you uneasy. Let me guide you carefully."
                    };

                case "curious":
                    return new List<string>
                    {
                        $"Great question, {userName}. That curiosity is exactly how people become safer online.",
                        $"I like where your mind is going with this, {userName}. Let’s unpack it properly.",
                        $"Good thinking. Asking this now can prevent bigger problems later.",
                        $"Nice one, {userName}. This is the kind of question a cyber-aware person asks."
                    };

                case "frustrated":
                    return new List<string>
                    {
                        $"No stress, {userName}. This can be confusing at first, but I’ll make it simple.",
                        $"I understand the frustration. Let’s break it down without the complicated tech language.",
                        $"You are not alone on this one. Cybersecurity can feel messy until it is explained clearly.",
                        $"Let’s simplify it, {userName}. I’ll keep this practical."
                    };

                default:
                    return new List<string>();
            }
        }

        private List<string> GetTopicOpenings(string userName, string topic)
        {
            switch (topic)
            {
                case "password":
                    return new List<string>
                    {
                        $"Password safety is a strong place to start, {userName}. One weak password can open many doors.",
                        $"Good topic. Passwords are boring until one gets leaked — then they become very important.",
                        $"Let’s talk passwords properly. This is one of the easiest areas to improve quickly."
                    };

                case "phishing":
                    return new List<string>
                    {
                        $"Phishing is a big one, {userName}. Attackers often rely on pressure and fake trust.",
                        $"Good choice. Phishing is dangerous because it often looks normal at first glance.",
                        $"Let’s inspect this like a cyber detective. Phishing is all about spotting the small red flags."
                    };

                case "scam":
                    return new List<string>
                    {
                        $"Scams are tricky because they attack emotions first, not technology.",
                        $"Good topic, {userName}. A scam usually tries to rush you before you think clearly.",
                        $"Let’s treat this carefully. Scammers are getting smarter, so we must be smarter too."
                    };

                case "privacy":
                    return new List<string>
                    {
                        $"Privacy matters, {userName}. The less unnecessary information you expose, the safer you become.",
                        $"This is an important one. Privacy is not about hiding; it is about control.",
                        $"Good focus. Online privacy protects your identity, your accounts, and your reputation."
                    };

                case "safe browsing":
                    return new List<string>
                    {
                        $"Safe browsing is one of those habits that protects you every day without you noticing.",
                        $"Good one, {userName}. Most online risks start with one careless click.",
                        $"Let’s make your browsing behaviour sharper and safer."
                    };

                case "malware":
                    return new List<string>
                    {
                        $"Malware is serious, {userName}. It can hide quietly while causing real damage.",
                        $"Good topic. Malware is not always obvious, which is why prevention matters.",
                        $"Let’s look at malware carefully. The goal is to stop it before it touches your device."
                    };

                case "2fa":
                    return new List<string>
                    {
                        $"2FA is one of the best security upgrades you can make, {userName}.",
                        $"Great topic. 2FA gives your account a second lock, not just one.",
                        $"Good thinking. Even if a password leaks, 2FA can still protect the account."
                    };

                default:
                    return new List<string>();
            }
        }

        private List<string> GetIntentOpenings(string userName, string intent, string topic)
        {
            switch (intent)
            {
                case "definition":
                    return new List<string>
                    {
                        $"Let me define it clearly first, {userName}.",
                        $"Let’s start with the meaning before going deeper.",
                        $"Good — a clear definition makes the rest easier to understand."
                    };

                case "prevention":
                    return new List<string>
                    {
                        $"Now we are talking about protection, which is the most important part.",
                        $"Good move, {userName}. Prevention is always better than fixing damage later.",
                        $"Let’s focus on what you can actually do to stay safe."
                    };

                case "example":
                    return new List<string>
                    {
                        $"An example will make this much easier to see.",
                        $"Good idea. Realistic examples help you spot this in real life.",
                        $"Let’s make it practical with a real-world style example."
                    };

                case "summary":
                    return new List<string>
                    {
                        $"Sure, {userName}. Let me pull your session together neatly.",
                        $"Good idea. A summary helps you see what we have covered.",
                        $"Let me give you a clean overview of this CyberBot session."
                    };

                case "emergency":
                    return new List<string>
                    {
                        $"This sounds serious, {userName}. I’m switching into careful guidance mode.",
                        $"Let’s handle this calmly and quickly. The next steps matter.",
                        $"Okay, {userName}. If something suspicious already happened, we need to reduce the damage first."
                    };

                default:
                    if (!string.IsNullOrWhiteSpace(topic))
                    {
                        return new List<string>
                        {
                            $"Let’s work through {topic} properly.",
                            $"Good, let’s focus on {topic}.",
                            $"I can help with that. {topic} is important for staying safe online."
                        };
                    }

                    return new List<string>();
            }
        }

        private List<string> GetGeneralOpenings(string userName, int totalMessages)
        {
            if (totalMessages <= 2)
            {
                return new List<string>
                {
                    $"Alright {userName}, let’s get started.",
                    $"Good start, {userName}. Let’s build from here.",
                    $"I’m with you, {userName}. Let’s keep this practical."
                };
            }

            return new List<string>
            {
                $"I see what you are asking, {userName}.",
                $"Good question. Let’s keep it clear and useful.",
                $"Let’s look at that properly.",
                $"That is worth unpacking.",
                $"I can help with that. Let’s make it practical."
            };
        }

        private string GetSmartFollowUp(
            string userName,
            string detectedTopic,
            string detectedIntent,
            string detectedSentiment,
            string favouriteTopic)
        {
            List<string> followUps = new List<string>();

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                followUps.AddRange(GetTopicFollowUps(userName, detectedTopic));
            }

            if (detectedIntent == "definition")
            {
                followUps.Add("Do you want me to give you a real-life example next?");
                followUps.Add("I can also show you how this usually appears in real life.");
            }

            if (detectedIntent == "prevention")
            {
                followUps.Add("Would you like me to turn this into a quick safety checklist?");
                followUps.Add("I can also give you a simple do-and-don’t list for this.");
            }

            if (detectedIntent == "example")
            {
                followUps.Add("Want another example, but this time from WhatsApp, email, or banking?");
                followUps.Add("I can also explain the red flags in that example one by one.");
            }

            if (detectedSentiment == "worried")
            {
                followUps.Add($"Do you want to tell me exactly what happened, {userName}? I can help you decide how serious it is.");
                followUps.Add("If this happened to you already, tell me what step happened first: clicked link, shared password, shared OTP, or downloaded a file.");
            }

            if (!string.IsNullOrWhiteSpace(favouriteTopic) &&
                string.IsNullOrWhiteSpace(detectedTopic))
            {
                followUps.Add($"Since you are interested in {favouriteTopic}, we can also continue from there.");
            }

            if (followUps.Count == 0)
            {
                followUps.Add("You can ask me for an example, a prevention tip, or a quick checklist.");
                followUps.Add("Want me to explain it in a simpler way or make it more practical?");
                followUps.Add("You can also ask: ‘What should I do next?’");
            }

            return PickDifferent(followUps, ref lastFollowUpIndex);
        }

        private List<string> GetTopicFollowUps(string userName, string topic)
        {
            switch (topic)
            {
                case "password":
                    return new List<string>
                    {
                        $"Do you want me to help you create a strong password pattern, {userName}?",
                        "Want a quick checklist for checking whether a password is strong?",
                        "I can also explain why reusing passwords is risky."
                    };

                case "phishing":
                    return new List<string>
                    {
                        "Do you want to paste a suspicious message and let me point out the red flags?",
                        "I can also give you a phishing checklist you can use before clicking links.",
                        $"Want me to show you how to inspect a suspicious link safely, {userName}?"
                    };

                case "scam":
                    return new List<string>
                    {
                        "Want me to show you the common scam warning signs?",
                        "You can paste a suspicious message, and I’ll help you check it.",
                        "I can also give you a quick scam-verification checklist."
                    };

                case "privacy":
                    return new List<string>
                    {
                        "Want me to give you a privacy checklist for your phone or social media?",
                        "I can also show you what personal information you should avoid posting online.",
                        $"Want practical privacy settings to check, {userName}?"
                    };

                case "safe browsing":
                    return new List<string>
                    {
                        "Want a quick safe browsing checklist?",
                        "I can also explain how to spot unsafe websites.",
                        "Do you want examples of website red flags?"
                    };

                case "malware":
                    return new List<string>
                    {
                        "Want me to explain signs that a device may have malware?",
                        "I can also give you steps to take after downloading a suspicious file.",
                        "Do you want a malware prevention checklist?"
                    };

                case "2fa":
                    return new List<string>
                    {
                        "Want me to compare SMS codes and authenticator apps?",
                        "I can also show you where 2FA matters most.",
                        $"Want a quick 2FA setup checklist, {userName}?"
                    };

                default:
                    return new List<string>();
            }
        }

        private string PickDifferent(List<string> items, ref int lastIndex)
        {
            if (items == null || items.Count == 0)
            {
                return "";
            }

            if (items.Count == 1)
            {
                lastIndex = 0;
                return items[0];
            }

            int index;

            do
            {
                index = random.Next(items.Count);
            }
            while (index == lastIndex);

            lastIndex = index;

            return items[index];
        }
    }
}