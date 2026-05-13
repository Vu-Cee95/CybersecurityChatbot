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

        public MainWindow()
        {
            InitializeComponent();
            audioPlayer.PlayWelcomeSound();

            AddBotMessage("Hello! Welcome to Cyber-Bot, your Cybersecurity Awareness Assistant.");
            AddBotMessage("You can ask me about password safety, phishing, privacy, scams, and safe browsing.");
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
            AddBotMessage("You can type questions like: 'Tell me about password safety', 'I am worried about scams', or 'Give me a phishing tip'.");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AddBotMessage("Chat cleared. How can I help you stay safe online?");
        }

        private void SendUserMessage()
        {
            string userInput = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                AddBotMessage("Please type something first so I can help you.");
                return;
            }

            AddUserMessage(userInput);
            UserInputTextBox.Clear();

            AddBotMessage("I received your message. In the next step, we will connect this to the chatbot response engine.");
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