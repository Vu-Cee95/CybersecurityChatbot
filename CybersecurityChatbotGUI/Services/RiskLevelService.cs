namespace CybersecurityChatbotGUI.Services
{
    public class RiskLevelService
    {
        public string DetectRiskLevel(string normalisedInput)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "Low";
            }

            string input = normalisedInput.ToLower();

            if (ContainsAny(input,
                    "entered my banking password",
                    "gave my banking password",
                    "shared my banking password",
                    "gave my otp",
                    "shared my otp",
                    "sent my otp",
                    "gave my pin",
                    "shared my pin",
                    "money is gone",
                    "money was taken",
                    "account was emptied",
                    "my bank account was hacked",
                    "i paid them",
                    "i sent money",
                    "i transferred money"))
            {
                return "Emergency";
            }

            if (ContainsAny(input,
                    "i was hacked",
                    "i have been hacked",
                    "my account is hacked",
                    "account compromised",
                    "i clicked a link and entered",
                    "i downloaded a file",
                    "i installed something",
                    "i opened an attachment",
                    "i gave my password",
                    "shared my password",
                    "entered my password",
                    "gave my details",
                    "shared my details"))
            {
                return "High";
            }

            if (ContainsAny(input,
                    "i clicked a link",
                    "clicked a suspicious link",
                    "opened a link",
                    "suspicious message",
                    "strange message",
                    "unknown link",
                    "fake email",
                    "fake sms",
                    "phishing message",
                    "scam message"))
            {
                return "Medium";
            }

            if (ContainsAny(input,
                    "what is",
                    "explain",
                    "teach me",
                    "i want to learn",
                    "tips",
                    "example",
                    "checklist",
                    "how do i"))
            {
                return "Low";
            }

            return "Low";
        }

        public string DetectIssue(string normalisedInput)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "";
            }

            string input = normalisedInput.ToLower();

            if (ContainsAny(input, "otp", "one time password"))
            {
                return "OTP shared or exposed";
            }

            if (ContainsAny(input, "bank", "banking", "money", "payment", "paid", "transfer"))
            {
                return "Possible banking scam";
            }

            if (ContainsAny(input, "password", "login", "account"))
            {
                return "Account or password risk";
            }

            if (ContainsAny(input, "clicked", "link", "url", "website"))
            {
                return "Suspicious link interaction";
            }

            if (ContainsAny(input, "download", "attachment", "file", "installed"))
            {
                return "Possible malware risk";
            }

            if (ContainsAny(input, "message", "sms", "email", "whatsapp"))
            {
                return "Suspicious message";
            }

            return "";
        }

        public string BuildRiskResponse(string riskLevel, string issue)
        {
            string issueText = string.IsNullOrWhiteSpace(issue)
                ? "No specific issue detected yet"
                : issue;

            switch (riskLevel)
            {
                case "Emergency":
                    return $"Risk Level: EMERGENCY\nDetected issue: {issueText}\n\nThis may require immediate action. If banking details, money, OTPs, or passwords were shared, act quickly: change passwords, contact the bank or service provider, enable 2FA, and monitor the account.";

                case "High":
                    return $"Risk Level: HIGH\nDetected issue: {issueText}\n\nThis situation may expose your account, device, or personal information. You should secure the affected account, change passwords, enable 2FA, and avoid using the suspicious link, file, or message again.";

                case "Medium":
                    return $"Risk Level: MEDIUM\nDetected issue: {issueText}\n\nThere are warning signs here. Do not click further links, do not share personal details, and verify the message or website through an official source.";

                default:
                    return $"Risk Level: LOW\nDetected issue: {issueText}\n\nThis looks more like a learning or awareness question, but it is still good to build safer habits.";
            }
        }

        private bool ContainsAny(string input, params string[] phrases)
        {
            foreach (string phrase in phrases)
            {
                if (input.Contains(phrase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}