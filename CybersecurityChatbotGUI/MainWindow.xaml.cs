using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbotGUI.Services;

namespace CybersecurityChatbotGUI
{
    public partial class MainWindow : Window
    {
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        private readonly ChatbotEngine chatbotEngine = new ChatbotEngine();

        public MainWindow()
        {
            InitializeComponent();

            audioPlayer.PlayWelcomeSound();

            AddBotMessage("Hello! Welcome to Cyber-Bot, your Cybersecurity Awareness Assistant.");
            AddBotMessage("You can ask me about password safety, phishing, privacy, scams, safe browsing, malware, and 2FA.");

            UserInputTextBox.Focus();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendUserMessage();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendUserMessage();
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotMessage("You can type questions like: 'Tell me about password safety', 'I am worried about scams', 'Give me a phishing tip', 'How do I protect my privacy?', or 'Tell me more'.");
            UserInputTextBox.Focus();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();

            AddBotMessage("Chat cleared. How can I help you stay safe online?");
            UserInputTextBox.Focus();
        }

        private void SendUserMessage()
        {
            string userInput = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                AddBotMessage("Please type something first so I can help you.");
                UserInputTextBox.Focus();
                return;
            }

            AddUserMessage(userInput);
            UserInputTextBox.Clear();

            string botResponse = chatbotEngine.ProcessUserMessage(userInput);
            AddBotMessage(botResponse);

            UserInputTextBox.Focus();
        }

        private void AddUserMessage(string message)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = "You: " + message,
                Background = new SolidColorBrush(Color.FromRgb(219, 234, 254)),
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                Padding = new Thickness(12),
                Margin = new Thickness(80, 5, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(textBlock);
            ChatScrollViewer.ScrollToEnd();
        }

        private void AddBotMessage(string message)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = "Cyber-Bot: " + message,
                Background = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 5, 80, 5),
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(textBlock);
            ChatScrollViewer.ScrollToEnd();
        }
    }
}