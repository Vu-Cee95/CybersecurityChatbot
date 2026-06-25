using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbotGUI.Services
{
    // Represents a single quiz question with options and answer
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
        public string Topic { get; set; }
        public bool IsTrueFalse { get; set; }
    }

    // Manages quiz logic, questions, scoring, and feedback
    public class QuizManager
    {
        private List<QuizQuestion> questionBank;
        private List<QuizQuestion> currentQuizQuestions;
        private int currentQuestionIndex;
        private int correctAnswers;
        private readonly Random random;

        public int TotalQuestions => currentQuizQuestions?.Count ?? 0;
        public int CurrentQuestionNumber => currentQuestionIndex + 1;
        public bool IsQuizActive { get; private set; }

        public QuizManager()
        {
            random = new Random();
            InitializeQuestionBank();
        }

        // Populates the question bank with 14 cybersecurity questions
        private void InitializeQuestionBank()
        {
            questionBank = new List<QuizQuestion>
            {
                // Phishing questions (2)
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string>
                    {
                        "Reply with your password",
                        "Click the link to verify",
                        "Report it as phishing and delete it",
                        "Forward it to friends"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Legitimate companies will never ask for your password via email. Always report phishing attempts.",
                    Topic = "Phishing",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "True or False: It is safe to click links in emails from unknown senders if the email looks professional.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Professional appearance does not guarantee safety. Always verify the sender before clicking links.",
                    Topic = "Phishing",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "Which is a common sign of a phishing email?",
                    Options = new List<string>
                    {
                        "Urgent language demanding immediate action",
                        "A personal greeting using your full name",
                        "Sent from a verified company domain",
                        "Includes your correct account number"
                    },
                    CorrectAnswerIndex = 0,
                    Explanation = "Phishing emails often use urgency and fear to pressure victims into acting without thinking.",
                    Topic = "Phishing",
                    IsTrueFalse = false
                },

                // Password safety questions (2)
                new QuizQuestion
                {
                    Question = "Which password is the strongest?",
                    Options = new List<string>
                    {
                        "password123",
                        "MyDogBuddy!",
                        "C0rr3ct-H0rs3-B@ttery-St@ple",
                        "qwerty"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Long passphrases with mixed characters, numbers, and symbols are significantly harder to crack.",
                    Topic = "Password Safety",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "True or False: You should use the same strong password for all your accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Using unique passwords prevents a single breach from compromising all your accounts.",
                    Topic = "Password Safety",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "How often should you change your passwords?",
                    Options = new List<string>
                    {
                        "Every day",
                        "Never",
                        "When there is a security breach or suspicious activity",
                        "Every week"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Change passwords immediately after a breach. Unnecessary frequent changes can lead to weaker passwords.",
                    Topic = "Password Safety",
                    IsTrueFalse = false
                },

                // Safe browsing questions (2)
                new QuizQuestion
                {
                    Question = "What does HTTPS in a website URL indicate?",
                    Options = new List<string>
                    {
                        "The website is completely safe",
                        "The connection is encrypted",
                        "The website sells products",
                        "The website is popular"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "HTTPS encrypts data between your browser and the website, but the site itself could still be malicious.",
                    Topic = "Safe Browsing",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "True or False: Public Wi-Fi is safe for online banking if the website uses HTTPS.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Public Wi-Fi networks can be intercepted. Avoid sensitive transactions on public networks.",
                    Topic = "Safe Browsing",
                    IsTrueFalse = true
                },

                // Social engineering questions (2)
                new QuizQuestion
                {
                    Question = "What is social engineering in cybersecurity?",
                    Options = new List<string>
                    {
                        "Using social media for marketing",
                        "Manipulating people to reveal confidential information",
                        "Building secure networks",
                        "Programming social media applications"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities to gain access.",
                    Topic = "Social Engineering",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "A caller claims to be from IT support and requests your password. What should you do?",
                    Options = new List<string>
                    {
                        "Provide the password",
                        "Ask for their employee ID first",
                        "Hang up and contact IT through official channels",
                        "Give a fake password"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Always verify through official channels. Legitimate IT support will never ask for your password.",
                    Topic = "Social Engineering",
                    IsTrueFalse = false
                },

                // General cybersecurity questions (2)
                new QuizQuestion
                {
                    Question = "What is Two-Factor Authentication (2FA)?",
                    Options = new List<string>
                    {
                        "Using two passwords",
                        "A second verification step beyond your password",
                        "Having two accounts",
                        "Logging in twice"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "2FA combines something you know (password) with something you have (phone) for enhanced security.",
                    Topic = "General Cybersecurity",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "True or False: Antivirus software alone is sufficient to protect against all cyber threats.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Antivirus is one layer of defense. Safe habits, strong passwords, and updates are also essential.",
                    Topic = "General Cybersecurity",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "What should you do first if you suspect malware on your device?",
                    Options = new List<string>
                    {
                        "Ignore it and continue working",
                        "Run a full antivirus scan immediately",
                        "Restart the device only",
                        "Delete random files"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Run a full scan immediately and disconnect from the internet to prevent data exfiltration.",
                    Topic = "Malware",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string>
                    {
                        "Software that improves computer performance",
                        "Malware that encrypts files and demands payment",
                        "A type of antivirus program",
                        "A password management tool"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware locks your files until a ransom is paid. Regular backups are your best defense.",
                    Topic = "Malware",
                    IsTrueFalse = false
                }
            };
        }

        // Starts a new quiz session with 10 randomly selected questions
        public void StartNewQuiz()
        {
            currentQuizQuestions = questionBank.OrderBy(q => random.Next()).Take(10).ToList();
            currentQuestionIndex = 0;
            correctAnswers = 0;
            IsQuizActive = true;
        }

        // Returns the current question or null if quiz is complete
        public QuizQuestion GetCurrentQuestion()
        {
            if (currentQuizQuestions == null || currentQuestionIndex >= currentQuizQuestions.Count)
                return null;

            return currentQuizQuestions[currentQuestionIndex];
        }

        // Processes user's answer and returns true if correct
        public bool SubmitAnswer(int selectedAnswerIndex)
        {
            if (!IsQuizActive || currentQuizQuestions == null)
                return false;

            bool isCorrect = selectedAnswerIndex == currentQuizQuestions[currentQuestionIndex].CorrectAnswerIndex;

            if (isCorrect)
                correctAnswers++;

            return isCorrect;
        }

        // Returns the explanation for the current question
        public string GetCurrentExplanation()
        {
            if (currentQuizQuestions == null || currentQuestionIndex >= currentQuizQuestions.Count)
                return string.Empty;

            return currentQuizQuestions[currentQuestionIndex].Explanation;
        }

        // Returns the correct answer text for the current question
        public string GetCorrectAnswerText()
        {
            if (currentQuizQuestions == null || currentQuestionIndex >= currentQuizQuestions.Count)
                return string.Empty;

            var question = currentQuizQuestions[currentQuestionIndex];
            return question.Options[question.CorrectAnswerIndex];
        }

        // Advances to the next question; returns false if quiz is complete
        public bool MoveToNextQuestion()
        {
            currentQuestionIndex++;

            if (currentQuestionIndex >= currentQuizQuestions.Count)
            {
                IsQuizActive = false;
                return false;
            }

            return true;
        }

        // Returns the number of correct answers
        public int GetScore()
        {
            return correctAnswers;
        }

        // Returns the total number of questions in the current quiz
        public int GetTotalQuestions()
        {
            return currentQuizQuestions?.Count ?? 0;
        }

        // Calculates the percentage score
        public double GetPercentage()
        {
            if (currentQuizQuestions == null || currentQuizQuestions.Count == 0)
                return 0;

            return (double)correctAnswers / currentQuizQuestions.Count * 100;
        }

        // Generates final feedback based on the score percentage
        public string GetFinalFeedback()
        {
            double percentage = GetPercentage();
            int total = currentQuizQuestions?.Count ?? 0;

            if (percentage >= 90)
                return $"Great job! You scored {correctAnswers} out of {total} ({percentage:F0}%). You are a cybersecurity pro!";
            else if (percentage >= 70)
                return $"Good work! You scored {correctAnswers} out of {total} ({percentage:F0}%). You are well on your way!";
            else if (percentage >= 50)
                return $"Keep learning! You scored {correctAnswers} out of {total} ({percentage:F0}%). Review the basics to stay safe online.";
            else
                return $"Do not give up! You scored {correctAnswers} out of {total} ({percentage:F0}%). Review the fundamentals of cybersecurity.";
        }
    }
}