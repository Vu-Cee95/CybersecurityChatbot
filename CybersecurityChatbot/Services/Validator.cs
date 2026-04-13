namespace CybersecurityChatbot.Services
{
    // Provides validation methods for user input throughout the chatbot application
    internal class Validator
    {
        // Checks if the provided input string is null, empty, or only whitespace
        public bool IsEmpty(string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        // Validates that a name contains at least 3 characters with letters or digits
        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            if (name.Length < 3)
                return false;

            bool hasLetterOrDigit = false;

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    hasLetterOrDigit = true;
                    break;
                }
            }

            return hasLetterOrDigit;
        }

        // Validates that menu choice input is numeric and within the specified range
        public bool IsValidMenuChoice(string input, int min, int max)
        {
            if (!int.TryParse(input, out int choice))
                return false;

            return choice >= min && choice <= max;
        }

        // Ensures user input is meaningful with at least 3 characters and alphanumeric content
        public bool IsMeaningfulInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            if (input.Length < 3)
                return false;

            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c))
                    return true;
            }

            return false;
        }

        // Detects positive emotional responses based on keyword matching in user input
        public bool IsPositiveFeeling(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.ToLower();

            return input.Contains("fine") ||
                   input.Contains("good") ||
                   input.Contains("great") ||
                   input.Contains("okay") ||
                   input.Contains("ok") ||
                   input.Contains("well") ||
                   input.Contains("happy") ||
                   input.Contains("awesome") ||
                   input.Contains("alright");
        }

        // Detects negative emotional responses based on keyword matching in user input
        public bool IsNegativeFeeling(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.ToLower();

            return input.Contains("not fine") ||
                   input.Contains("bad") ||
                   input.Contains("sad") ||
                   input.Contains("worried") ||
                   input.Contains("stressed") ||
                   input.Contains("anxious") ||
                   input.Contains("upset") ||
                   input.Contains("confused") ||
                   input.Contains("not okay") ||
                   input.Contains("not ok") ||
                   input.Contains("tired") ||
                   input.Contains("angry");
        }
    }
}