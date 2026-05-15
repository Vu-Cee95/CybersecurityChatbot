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
        private readonly Random random = new Random();

        private Storyboard loadingProgressStoryboard;

        private bool isChatEnded = false;
        private bool isBotTyping = false;
        private bool keepChatAtTop = true;

        private int nameAttempts = 0;
        private int typingDotCount = 0;

        private string userName = "User";
        private string currentTypingMessage = "CyberBot is typing";

        private readonly string[] typingMessages =
        {
            "CyberBot is analysing your message",
            "CyberBot is checking the risk level",
            "CyberBot is reviewing possible red flags",
            "CyberBot is preparing safe guidance",
            "CyberBot is connecting the conversation context",
            "CyberBot is thinking securely"
        };

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

            await FadeSlideElementAsync(ChatPage, 1, 0, 0, 18, 280);
            ChatPage.Visibility = Visibility.Collapsed;

            await FadeSlideElementAsync(WelcomePage, 0, 1, 18, 0, 360);

            WelcomeNameTextBox.Focus();
        }

        private async Task ShowChatPageWithTransitionAsync()
        {
            ChatPage.Visibility = Visibility.Visible;
            ChatPage.Opacity = 0;

            await FadeSlideElementAsync(WelcomePage, 1, 0, 0, -18, 280);
            WelcomePage.Visibility = Visibility.Collapsed;

            await FadeSlideElementAsync(ChatPage, 0, 1, 20, 0, 360);

            UserInputTextBox.Focus();
            ScrollChatToTop();
        }

        private async Task ShowLoadingOverlayAsync(string statusMessage)
        {
            LoadingStatusTextBlock.Text = statusMessage;
            LoadingOverlay.Visibility = Visibility.Visible;

            if (LoadingCardBorder != null)
            {
                LoadingCardBorder.Opacity = 0;
            }

            await FadeElementAsync(LoadingOverlay, 0, 1, 180);

            AnimateLoadingCardIn();
            StartLoadingProgressAnimation();
            StartLoadingIconPulse();
        }

        private async Task HideLoadingOverlayAsync()
        {
            StopLoadingProgressAnimation();

            if (LoadingCardBorder != null)
            {
                AnimateLoadingCardOut();
                await Task.Delay(180);
            }

            await FadeElementAsync(LoadingOverlay, 1, 0, 200);
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
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            animation.Completed += (sender, args) =>
            {
                completionSource.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            return completionSource.Task;
        }

        private Task FadeSlideElementAsync(
            UIElement element,
            double fromOpacity,
            double toOpacity,
            double fromY,
            double toY,
            int milliseconds)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            TranslateTransform translateTransform = element.RenderTransform as TranslateTransform;

            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform();
                element.RenderTransform = translateTransform;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = fromY,
                To = toY,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            opacityAnimation.Completed += (sender, args) =>
            {
                completionSource.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);

            return completionSource.Task;
        }

        private void AnimateLoadingCardIn()
        {
            if (LoadingCardBorder == null)
            {
                return;
            }

            LoadingCardBorder.Opacity = 0;

            TransformGroup transformGroup = LoadingCardBorder.RenderTransform as TransformGroup;

            if (transformGroup == null)
            {
                return;
            }

            ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
            TranslateTransform translateTransform = transformGroup.Children[1] as TranslateTransform;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                From = 0.94,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(320),
                EasingFunction = new BackEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Amplitude = 0.22
                }
            };

            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = 18,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(320),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            LoadingCardBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            translateTransform?.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void AnimateLoadingCardOut()
        {
            if (LoadingCardBorder == null)
            {
                return;
            }

            TransformGroup transformGroup = LoadingCardBorder.RenderTransform as TransformGroup;

            if (transformGroup == null)
            {
                return;
            }

            ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
            TranslateTransform translateTransform = transformGroup.Children[1] as TranslateTransform;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 0.97,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                To = 10,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            LoadingCardBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            translateTransform?.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void StartLoadingProgressAnimation()
        {
            if (LoadingProgressBar == null)
            {
                return;
            }

            StopLoadingProgressAnimation();

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = -110,
                To = 390,
                Duration = TimeSpan.FromMilliseconds(1350),
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            loadingProgressStoryboard = new Storyboard();
            loadingProgressStoryboard.Children.Add(progressAnimation);

            Storyboard.SetTarget(progressAnimation, LoadingProgressBar);
            Storyboard.SetTargetProperty(
                progressAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            loadingProgressStoryboard.Begin();
        }

        private void StopLoadingProgressAnimation()
        {
            if (loadingProgressStoryboard != null)
            {
                loadingProgressStoryboard.Stop();
                loadingProgressStoryboard = null;
            }
        }

        private void StartLoadingIconPulse()
        {
            if (LoadingShieldIconBorder == null)
            {
                return;
            }

            ScaleTransform scaleTransform = LoadingShieldIconBorder.RenderTransform as ScaleTransform;

            if (scaleTransform == null)
            {
                return;
            }

            DoubleAnimation pulseAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.07,
                Duration = TimeSpan.FromMilliseconds(650),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
        }

        private async void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
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
                WelcomeValidationTextBlock.Text = $"Welcome, {userName}. Preparing your secure chat.";

                await ShowLoadingOverlayAsync("Setting up CyberBot and preparing your cybersecurity guidance.");
                await Task.Delay(900);

                PrepareChatSession();

                LoadingStatusTextBlock.Text = "Opening your secure chat.";
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
            AnimateButtonPress(sender as Button);
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
            AnimateButtonPress(sender as Button);
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
            AnimateButtonPress(sender as Button);

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

            await ShowBotReplyAsync("You can type naturally. Try asking:\n\n• What is phishing?\n• How do I create a strong password?\n• I clicked a suspicious link\n• How do I protect my privacy?\n• Give me an example of a scam\n• Generate report");

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

            currentTypingMessage = typingMessages[random.Next(typingMessages.Length)];

            TypingIndicatorBorder.Visibility = Visibility.Visible;
            TypingIndicatorBorder.Opacity = 0;
            TypingIndicatorTextBlock.Text = currentTypingMessage;

            FadeElementAsync(TypingIndicatorBorder, 0, 1, 180);

            typingDotsTimer.Start();
        }

        private void StopTypingAnimation()
        {
            typingDotsTimer.Stop();

            FadeElementAsync(TypingIndicatorBorder, TypingIndicatorBorder.Opacity, 0, 140);

            TypingIndicatorTextBlock.Text = "";
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;
        }

        private void TypingDotsTimer_Tick(object sender, EventArgs e)
        {
            typingDotCount++;

            if (typingDotCount > 3)
            {
                typingDotCount = 0;
            }

            TypingIndicatorTextBlock.Text = currentTypingMessage + new string('.', typingDotCount);
        }

        private int CalculateTypingDelay(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return 1700;
            }

            int delay = message.Length * 18;

            if (message.Contains("Risk Level: HIGH") || message.Contains("Risk Level: EMERGENCY"))
            {
                delay += 700;
            }

            if (delay < 1700)
            {
                delay = 1700;
            }

            if (delay > 4800)
            {
                delay = 4800;
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
                true);
        }

        private void AddBotMessage(string message)
        {
            string riskLevel = DetectRiskLevelFromMessage(message);

            Color backgroundColor = Color.FromRgb(237, 237, 237);
            Color foregroundColor = Color.FromRgb(17, 17, 17);

            if (riskLevel == "Emergency")
            {
                backgroundColor = Color.FromRgb(255, 237, 213);
            }
            else if (riskLevel == "High")
            {
                backgroundColor = Color.FromRgb(255, 247, 237);
            }
            else if (riskLevel == "Medium")
            {
                backgroundColor = Color.FromRgb(255, 251, 235);
            }

            AddMessageBubble(
                "CyberBot",
                message,
                DateTime.Now.ToString("HH:mm"),
                backgroundColor,
                foregroundColor,
                HorizontalAlignment.Left,
                false);
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

            string riskLevel = isUserMessage ? "" : DetectRiskLevelFromMessage(message);

            Border bubble = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(15, 10, 15, 9),
                Margin = bubbleMargin,
                HorizontalAlignment = alignment,
                MinWidth = 175,
                MaxWidth = maxBubbleWidth,
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(0.96, 0.96));
            transformGroup.Children.Add(new TranslateTransform(0, 14));
            bubble.RenderTransform = transformGroup;

            bubble.Effect = new DropShadowEffect
            {
                BlurRadius = isUserMessage ? 10 : 12,
                ShadowDepth = 1,
                Opacity = isUserMessage ? 0.13 : 0.11
            };

            if (!isUserMessage && riskLevel != "")
            {
                bubble.BorderThickness = new Thickness(2);
                bubble.BorderBrush = GetRiskBrush(riskLevel);
            }

            StackPanel messageStack = new StackPanel();

            if (!isUserMessage && riskLevel != "")
            {
                Border riskBanner = BuildRiskBanner(riskLevel);
                messageStack.Children.Add(riskBanner);
            }

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

            AnimateMessageBubble(bubble, isUserMessage);

            if (riskLevel == "Emergency")
            {
                AnimateEmergencyPulse(bubble);
            }

            if (keepChatAtTop)
            {
                ScrollChatToTop();
            }
            else
            {
                ScrollChatToBottom();
            }
        }

        private Border BuildRiskBanner(string riskLevel)
        {
            string bannerText = "CYBER RISK DETECTED";

            if (riskLevel == "Emergency")
            {
                bannerText = "🚨 CYBER EMERGENCY MODE";
            }
            else if (riskLevel == "High")
            {
                bannerText = "⚠ HIGH RISK GUIDANCE";
            }
            else if (riskLevel == "Medium")
            {
                bannerText = "⚠ MEDIUM RISK CHECK";
            }
            else if (riskLevel == "Low")
            {
                bannerText = "✓ LOW RISK LEARNING";
            }

            Border banner = new Border
            {
                Background = GetRiskBrush(riskLevel),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock textBlock = new TextBlock
            {
                Text = bannerText,
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };

            banner.Child = textBlock;

            return banner;
        }

        private SolidColorBrush GetRiskBrush(string riskLevel)
        {
            switch (riskLevel)
            {
                case "Emergency":
                    return new SolidColorBrush(Color.FromRgb(185, 28, 28));

                case "High":
                    return new SolidColorBrush(Color.FromRgb(234, 88, 12));

                case "Medium":
                    return new SolidColorBrush(Color.FromRgb(249, 115, 22));

                case "Low":
                    return new SolidColorBrush(Color.FromRgb(34, 197, 94));

                default:
                    return new SolidColorBrush(Color.FromRgb(249, 115, 22));
            }
        }

        private string DetectRiskLevelFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "";
            }

            string lowerMessage = message.ToLower();

            if (lowerMessage.Contains("risk level: emergency") ||
                lowerMessage.Contains("risk level: emergency") ||
                lowerMessage.Contains("cyber emergency"))
            {
                return "Emergency";
            }

            if (lowerMessage.Contains("risk level: high"))
            {
                return "High";
            }

            if (lowerMessage.Contains("risk level: medium"))
            {
                return "Medium";
            }

            if (lowerMessage.Contains("risk level: low"))
            {
                return "Low";
            }

            return "";
        }

        private void AnimateMessageBubble(Border bubble, bool isUserMessage)
        {
            TransformGroup transformGroup = bubble.RenderTransform as TransformGroup;

            if (transformGroup == null)
            {
                return;
            }

            ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
            TranslateTransform translateTransform = transformGroup.Children[1] as TranslateTransform;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                From = 0.96,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new BackEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Amplitude = 0.20
                }
            };

            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = 14,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(280),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            bubble.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            translateTransform?.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void AnimateEmergencyPulse(Border bubble)
        {
            DoubleAnimation pulseAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.72,
                Duration = TimeSpan.FromMilliseconds(520),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(2),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            if (bubble.BorderBrush is SolidColorBrush borderBrush)
            {
                borderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, pulseAnimation);
            }
        }

        private void AnimateButtonPress(Button button)
        {
            if (button == null || button.RenderTransform == null)
            {
                return;
            }

            ScaleTransform scaleTransform = button.RenderTransform as ScaleTransform;

            if (scaleTransform == null)
            {
                return;
            }

            DoubleAnimation shrinkAnimation = new DoubleAnimation
            {
                To = 0.94,
                Duration = TimeSpan.FromMilliseconds(80),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkAnimation);
        }

        private double GetResponsiveBubbleMaxWidth()
        {
            double chatWidth = ChatScrollViewer.ActualWidth;

            if (chatWidth <= 0)
            {
                return 560;
            }

            double calculatedWidth = chatWidth * 0.68;

            if (calculatedWidth < 360)
            {
                return 360;
            }

            if (calculatedWidth > 650)
            {
                return 650;
            }

            return calculatedWidth;
        }

        private void ScrollChatToTop()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToTop();
        }

        private void ScrollChatToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }
    }
}