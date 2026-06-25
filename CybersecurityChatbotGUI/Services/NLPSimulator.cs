using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbotGUI.Services
{
    // Holds the result of NLP processing
    public class NLPResult
    {
        public string DetectedIntent { get; set; }
        public string DetectedKeyword { get; set; }
        public string OriginalInput { get; set; }
        public bool IsCommand { get; set; }
    }

    // Simulates Natural Language Processing for Part 3 command detection
    public class NLPSimulator
    {
        // Maps intents to keyword patterns for detection
        private readonly Dictionary<string, List<string>> intentPatterns;

        public NLPSimulator()
        {
            intentPatterns = new Dictionary<string, List<string>>
            {
                {
                    "add_task", new List<string>
                    {
                        "add task", "create task", "new task", "add a task",
                        "create a task", "add reminder", "set task"
                    }
                },
                {
                    "view_tasks", new List<string>
                    {
                        "view tasks", "show tasks", "list tasks", "my tasks",
                        "all tasks", "display tasks", "see tasks"
                    }
                },
                {
                    "complete_task", new List<string>
                    {
                        "complete task", "mark task", "done task", "finish task",
                        "task complete", "mark as done"
                    }
                },
                {
                    "delete_task", new List<string>
                    {
                        "delete task", "remove task", "clear task", "erase task"
                    }
                },
                {
                    "start_quiz", new List<string>
                    {
                        "start quiz", "play quiz", "begin quiz", "take quiz",
                        "quiz", "game", "play game", "start game", "cyber quiz",
                        "test knowledge"
                    }
                },
                {
                    "show_activity", new List<string>
                    {
                        "show activity", "activity log", "what have you done",
                        "what did you do", "show log", "view log", "recent actions",
                        "show history"
                    }
                },
                {
                    "set_reminder", new List<string>
                    {
                        "remind me", "set reminder", "remind", "reminder",
                        "notify me", "alert me"
                    }
                },
                {
                    "help", new List<string>
                    {
                        "help", "what can you do", "commands", "options",
                        "menu", "features"
                    }
                }
            };
        }

        // Processes user input and detects the intended command
        public NLPResult ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return new NLPResult
                {
                    DetectedIntent = "unknown",
                    OriginalInput = userInput
                };

            string lowerInput = userInput.ToLower().Trim();

            var result = new NLPResult
            {
                OriginalInput = userInput,
                DetectedIntent = "unknown",
                IsCommand = false
            };

            // Check each intent pattern against the user input
            foreach (var intent in intentPatterns)
            {
                foreach (var pattern in intent.Value)
                {
                    if (lowerInput.Contains(pattern))
                    {
                        result.DetectedIntent = intent.Key;
                        result.DetectedKeyword = pattern;
                        result.IsCommand = true;
                        break;
                    }
                }
                if (result.IsCommand) break;
            }

            // Fallback: use regex for broader matching if no exact match found
            if (!result.IsCommand)
            {
                if (Regex.IsMatch(lowerInput, @"\b(task|tasks|remind(er)?)\b"))
                {
                    result.DetectedIntent = "add_task";
                    result.DetectedKeyword = "task";
                    result.IsCommand = true;
                }
                else if (Regex.IsMatch(lowerInput, @"\b(quiz|game|question|test)\b"))
                {
                    result.DetectedIntent = "start_quiz";
                    result.DetectedKeyword = "quiz";
                    result.IsCommand = true;
                }
                else if (Regex.IsMatch(lowerInput, @"\b(log|history|activity|done for me)\b"))
                {
                    result.DetectedIntent = "show_activity";
                    result.DetectedKeyword = "activity";
                    result.IsCommand = true;
                }
            }

            return result;
        }

        // Returns a formatted help menu of available commands
        public string GetHelpMenu()
        {
            return "Here is what I can do in this session:\n\n" +
                   "TASK ASSISTANT:\n" +
                   "- 'Add task to [description]'\n" +
                   "- 'View tasks'\n" +
                   "- 'Complete task [number]'\n" +
                   "- 'Delete task [number]'\n" +
                   "- 'Remind me in [X] days'\n\n" +
                   "QUIZ:\n" +
                   "- 'Start quiz' or 'Play game'\n\n" +
                   "ACTIVITY LOG:\n" +
                   "- 'Show activity log'\n" +
                   "- 'What have you done for me?'\n\n" +
                   "CYBERSECURITY HELP:\n" +
                   "- Ask about passwords, phishing, scams, privacy, malware, safe browsing, or 2FA\n" +
                   "- 'Generate report' for a session summary";
        }
    }
}