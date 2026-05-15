using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using CybersecurityChatbotGUI.Services;

namespace CybersecurityChatbotGUI
{
    public partial class MainWindow : Window
    {
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        private readonly ChatbotEngine chatbotEngine = new ChatbotEngine();
        private readonly Validator validator = new Validator();

        private readonly DispatcherTimer typingDotsTimer = new DispatcherTimer();

        private bool isChatEnded = false;
        private bool isBotTyping = false;
        private bool keepChatAtTop = true;

        private int nameAttempts = 0;
        private int typingDotCount = 0;

        private string userName = "User";

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            typingDotsTimer.Interval = TimeSpan.FromMilliseconds(420);
            typingDotsTimer.Tick += TypingDotsTimer_Tick;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWelcomePage();

            await Task.Run(() => audioPlayer.PlayWelcomeSound());

            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();

            WelcomeNameTextBox.Focus();
        }

        private void ShowWelcomePage()
        {
            WelcomePage.Visibility = Visibility.Visible;
            WelcomePage.Opacity = 1;

            ChatPage.Visibility = Visibility.Collapsed;
            ChatPage.Opacity = 0;
        }

        private async Task ShowWelcomePageWithTransitionAsync()
        {
            WelcomePage.Visibility = Visibility.Visible;
            WelcomePage.Opacity = 0;

            await FadeElementAsync(ChatPage, 1, 0, 260);
            ChatPage.Visibility = Visibility.Collapsed;

            await FadeElementAsync(WelcomePage, 0, 1, 320);

            WelcomeNameTextBox.Focus();
        }

        private async Task ShowChatPageWithTransitionAsync()
        {
            ChatPage.Visibility = Visibility.Visible;
            ChatPage.Opacity = 0;

            await FadeElementAsync(WelcomePage, 1, 0, 260);
            WelcomePage.Visibility = Visibility.Collapsed;

            await FadeElementAsync(ChatPage, 0, 1, 320);

            UserInputTextBox.Focus();
            ScrollChatToTop();
        }

        private async Task ShowLoadingOverlayAsync(string statusMessage)
        {
            LoadingStatusTextBlock.Text = statusMessage;
            LoadingOverlay.Visibility = Visibility.Visible;
            await FadeElementAsync(LoadingOverlay, 0, 1, 220);
        }

        private async Task HideLoadingOverlayAsync()
        {
            await FadeElementAsync(LoadingOverlay, 1, 0, 220);
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private Task FadeElementAsync(UIElement element, double from, double to, int milliseconds)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += (sender, args) =>
            {
                completionSource.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            return completionSource.Task;
        }

        private async void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            await TryStartChatAsync();
        }

