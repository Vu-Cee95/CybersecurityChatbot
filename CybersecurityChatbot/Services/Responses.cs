namespace CybersecurityChatbot.Services
{
    // Provides all response text content for the chatbot's educational conversations
    internal class Responses
    {
        // Introduction to password safety topic with real-world context
        public string PasswordIntro()
        {
            return "password safety is one of the most important parts of cybersecurity. In real life, a weak password can make it easier for attackers to break into your email, social media, or even banking accounts.";
        }

        // Explanation of what constitutes a strong and secure password
        public string StrongPassword()
        {
            return "a strong password should be long, unique, and difficult to guess. It should include uppercase letters, lowercase letters, numbers, and symbols so it becomes harder for attackers to crack.";
        }

        // Practical tips for maintaining good password hygiene
        public string PasswordTips()
        {
            return "some useful password tips are to use a different password for each account, avoid personal details, and change your password when you suspect your account may have been exposed.";
        }

        // Common errors users make when creating and managing passwords
        public string PasswordMistakes()
        {
            return "common password mistakes include using short passwords, reusing the same password on multiple accounts, and choosing easy information like birthdays, names, or simple number patterns.";
        }

        // Benefits and purpose of using a password manager tool
        public string PasswordManager()
        {
            return "a password manager helps you store and generate strong passwords securely, which makes it easier to stay safe without relying on weak or repeated passwords.";
        }

        // Interactive quiz question to test password strength knowledge
        public string PasswordQuizQuestion()
        {
            return "quick check — which of these is the best password choice?\n1. 12345678\n2. P@ssw0rd!2026\n3. Vusi123";
        }

        // Positive feedback response for correct password quiz answers
        public string PasswordQuizCorrect()
        {
            return "that's right. A longer and more complex password is much safer than a simple or predictable one.";
        }

        // Corrective feedback explaining the right answer for password quiz
        public string PasswordQuizWrong()
        {
            return "the better answer was option 2, because it is stronger and harder to guess than the others.";
        }

        // Realistic scenario question about password reuse consequences
        public string PasswordScenarioQuestion()
        {
            return "here's a real-life scenario — you use the same password for your email and social media, and one site gets hacked. What is the safest response?\n1. Ignore it and hope for the best\n2. Only change the social media password\n3. Change both passwords immediately and make them different";
        }

        // Confirmation that changing all affected passwords is the safest action
        public string PasswordScenarioCorrect()
        {
            return "that's the safest move. If one reused password is exposed, several accounts can be put at risk, so changing all affected passwords quickly is important.";
        }

        // Explanation of why partial password changes are insufficient
        public string PasswordScenarioWrong()
        {
            return "the best answer was option 3. Reused passwords can create a chain reaction if one account is breached.";
        }

        // Interesting fact about the dangers of password reuse
        public string PasswordDidYouKnow()
        {
            return "did you know? Reusing one password across multiple accounts means a single data breach can expose much more of your digital life than you expect.";
        }

        // Opening prompt for the password help and troubleshooting section
        public string PasswordHelperIntro()
        {
            return "let's sort that out. Tell me which password problem sounds closest to your situation, and I'll guide you.";
        }

        // Guidance for users who have forgotten their account passwords
        public string PasswordHelperForgot()
        {
            return "the safest step is to use the official password reset option on the real website or app. Avoid resetting through links in random emails, and once you regain access, create a stronger password.";
        }

        // Advice for addressing the security risk of reused passwords
        public string PasswordHelperReused()
        {
            return "you should change those passwords as soon as possible, starting with your email and banking accounts. Make sure each important account gets its own unique password.";
        }

        // Steps to take when a user suspects their password has been compromised
        public string PasswordHelperCompromised()
        {
            return "change the password immediately, sign out of other devices if possible, and enable two-factor authentication. If the account is important, also review recent activity for anything suspicious.";
        }

        // Introduction to phishing awareness with real-world scam context
        public string PhishingIntro()
        {
            return "phishing is a cyber scam where criminals try to trick people into sharing personal information such as passwords, banking details, or login credentials. In real life, it often appears as fake bank alerts, fake delivery messages, or urgent workplace notices.";
        }

        // Key indicators that help identify potential phishing attempts
        public string PhishingSigns()
        {
            return "some signs of phishing include urgent messages, suspicious links, poor spelling, unknown senders, and requests for confidential information.";
        }

        // Practical strategies for avoiding phishing scams
        public string PhishingTips()
        {
            return "to stay safe from phishing, always check the sender, avoid clicking unknown links, and confirm suspicious messages directly with the company or person involved.";
        }

        // Recommended immediate actions when phishing is suspected
        public string PhishingAction()
        {
            return "if you suspect phishing, do not click the link, do not download attachments, and report or delete the message immediately.";
        }

        // Interactive quiz question testing phishing awareness and response
        public string PhishingQuizQuestion()
        {
            return "quick check — what is the safest thing to do if you get an unexpected email asking for your password?\n1. Reply quickly\n2. Verify the sender and avoid clicking links\n3. Download the attachment first";
        }

        // Positive reinforcement for correct phishing quiz answers
        public string PhishingQuizCorrect()
        {
            return "correct. Verifying the sender and avoiding suspicious links is the safest move.";
        }

        // Educational correction explaining proper phishing response protocol
        public string PhishingQuizWrong()
        {
            return "the correct answer was option 2. You should verify the sender and avoid clicking suspicious links or attachments.";
        }

        // Realistic phishing scenario involving urgent bank communication
        public string PhishingScenarioQuestion()
        {
            return "here's a real-life scenario — you receive an email saying your bank account will be locked in 10 minutes unless you click a link. What should you do?\n1. Click the link quickly\n2. Ignore the link and contact the bank using official details\n3. Reply with your banking details";
        }

        // Validation that verifying through official channels is the correct approach
        public string PhishingScenarioCorrect()
        {
            return "that's the safest response. Attackers often use urgency to make people panic and act without thinking.";
        }

        // Explanation of why urgency tactics should trigger suspicion
        public string PhishingScenarioWrong()
        {
            return "the correct answer was option 2. Urgent scare tactics are common in phishing scams, so verification through official channels is much safer.";
        }

        // Interesting fact about the sophistication of phishing messages
        public string PhishingDidYouKnow()
        {
            return "did you know? Phishing messages often look more convincing when they pretend to come from brands, schools, banks, or even your own workplace.";
        }

        // Opening prompt for the phishing help and guidance section
        public string PhishingHelperIntro()
        {
            return "let's handle it carefully. Choose the situation that sounds most like what you're dealing with.";
        }

        // Guidance for handling urgent phishing links and pressure tactics
        public string PhishingHelperUrgentLink()
        {
            return "do not click the link. Urgent pressure is a classic phishing tactic. Instead, verify the message through the official website or a trusted contact number.";
        }

        // Warning against sharing banking information through insecure channels
        public string PhishingHelperBankDetails()
        {
            return "never send banking details through email or messages like that. Real banks usually do not ask for sensitive information in that way.";
        }

        // Advice for verifying suspicious workplace-related messages
        public string PhishingHelperWorkMessage()
        {
            return "treat it carefully and confirm with your workplace through an official method, such as calling the sender directly or checking with IT or your supervisor.";
        }

        // Introduction to safe browsing practices and online navigation awareness
        public string BrowsingIntro()
        {
            return "safe browsing means using the internet carefully so you can avoid dangerous websites, harmful downloads, and online scams. In real life, this includes being careful on shopping sites, streaming pages, pop-up ads, and download pages.";
        }

        // Essential tips for maintaining safety while browsing the web
        public string BrowsingTips()
        {
            return "safe browsing tips include visiting trusted websites, checking for HTTPS, keeping your browser updated, and avoiding suspicious pop-ups or unknown links.";
        }

        // Overview of potential consequences from unsafe browsing habits
        public string UnsafeBrowsing()
        {
            return "unsafe browsing can expose you to malware, fake websites, stolen information, and privacy risks. In some cases, one careless click can cause serious problems.";
        }

        // Best practices for safely downloading files from the internet
        public string SafeDownloads()
        {
            return "you should only download files from trusted sources, scan downloads when possible, and avoid cracked software or unknown attachments because they often carry hidden risks.";
        }

        // Interactive quiz question testing secure connection awareness
        public string BrowsingQuizQuestion()
        {
            return "quick check — which website sign usually shows a safer connection?\n1. HTTPS\n2. Random pop-ups\n3. Unknown download prompts";
        }

        // Confirmation that HTTPS indicates a secure browser connection
        public string BrowsingQuizCorrect()
        {
            return "correct. HTTPS is a good sign that the website is using a secure connection.";
        }

        // Educational correction about secure connection indicators
        public string BrowsingQuizWrong()
        {
            return "the correct answer was option 1. HTTPS usually shows that the connection is secured.";
        }

        // Realistic scenario about downloading software from suspicious sources
        public string BrowsingScenarioQuestion()
        {
            return "here's a real-life scenario — you want to download free software from a site full of flashing ads and random pop-ups. What should you do?\n1. Download it quickly before the offer disappears\n2. Leave the site and look for the software on an official source\n3. Click the biggest download button you see";
        }

        // Validation that avoiding suspicious download sites is the safest choice
        public string BrowsingScenarioCorrect()
        {
            return "good decision. Risky-looking download pages often hide malware or fake download buttons.";
        }

        // Explanation of the hidden dangers on untrustworthy download pages
        public string BrowsingScenarioWrong()
        {
            return "the safest answer was option 2. Suspicious download sites often trick users into installing harmful files.";
        }

        // Interesting fact about the deceptive nature of fake websites
        public string BrowsingDidYouKnow()
        {
            return "did you know? Fake websites sometimes copy the design of real brands so closely that only the web address reveals the difference.";
        }

        // Opening prompt for safe browsing help and troubleshooting section
        public string BrowsingHelperIntro()
        {
            return "that's a smart concern. Tell me what the website is doing, and I'll guide you on the safest response.";
        }

        // Guidance for handling websites with suspicious pop-up behavior
        public string BrowsingHelperPopups()
        {
            return "close the page if possible and avoid clicking the pop-ups. Strange pop-ups often try to trick users into allowing notifications, downloading files, or visiting unsafe pages.";
        }

        // Advice for responding to potentially spoofed or misspelled website addresses
        public string BrowsingHelperMisspelledUrl()
        {
            return "leave the site and double-check the address. Misspelled URLs are a common sign of fake websites designed to copy real ones.";
        }

        // Safety protocol for unexpected download prompts while browsing
        public string BrowsingHelperUnexpectedDownload()
        {
            return "do not download the file unless you are absolutely sure it came from a trusted source. Unexpected downloads are often linked to malware or scams.";
        }

        // Introduction to the general risk guidance and help section
        public string WhatShouldIDoIntro()
        {
            return "I can help with that. Choose the situation that sounds closest to what you're facing, and I'll suggest the safest next step.";
        }

        // Step-by-step guidance for handling suspicious email messages
        public string HelperSuspiciousEmail()
        {
            return "do not click any links or open attachments. Check the sender carefully, and if the message claims to be from a company, contact that company through its official website or phone number.";
        }

        // Recommended actions for users who recognize their password is weak
        public string HelperWeakPassword()
        {
            return "change it as soon as possible and replace it with something longer and more unique. If you have reused it anywhere else, change those accounts too.";
        }

        // Safety measures to take when encountering a suspicious website
        public string HelperUnsafeWebsite()
        {
            return "leave the site if it looks suspicious, avoid entering any personal details, and do not trust warnings or offers that seem too urgent or too good to be true.";
        }

        // Guidance for handling potentially unsafe downloaded files
        public string HelperDownloadedFile()
        {
            return "avoid opening it until you can scan it properly. If you already opened it and something feels wrong, disconnect from the internet if needed and run a security scan.";
        }

        // Warning about the critical importance of never sharing OTP codes
        public string HelperOtpCode()
        {
            return "never share your OTP or verification code with anyone. Those codes are meant to protect your account, and scammers often ask for them to bypass security.";
        }

        // Introduction to the cybersecurity facts and trivia section
        public string DidYouKnowIntro()
        {
            return "I've got a few useful facts for you. These are quick, practical bits of information that can really improve your awareness.";
        }

        // Returns a randomly selected cybersecurity fact from a curated collection
        public string RandomDidYouKnow()
        {
            string[] facts = new string[]
            {
                "did you know? One weak password can sometimes be enough to expose email, banking, and social media accounts if it has been reused.",
                "did you know? Attackers often rely on panic and urgency more than technical skill when they run phishing scams.",
                "did you know? A fake website can look almost identical to the real one, but the web address often gives it away.",
                "did you know? Unexpected downloads, cracked software, and fake update prompts are common ways malware spreads.",
                "did you know? Two-factor authentication can help protect an account even if a password is stolen."
            };

            Random random = new Random();
            int index = random.Next(facts.Length);
            return facts[index];
        }

        // Explains the chatbot's educational mission and purpose
        public string Purpose()
        {
            return "my purpose is to help you stay safe online by teaching you about cybersecurity awareness, digital safety, and common online threats in a simple and helpful way.";
        }

        // Lists the available topics and types of questions the chatbot can address
        public string Help()
        {
            return "you can ask me about password safety, phishing, safe browsing, suspicious links, risky situations, and general cybersecurity awareness. I'm here to make these topics easier to understand.";
        }

        // Personalized response when users ask how the chatbot is doing
        public string HowAreYou(string name)
        {
            return $"I'm doing well, {name}. I'm fully alert, ready to assist, and always happy to help with staying safe online. Thanks for asking — that was thoughtful.";
        }

        // Prompt asking the user about their current emotional state
        public string AskUserHowTheyAre()
        {
            return "how are you doing today?";
        }

        // Encouraging response when users report feeling positive or good
        public string UserFeelingPositive(string name)
        {
            return $"I'm glad to hear that, {name}. It's always nice when someone is doing well. Since you're here, this is a great time to strengthen your cybersecurity awareness before problems ever start.";
        }

        // Follow-up prompt offering educational content after positive user response
        public string UserFeelingPositiveFollowUp()
        {
            return "while things are going well, would you like a quick cybersecurity tip, a phishing warning sign, or a password safety reminder?";
        }

        // Empathetic response when users report feeling negative or struggling
        public string UserFeelingNegative(string name)
        {
            return $"I'm sorry to hear that, {name}. That sounds difficult. Let's take things one step at a time, and I'll keep this simple and helpful for you.";
        }

        // Follow-up prompt offering practical help after negative user response
        public string UserFeelingNegativeFollowUp()
        {
            return "would you like me to help with something practical right now, like suspicious emails, password safety, or unsafe websites?";
        }

        // Neutral and supportive response for ambiguous or unclear user feelings
        public string UserFeelingUnknown(string name)
        {
            return $"Thank you for sharing that, {name}. I appreciate the reply. Whether you're feeling good, unsure, or somewhere in between, I'm here to help.";
        }

        // Follow-up prompt offering options after ambiguous user response
        public string UserFeelingUnknownFollowUp()
        {
            return "would you like to continue with a cybersecurity tip, ask a question, or go to the main menu?";
        }

        // Default response for unrecognized or invalid user input
        public string Default()
        {
            return "I didnt quite undersatnd that, can you plese repeat it.";
        }

        // Farewell message displayed when the user chooses to exit the chatbot
        public string Exit(string name)
        {
            return $"alright, {name}, it was good chatting with you. Stay safe, stay alert, and take care online.";
        }

        // Notification displayed when user exceeds the maximum allowed attempts
        public string AttemptsExceeded()
        {
            return "you have used all your attempts for this step. Goodbye for now.";
        }
    }
}