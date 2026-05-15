namespace CybersecurityChatbotGUI.Services
{
    public class Validator
    {
        // Checks if the user entered nothing or only spaces.
        public bool IsEmpty(string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        // Validates the user's name using the same Part 1 strategy:
        // 1. Name cannot be empty.
        // 2. Name must be at least 3 characters.
        // 3. Name must contain at least one letter or digit.
        // 4. Name cannot be only special characters.
        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            name = name.Trim();

            if (name.Length < 3)
            {
                return false;
            }

            bool hasLetterOrDigit = false;

            foreach (char character in name)
            {
                if (char.IsLetterOrDigit(character))
                {
                    hasLetterOrDigit = true;
                    break;
                }
            }

            return hasLetterOrDigit;
        }

        // Used after the name step to check normal chatbot input.
        // This prevents empty, very short, or meaningless inputs from causing issues.
        public bool IsMeaningfulInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();

            if (input.Length < 3)
            {
                return false;
            }

            foreach (char character in input)
            {
                if (char.IsLetterOrDigit(character))
                {
                    return true;
                }
            }

            return false;
        }
    }
}