        private async void WelcomeNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && StartChatButton.IsEnabled)
            {
                await TryStartChatAsync();
            }
        }

        private void WelcomeNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
        }

        private async Task TryStartChatAsync()
        {
            string enteredName = WelcomeNameTextBox.Text.Trim();

            if (validator.IsValidName(enteredName))
            {
                userName = enteredName;
                chatbotEngine.SetUserName(userName);

                nameAttempts = 0;
                isChatEnded = false;

                WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52));
                WelcomeValidationTextBlock.Text = $"Welcome, {userName}. Preparing your secure chat...";

                await ShowLoadingOverlayAsync("Setting up CyberBot and preparing your cybersecurity guidance...");
                await Task.Delay(900);

                PrepareChatSession();

                LoadingStatusTextBlock.Text = "Opening your secure chat...";
                await Task.Delay(500);

                await ShowChatPageWithTransitionAsync();
                await HideLoadingOverlayAsync();

                return;
            }

            nameAttempts++;

            int attemptsLeft = 3 - nameAttempts;

            WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28));

            if (nameAttempts < 3)
            {
                WelcomeValidationTextBlock.Text =
                    $"That name does not meet the requirements. Attempts left: {attemptsLeft}. Use at least 3 characters and include at least one letter.";

                WelcomeNameTextBox.Clear();
                UpdateWelcomePlaceholderState();
                UpdateStartChatButtonState();
                WelcomeNameTextBox.Focus();
            }
            else
            {
                WelcomeValidationTextBlock.Text =
                    "You have used all your attempts for this step. Goodbye for now.";

                WelcomeNameTextBox.IsEnabled = false;
                StartChatButton.IsEnabled = false;
            }
        }

        private void PrepareChatSession()
        {
            ChatPanel.Children.Clear();
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;

            keepChatAtTop = true;

            UpdateSessionPanel();

            AddBotMessage($"Welcome, {userName}. I am CyberBot, your cybersecurity awareness assistant.");
            AddBotMessage("You can ask me about passwords, phishing, scams, privacy, safe browsing, malware, 2FA, or what to do if something suspicious already happened.");
            AddBotMessage("For example, you can type: 'What is phishing?', 'How do I avoid scams?', 'I clicked a suspicious link', or 'Give me an example'.");

            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        private void ResetWelcomePage()
        {
            nameAttempts = 0;
            userName = "User";
            isChatEnded = false;
            isBotTyping = false;

            typingDotsTimer.Stop();

            chatbotEngine.ResetConversationButKeepUser();
            chatbotEngine.SetUserName(userName);

            ChatPanel.Children.Clear();
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;
            TypingIndicatorTextBlock.Text = "";

            UserInputTextBox.Clear();
            UserInputTextBox.IsEnabled = true;
            SendButton.IsEnabled = false;
            MenuDotsButton.IsEnabled = true;

            WelcomeNameTextBox.Clear();
            WelcomeNameTextBox.IsEnabled = true;
            StartChatButton.IsEnabled = false;

            WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
            WelcomeValidationTextBlock.Text = "Name must be at least 3 characters and include at least one letter.";

            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
            UpdatePlaceholderState();
        }

        private void UpdateWelcomePlaceholderState()
        {
            if (WelcomeNamePlaceholderTextBlock == null || WelcomeNameTextBox == null)
            {
                return;
            }

            WelcomeNamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateStartChatButtonState()
        {
            if (StartChatButton == null || WelcomeNameTextBox == null)
            {
                return;
            }

            StartChatButton.IsEnabled =
                !string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text) &&
                nameAttempts < 3;
        }

        private async void ExitApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            await ExitApplicationAsync();
        }

        private async Task ExitApplicationAsync()
        {
            bool confirmExit = CyberDialog.ShowConfirmation(
                this,
                "Confirm Exit",
                "Are you sure you want to exit CyberBot?");

            if (!confirmExit)
            {
                WelcomeNameTextBox.Focus();
                return;
            }

            CyberDialog.ShowMessage(
                this,
                "CyberBot Signing Off",
                "Stay safe out there! Remember: the safest click is the one you think about first. CyberBot is signing off.");

            await Task.Delay(150);

            Application.Current.Shutdown();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendUserMessageAsync();
        }

        private async void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SendButton.IsEnabled)
            {
                await SendUserMessageAsync();
            }
        }

        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        private void MenuDotsButton_Click(object sender, RoutedEventArgs e)
        {
            ChatOptionsContextMenu.PlacementTarget = MenuDotsButton;
            ChatOptionsContextMenu.IsOpen = true;
        }

        private async void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await ShowHelpMessageAsync();
        }

        private void NewChatMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartNewChat();
        }

        private async void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await LogoutAsync();
        }

        private async Task ShowHelpMessageAsync()
        {
            if (isChatEnded || isBotTyping)
            {
                return;
            }

            keepChatAtTop = false;

            await ShowBotReplyAsync("You can type naturally. Try asking:\n\n• What is phishing?\n• How do I create a strong password?\n• I clicked a suspicious link\n• How do I protect my privacy?\n• Give me an example of a scam\n• Summarise this chat");

            UserInputTextBox.Focus();
        }

        private void StartNewChat()
        {
            ChatPanel.Children.Clear();
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;

            keepChatAtTop = true;

            if (isChatEnded)
            {
                AddBotMessage("The chat session has ended. Please restart the application to begin again.");
                ScrollChatToTop();
                return;
            }

            chatbotEngine.ResetConversationButKeepUser();

            AddBotMessage($"New chat started, {userName}. How can I help you stay safe online?");
            AddBotMessage("You can ask about password safety, phishing, scams, privacy, safe browsing, malware, or 2FA.");

            UserInputTextBox.Clear();

            UpdateSessionPanel();
            UpdatePlaceholderState();
            UpdateSendButtonState();

            ScrollChatToTop();
        }

        private async Task LogoutAsync()
        {
            if (isBotTyping)
            {
                StopTypingAnimation();
                isBotTyping = false;
            }

            bool confirmLogout = CyberDialog.ShowConfirmation(
                this,
                "Confirm Log Out",
                "Are you sure you want to log out of this secure CyberBot session?");

            if (!confirmLogout)
            {
                UserInputTextBox.Focus();
                return;
            }

            CyberDialog.ShowMessage(
                this,
                "Logged Out Safely",
                $"Stay safe out there, {userName}! Remember: pause, check, and verify before you click. CyberBot will be ready when you return.");

            ResetWelcomePage();

            await ShowWelcomePageWithTransitionAsync();
        }

        private async Task SendUserMessageAsync()
        {
            if (isChatEnded || isBotTyping)
            {
                return;
            }

            keepChatAtTop = false;

            string userInput = UserInputTextBox.Text.Trim();

            if (validator.IsEmpty(userInput))
            {
                await ShowBotReplyAsync("Please type a message first.");
                UpdatePlaceholderState();
                UpdateSendButtonState();
                UserInputTextBox.Focus();
                return;
            }

            AddUserMessage(userInput);

            UserInputTextBox.Clear();
            UpdatePlaceholderState();
            UpdateSendButtonState();

            if (!validator.IsMeaningfulInput(userInput))
            {
                await ShowBotReplyAsync("I could not understand that clearly. Try asking about passwords, phishing, scams, privacy, malware, safe browsing, or 2FA.");
                UpdateSessionPanel();
                UserInputTextBox.Focus();
                return;
            }

            string botResponse = chatbotEngine.ProcessMessage(userInput);

            await ShowBotReplyAsync(botResponse);

            UpdateSessionPanel();
            UpdatePlaceholderState();
            UpdateSendButtonState();

            UserInputTextBox.Focus();
        }

        private async Task ShowBotReplyAsync(string message)
        {
            isBotTyping = true;
            UpdateSendButtonState();

            StartTypingAnimation();

            int delay = CalculateTypingDelay(message);
            await Task.Delay(delay);

            StopTypingAnimation();

            AddBotMessage(message);

            isBotTyping = false;
            UpdateSendButtonState();
        }

        private void StartTypingAnimation()
        {
            typingDotCount = 0;

            TypingIndicatorBorder.Visibility = Visibility.Visible;
            TypingIndicatorTextBlock.Text = "CyberBot is typing";

            typingDotsTimer.Start();
        }

        private void StopTypingAnimation()
        {
            typingDotsTimer.Stop();

            TypingIndicatorTextBlock.Text = "";
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;
        }

        private void TypingDotsTimer_Tick(object? sender, EventArgs e)
        {
            typingDotCount++;

            if (typingDotCount > 3)
            {
                typingDotCount = 0;
            }

            TypingIndicatorTextBlock.Text = "CyberBot is typing" + new string('.', typingDotCount);
        }

        private int CalculateTypingDelay(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return 3000;
            }

            int delay = message.Length * 20;

            if (delay < 2200)
            {
                delay = 2200;
            }

            if (delay > 5200)
            {
                delay = 5200;
            }

            return delay;
        }

        private void UpdateSessionPanel()
        {
            SessionUserTextBlock.Text = $"User: {userName}";
            SessionLastTopicTextBlock.Text = $"Last topic: {chatbotEngine.LastTopicDisplay}";
            SessionMoodTextBlock.Text = $"Mood: {chatbotEngine.LastSentimentDisplay}";
        }

        private void EndChatSession()
        {
            isChatEnded = true;
            isBotTyping = false;

            StopTypingAnimation();

            UserInputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            MenuDotsButton.IsEnabled = false;

            UserInputTextBox.Clear();
            UpdatePlaceholderState();
        }

        private void UpdatePlaceholderState()
        {
            if (PlaceholderTextBlock == null || UserInputTextBox == null)
            {
                return;
            }

            PlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateSendButtonState()
        {
            if (SendButton == null || UserInputTextBox == null)
            {
                return;
            }

            string input = UserInputTextBox.Text.Trim();

            bool hasRealText =
                !string.IsNullOrWhiteSpace(input) &&
                !isChatEnded &&
                !isBotTyping;

            SendButton.IsEnabled = hasRealText;
        }

        private void AddUserMessage(string message)
        {
            AddMessageBubble(
                userName,
                message,
                DateTime.Now.ToString("HH:mm"),
                Color.FromRgb(249, 115, 22),
                Colors.White,
                HorizontalAlignment.Right,
                true
            );
        }

        private void AddBotMessage(string message)
        {
            AddMessageBubble(
                "CyberBot",
                message,
                DateTime.Now.ToString("HH:mm"),
                Color.FromRgb(237, 237, 237),
                Color.FromRgb(17, 17, 17),
                HorizontalAlignment.Left,
                false
            );
        }

        private void AddMessageBubble(
            string sender,
            string message,
            string time,
            Color backgroundColor,
            Color foregroundColor,
            HorizontalAlignment alignment,
            bool isUserMessage)
        {
            double maxBubbleWidth = GetResponsiveBubbleMaxWidth();

            Thickness bubbleMargin = isUserMessage
                ? new Thickness(100, 7, 0, 7)
                : new Thickness(0, 7, 100, 7);

            Border bubble = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(15, 10, 15, 9),
                Margin = bubbleMargin,
                HorizontalAlignment = alignment,
                MinWidth = 175,
                MaxWidth = maxBubbleWidth
            };

            bubble.Effect = new DropShadowEffect
            {
                BlurRadius = 8,
                ShadowDepth = 1,
                Opacity = 0.10
            };

            StackPanel messageStack = new StackPanel();

            TextBlock senderTextBlock = new TextBlock
            {
                Text = sender,
                Foreground = new SolidColorBrush(foregroundColor),
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Opacity = 0.9,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(foregroundColor),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                LineHeight = 20,
                MaxWidth = maxBubbleWidth - 30
            };

            TextBlock timeTextBlock = new TextBlock
            {
                Text = time,
                Foreground = new SolidColorBrush(foregroundColor),
                FontSize = 9.5,
                FontWeight = FontWeights.SemiBold,
                Opacity = 0.65,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 6, 0, 0)
            };

            messageStack.Children.Add(senderTextBlock);
            messageStack.Children.Add(messageTextBlock);
            messageStack.Children.Add(timeTextBlock);

            bubble.Child = messageStack;

            ChatPanel.Children.Add(bubble);

            if (keepChatAtTop)
            {
                ScrollChatToTop();
            }
            else
            {
                ScrollChatToBottom();
            }
        }

        private double GetResponsiveBubbleMaxWidth()
        {
            double chatWidth = ChatScrollViewer.ActualWidth;

            if (chatWidth <= 0)
            {
                return 560;
            }

            double calculatedWidth = chatWidth * 0.70;

            if (calculatedWidth < 300)
            {
                return 300;
            }

            if (calculatedWidth > 610)
            {
                return 610;
            }

            return calculatedWidth;
        }

        private void ScrollChatToTop()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToTop();
            }), DispatcherPriority.Background);
        }

        private void ScrollChatToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }), DispatcherPriority.Background);
        }
    }
}