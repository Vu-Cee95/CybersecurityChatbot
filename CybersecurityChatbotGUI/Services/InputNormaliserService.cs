using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbotGUI.Services
{
    public class InputNormaliserService
    {
        private readonly Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "im", "i am" },
            { "i'm", "i am" },
            { "iam", "i am" },
            { "ive", "i have" },
            { "i've", "i have" },
            { "id", "i would" },
            { "i'd", "i would" },
            { "ill", "i will" },
            { "i'll", "i will" },
            { "dont", "do not" },
            { "don't", "do not" },
            { "cant", "cannot" },
            { "can't", "cannot" },
            { "wont", "will not" },
            { "won't", "will not" },
            { "didnt", "did not" },
            { "didn't", "did not" },
            { "doesnt", "does not" },
            { "doesn't", "does not" },
            { "isnt", "is not" },
            { "isn't", "is not" },
            { "wasnt", "was not" },
            { "wasn't", "was not" },
            { "werent", "were not" },
            { "weren't", "were not" },
            { "shouldnt", "should not" },
            { "shouldn't", "should not" },
            { "couldnt", "could not" },
            { "couldn't", "could not" },
            { "wouldnt", "would not" },
            { "wouldn't", "would not" },
            { "whats", "what is" },
            { "what's", "what is" },
            { "thats", "that is" },
            { "that's", "that is" },
            { "theres", "there is" },
            { "there's", "there is" },
            { "pls", "please" },
            { "plz", "please" },
            { "u", "you" },
            { "ur", "your" },
            { "yr", "your" },
            { "msg", "message" },
            { "msgs", "messages" },
            { "pwd", "password" },
            { "passcode", "password" },
            { "2 step", "2fa" },
            { "two factor", "2fa" },
            { "two-factor", "2fa" },
            { "otp", "one time password" },
            { "one-time password", "one time password" },
            { "hacked", "compromised hacked" },
            { "hackd", "hacked" },
            { "phising", "phishing" },
            { "phisihing", "phishing" },
            { "scammed", "scam" },
            { "malwear", "malware" }
        };

        public string Normalise(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            string normalisedInput = input.ToLower().Trim();

            normalisedInput = normalisedInput.Replace("’", "'");
            normalisedInput = normalisedInput.Replace("`", "'");
            normalisedInput = normalisedInput.Replace("“", "\"");
            normalisedInput = normalisedInput.Replace("”", "\"");

            normalisedInput = Regex.Replace(normalisedInput, @"[^\w\s'\-]", " ");
            normalisedInput = Regex.Replace(normalisedInput, @"\s+", " ");

            foreach (var replacement in replacements)
            {
                normalisedInput = Regex.Replace(
                    normalisedInput,
                    $@"\b{Regex.Escape(replacement.Key)}\b",
                    replacement.Value,
                    RegexOptions.IgnoreCase
                );
            }

            normalisedInput = Regex.Replace(normalisedInput, @"\s+", " ").Trim();

            return normalisedInput;
        }

        public bool ContainsPhrase(string input, params string[] phrases)
        {
            string normalisedInput = Normalise(input);

            foreach (string phrase in phrases)
            {
                string normalisedPhrase = Normalise(phrase);

                if (normalisedInput.Contains(normalisedPhrase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}