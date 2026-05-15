using System.Collections.Generic;

namespace CybersecurityChatbotGUI.Services
{
    public class ContextChoiceService
    {
        public string DetectChoice(string normalisedInput, string pendingOptions)
        {
            if (string.IsNullOrWhiteSpace(normalisedInput))
            {
                return "";
            }

            normalisedInput = normalisedInput.ToLower();
            pendingOptions = string.IsNullOrWhiteSpace(pendingOptions) ? "" : pendingOptions.ToLower();

            if (IsAllChoice(normalisedInput))
            {
                return "all";
            }

            if (IsTipChoice(normalisedInput) && pendingOptions.Contains("tip"))
            {
                return "tip";
            }

            if (IsExampleChoice(normalisedInput) && pendingOptions.Contains("example"))
            {
                return "example";
            }

            if (IsChecklistChoice(normalisedInput) && pendingOptions.Contains("checklist"))
            {
                return "checklist";
            }

            if (IsDefinitionChoice(normalisedInput) && pendingOptions.Contains("definition"))
            {
                return "definition";
            }

            if (IsYesChoice(normalisedInput))
            {
                return "yes";
            }

            if (IsNoChoice(normalisedInput))
            {
                return "no";
            }

            return "";
        }

        public bool LooksLikeVagueFollowUp(string normalisedInput)
        {
            List<string> vagueReplies = new List<string>
            {
                "yes",
                "yeah",
                "yep",
                "sure",
                "okay",
                "ok",
                "please",
                "show me",
                "give me",
                "tell me",
                "that one",
                "the first one",
                "the second one",
                "the third one",
                "continue",
                "go on",
                "more",
                "explain more",
                "i want that",
                "do that",
                "let us do that",
                "lets do that"
            };

            foreach (string reply in vagueReplies)
            {
                if (normalisedInput == reply || normalisedInput.Contains(reply))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTipChoice(string input)
        {
            return input.Contains("tip") ||
                   input.Contains("tips") ||
                   input.Contains("advice") ||
                   input.Contains("recommendation") ||
                   input.Contains("how to stay safe") ||
                   input.Contains("protect myself") ||
                   input.Contains("prevent");
        }

        private bool IsExampleChoice(string input)
        {
            return input.Contains("example") ||
                   input.Contains("examples") ||
                   input.Contains("scenario") ||
                   input.Contains("real life") ||
                   input.Contains("show me how") ||
                   input.Contains("sample");
        }

        private bool IsChecklistChoice(string input)
        {
            return input.Contains("checklist") ||
                   input.Contains("list") ||
                   input.Contains("steps") ||
                   input.Contains("step by step") ||
                   input.Contains("what should i do") ||
                   input.Contains("what to do");
        }

        private bool IsDefinitionChoice(string input)
        {
            return input.Contains("meaning") ||
                   input.Contains("define") ||
                   input.Contains("definition") ||
                   input.Contains("what is") ||
                   input.Contains("explain what");
        }

        private bool IsAllChoice(string input)
        {
            return input.Contains("all") ||
                   input.Contains("everything") ||
                   input.Contains("both") ||
                   input.Contains("all of them") ||
                   input.Contains("all options") ||
                   input.Contains("tip and example") ||
                   input.Contains("example and tip") ||
                   input.Contains("give me all");
        }

        private bool IsYesChoice(string input)
        {
            return input == "yes" ||
                   input == "yeah" ||
                   input == "yep" ||
                   input == "sure" ||
                   input == "ok" ||
                   input == "okay" ||
                   input.Contains("yes please") ||
                   input.Contains("sure please") ||
                   input.Contains("go ahead") ||
                   input.Contains("show me") ||
                   input.Contains("give me") ||
                   input.Contains("tell me more");
        }

        private bool IsNoChoice(string input)
        {
            return input == "no" ||
                   input == "nope" ||
                   input.Contains("not now") ||
                   input.Contains("no thanks") ||
                   input.Contains("stop") ||
                   input.Contains("cancel");
        }
    }
}