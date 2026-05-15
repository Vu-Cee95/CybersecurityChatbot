namespace CybersecurityChatbotGUI.Services
{
    public class ClarifyingQuestionService
    {
        public bool NeedsClarification(string normalisedInput, string detectedTopic, string detectedIntent, string riskLevel)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return false;
            }

            string input = normalisedInput.ToLower();

            if (!string.IsNullOrWhiteSpace(detectedTopic))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(detectedIntent) &&
                detectedIntent != "general" &&
                detectedIntent != "follow-up")
            {
                return false;
            }

            if (riskLevel == "High" || riskLevel == "Emergency")
            {
                return false;
            }

            if (input.Contains("something happened") ||
                input.Contains("i need help") ||
                input.Contains("help me") ||
                input.Contains("i am scared") ||
                input.Contains("i am worried") ||
                input.Contains("i do not know what to do") ||
                input.Contains("not sure") ||
                input.Contains("i think it is bad") ||
                input.Contains("this looks suspicious") ||
                input.Contains("what should i do"))
            {
                return true;
            }

            return false;
        }

        public string BuildClarifyingQuestion(string userName, string riskLevel)
        {
            return $"I can help, {userName}, but I need to understand what happened first.\n\n" +
                   "Choose the closest option or type it in your own words:\n\n" +
                   "• I clicked a suspicious link\n" +
                   "• I shared my password\n" +
                   "• I shared an OTP\n" +
                   "• I downloaded a suspicious file\n" +
                   "• I received a suspicious message\n" +
                   "• I want to learn about a cybersecurity topic";
        }
    }
}