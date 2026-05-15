namespace CybersecurityChatbotGUI.Services
{
    public class PlatformExampleService
    {
        public string DetectPlatform(string normalisedInput)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "";
            }

            string input = normalisedInput.ToLower();

            if (input.Contains("whatsapp"))
            {
                return "WhatsApp";
            }

            if (input.Contains("email") || input.Contains("mail"))
            {
                return "Email";
            }

            if (input.Contains("sms") || input.Contains("text message"))
            {
                return "SMS";
            }

            if (input.Contains("bank") || input.Contains("banking"))
            {
                return "Banking";
            }

            if (input.Contains("facebook"))
            {
                return "Facebook";
            }

            if (input.Contains("instagram"))
            {
                return "Instagram";
            }

            if (input.Contains("website") || input.Contains("browser") || input.Contains("link"))
            {
                return "Website";
            }

            return "";
        }

        public string GetPlatformExample(string topic, string platform)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = "cybersecurity";
            }

            if (string.IsNullOrWhiteSpace(platform))
            {
                return "";
            }

            topic = topic.ToLower();

            switch (platform)
            {
                case "WhatsApp":
                    return GetWhatsAppExample(topic);

                case "Email":
                    return GetEmailExample(topic);

                case "SMS":
                    return GetSmsExample(topic);

                case "Banking":
                    return GetBankingExample(topic);

                case "Facebook":
                    return GetFacebookExample(topic);

                case "Instagram":
                    return GetInstagramExample(topic);

                case "Website":
                    return GetWebsiteExample(topic);

                default:
                    return "";
            }
        }

        private string GetWhatsAppExample(string topic)
        {
            return "WhatsApp example:\n\n" +
                   "You receive a WhatsApp message saying: “Your account will be blocked today. Click this link now to verify your details.”\n\n" +
                   "Red flags:\n" +
                   "• It creates urgency.\n" +
                   "• It asks you to click a link.\n" +
                   "• It may pretend to be from a trusted organisation.\n" +
                   "• It may request personal information, OTPs, or passwords.\n\n" +
                   "Safer action: Do not click the link. Open the official app or website yourself.";
        }

        private string GetEmailExample(string topic)
        {
            return "Email example:\n\n" +
                   "You receive an email saying: “Your mailbox is full. Log in now using this link to avoid losing access.”\n\n" +
                   "Red flags:\n" +
                   "• The sender address may look slightly wrong.\n" +
                   "• The email pressures you to act quickly.\n" +
                   "• The link may lead to a fake login page.\n\n" +
                   "Safer action: Do not use the email link. Go directly to the official website.";
        }

        private string GetSmsExample(string topic)
        {
            return "SMS example:\n\n" +
                   "You receive an SMS saying: “You have won a prize. Reply with your ID number and banking details to claim.”\n\n" +
                   "Red flags:\n" +
                   "• Unexpected prize message.\n" +
                   "• Requests personal or banking details.\n" +
                   "• Tries to make you respond quickly.\n\n" +
                   "Safer action: Ignore, block, and report the number.";
        }

        private string GetBankingExample(string topic)
        {
            return "Banking example:\n\n" +
                   "You receive a message saying: “Your bank account has been locked. Click here and enter your card number, PIN, and OTP.”\n\n" +
                   "Red flags:\n" +
                   "• Banks will not ask for your PIN or OTP through a link.\n" +
                   "• The message creates fear.\n" +
                   "• It asks for sensitive banking information.\n\n" +
                   "Safer action: Open your banking app directly or contact your bank using official contact details.";
        }

        private string GetFacebookExample(string topic)
        {
            return "Facebook example:\n\n" +
                   "You receive a message saying: “Is this you in this video?” followed by a suspicious link.\n\n" +
                   "Red flags:\n" +
                   "• Curiosity-based trick.\n" +
                   "• Link may steal login details.\n" +
                   "• Message may come from a hacked friend’s account.\n\n" +
                   "Safer action: Do not click. Contact the friend through another channel.";
        }

        private string GetInstagramExample(string topic)
        {
            return "Instagram example:\n\n" +
                   "You receive a DM saying: “Your account violated rules. Verify here or your account will be deleted.”\n\n" +
                   "Red flags:\n" +
                   "• Threatens account deletion.\n" +
                   "• Sends a login link through DM.\n" +
                   "• May steal your Instagram password.\n\n" +
                   "Safer action: Check account notices inside the official Instagram app.";
        }

        private string GetWebsiteExample(string topic)
        {
            return "Website example:\n\n" +
                   "A website pop-up says: “Your device is infected. Download this cleaner now.”\n\n" +
                   "Red flags:\n" +
                   "• Scare message.\n" +
                   "• Pushes a download.\n" +
                   "• May install malware.\n\n" +
                   "Safer action: Close the page and scan your device using trusted security software.";
        }
    }
}