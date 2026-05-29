using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
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

            WelcomeNameTextBox.GotFocus += WelcomeNameTextBox_GotFocus;
            WelcomeNameTextBox.LostFocus += WelcomeNameTextBox_LostFocus;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

            ShowWelcomePage();

            StartOnlineStatusPulse();
            StartBotAvatarBreathing();

            await Task.Run(() => audioPlayer.PlayWelcomeSound());

            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
            UpdateAllTextBarGlowStates();

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

            UpdateAllTextBarGlowStates();
            WelcomeNameTextBox.Focus();
        }

        private async Task ShowChatPageWithTransitionAsync()
        {
            ChatPage.Visibility = Visibility.Visible;
            ChatPage.Opacity = 0;

            await FadeSlideElementAsync(WelcomePage, 1, 0, 0, -18, 280);
            WelcomePage.Visibility = Visibility.Collapsed;

            await FadeSlideElementAsync(ChatPage, 0, 1, 20, 0, 360);

            UpdateAllTextBarGlowStates();
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

            if (translateTransform == null || translateTransform.IsFrozen)
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

            if (transformGroup == null || transformGroup.IsFrozen)
            {
                transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(0.96, 0.96));
                transformGroup.Children.Add(new TranslateTransform(0, 18));
                LoadingCardBorder.RenderTransform = transformGroup;
                LoadingCardBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
            TranslateTransform translateTransform = transformGroup.Children[1] as TranslateTransform;

            if (scaleTransform == null || scaleTransform.IsFrozen)
            {
                scaleTransform = new ScaleTransform(0.96, 0.96);
                transformGroup.Children[0] = scaleTransform;
            }

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(0, 18);
                transformGroup.Children[1] = translateTransform;
            }

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
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void AnimateLoadingCardOut()
        {
            if (LoadingCardBorder == null)
            {
                return;
            }

            TransformGroup transformGroup = LoadingCardBorder.RenderTransform as TransformGroup;

            if (transformGroup == null || transformGroup.IsFrozen)
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

            if (scaleTransform != null && !scaleTransform.IsFrozen)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }

            if (translateTransform != null && !translateTransform.IsFrozen)
            {
                translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
            }
        }

        private void StartLoadingProgressAnimation()
        {
            if (LoadingProgressBar == null)
            {
                return;
            }

            StopLoadingProgressAnimation();

            TranslateTransform translateTransform = LoadingProgressBar.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(-100, 0);
                LoadingProgressBar.RenderTransform = translateTransform;
            }

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

            if (scaleTransform == null || scaleTransform.IsFrozen)
            {
                scaleTransform = new ScaleTransform(1, 1);
                LoadingShieldIconBorder.RenderTransform = scaleTransform;
                LoadingShieldIconBorder.RenderTransformOrigin = new Point(0.5, 0.5);
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

        private void StartOnlineStatusPulse()
        {
            AnimateOnlineDot(WelcomeOnlineDot);
            AnimateOnlineDot(ChatOnlineDot);
        }

        private void AnimateOnlineDot(Ellipse dot)
        {
            if (dot == null)
            {
                return;
            }

            ScaleTransform scaleTransform = dot.RenderTransform as ScaleTransform;

            if (scaleTransform == null || scaleTransform.IsFrozen)
            {
                scaleTransform = new ScaleTransform(1, 1);
                dot.RenderTransform = scaleTransform;
                dot.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.55,
                Duration = TimeSpan.FromMilliseconds(1100),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.45,
                Duration = TimeSpan.FromMilliseconds(1100),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            dot.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void StartBotAvatarBreathing()
        {
            if (HeaderBotAvatarBorder == null)
            {
                return;
            }

            ScaleTransform scaleTransform = HeaderBotAvatarBorder.RenderTransform as ScaleTransform;

            if (scaleTransform == null || scaleTransform.IsFrozen)
            {
                scaleTransform = new ScaleTransform(1, 1);
                HeaderBotAvatarBorder.RenderTransform = scaleTransform;
                HeaderBotAvatarBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation breathingAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.045,
                Duration = TimeSpan.FromMilliseconds(1600),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, breathingAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, breathingAnimation);
        }

        private void AnimateInputFocusGlow(bool shouldGlow)
        {
            if (InputOuterBorder == null)
            {
                return;
            }

            AnimateTextBarGlow(InputOuterBorder, shouldGlow);
        }

        private void AnimateWelcomeNameTextBarGlow(bool shouldGlow)
        {
            Border welcomeNameBorder = FindParentBorder(WelcomeNameTextBox);

            if (welcomeNameBorder == null)
            {
                return;
            }

            AnimateTextBarGlow(welcomeNameBorder, shouldGlow);
        }

        private void AnimateTextBarGlow(Border targetBorder, bool shouldGlow)
        {
            if (targetBorder == null)
            {
                return;
            }

            Color activeBorderColor = Color.FromRgb(249, 115, 22);
            Color inactiveBorderColor = Color.FromRgb(221, 221, 221);

            SolidColorBrush borderBrush = new SolidColorBrush(shouldGlow
                ? activeBorderColor
                : inactiveBorderColor);

            targetBorder.BorderBrush = borderBrush;

            DropShadowEffect glowEffect = new DropShadowEffect
            {
                BlurRadius = shouldGlow ? 5 : 0,
                ShadowDepth = 0,
                Opacity = shouldGlow ? 0.18 : 0,
                Color = activeBorderColor
            };

            targetBorder.Effect = glowEffect;

            DoubleAnimation blurAnimation = new DoubleAnimation
            {
                To = shouldGlow ? 5 : 0,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = shouldGlow ? 0.18 : 0,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnimation);
            glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);
        }

        private Border FindParentBorder(DependencyObject child)
        {
            DependencyObject current = child;

            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);

                if (current is Border border)
                {
                    return border;
                }
            }

            return null;
        }

        private void UpdateAllTextBarGlowStates()
        {
            UpdateWelcomeNameGlowState();
            UpdateInputGlowState();
        }

        private void UpdateWelcomeNameGlowState()
        {
            if (WelcomeNameTextBox == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text);
            bool isFocused = WelcomeNameTextBox.IsKeyboardFocusWithin;

            AnimateWelcomeNameTextBarGlow(hasText || isFocused);
        }

        private void UpdateInputGlowState()
        {
            if (UserInputTextBox == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrWhiteSpace(UserInputTextBox.Text);
            bool isFocused = UserInputTextBox.IsKeyboardFocusWithin;

            AnimateInputFocusGlow(hasText || isFocused);
        }

        private void AnimateSidebarUpdateFlash()
        {
            if (SessionInfoBorder == null)
            {
                return;
            }

            SolidColorBrush flashBrush = new SolidColorBrush(Color.FromRgb(249, 115, 22));
            SessionInfoBorder.BorderBrush = flashBrush;

            ColorAnimation flashAnimation = new ColorAnimation
            {
                From = Color.FromRgb(249, 115, 22),
                To = Color.FromRgb(34, 197, 94),
                Duration = TimeSpan.FromMilliseconds(650),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            flashBrush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }

        private void StartBouncingTypingDots()
        {
            AnimateTypingDot(TypingDot1, 0);
            AnimateTypingDot(TypingDot2, 140);
            AnimateTypingDot(TypingDot3, 280);
        }

        private void AnimateTypingDot(Ellipse dot, int beginDelay)
        {
            if (dot == null)
            {
                return;
            }

            TranslateTransform translateTransform = dot.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(0, 0);
                dot.RenderTransform = translateTransform;
                dot.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation bounceAnimation = new DoubleAnimation
            {
                From = 0,
                To = -5,
                BeginTime = TimeSpan.FromMilliseconds(beginDelay),
                Duration = TimeSpan.FromMilliseconds(360),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0.45,
                To = 1,
                BeginTime = TimeSpan.FromMilliseconds(beginDelay),
                Duration = TimeSpan.FromMilliseconds(360),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, bounceAnimation);
            dot.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void StopBouncingTypingDots()
        {
            StopTypingDot(TypingDot1);
            StopTypingDot(TypingDot2);
            StopTypingDot(TypingDot3);
        }

        private void StopTypingDot(Ellipse dot)
        {
            if (dot == null)
            {
                return;
            }

            if (dot.RenderTransform is TranslateTransform translateTransform)
            {
                translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                translateTransform.Y = 0;
            }

            dot.BeginAnimation(UIElement.OpacityProperty, null);
            dot.Opacity = 1;
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
            UpdateWelcomeNameGlowState();
        }

        private void WelcomeNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeNameGlowState();
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
        }

        private void WelcomeNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeNameGlowState();
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
                UpdateWelcomeNameGlowState();
                WelcomeNameTextBox.Focus();
            }
            else
            {
                WelcomeValidationTextBlock.Text =
                    "You have used all your attempts for this step. Goodbye for now.";

                WelcomeNameTextBox.IsEnabled = false;
                StartChatButton.IsEnabled = false;
                UpdateWelcomeNameGlowState();
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
            AddBotMessage("For example, you can type: 'What is phishing?', 'How do I avoid scams?', 'I clicked a suspicious link', or 'Generate report'.");

            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateAllTextBarGlowStates();
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
            UpdateAllTextBarGlowStates();
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
            UpdateInputGlowState();
        }

        private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateInputGlowState();
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateInputGlowState();
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

            await ShowBotReplyAsync(
                "You can type naturally. Try asking:\n\n• What is phishing?\n• How do I create a strong password?\n• I clicked a suspicious link\n• How do I protect my privacy?\n• Give me an example of a scam\n• Generate report");

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
            UpdateInputGlowState();

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
                UpdateInputGlowState();
                UserInputTextBox.Focus();
                return;
            }

            AddUserMessage(userInput);

            UserInputTextBox.Clear();
            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateInputGlowState();

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
            UpdateInputGlowState();

            UserInputTextBox.Focus();
        }

        private async Task ShowBotReplyAsync(string message)
        {
            isBotTyping = true;
            UpdateSendButtonState();
            UpdateInputGlowState();

            StartTypingAnimation();

            int delay = CalculateTypingDelay(message);
            await Task.Delay(delay);

            StopTypingAnimation();

            AddBotMessage(message);

            isBotTyping = false;
            UpdateSendButtonState();
            UpdateInputGlowState();
        }

        private void StartTypingAnimation()
        {
            typingDotCount = 0;

            currentTypingMessage = typingMessages[random.Next(typingMessages.Length)];

            TypingIndicatorBorder.Visibility = Visibility.Visible;
            TypingIndicatorBorder.Opacity = 0;
            TypingIndicatorTextBlock.Text = currentTypingMessage;

            _ = FadeElementAsync(TypingIndicatorBorder, 0, 1, 180);

            StartBouncingTypingDots();

            typingDotsTimer.Start();
        }

        private void StopTypingAnimation()
        {
            typingDotsTimer.Stop();
            StopBouncingTypingDots();

            TypingIndicatorTextBlock.Text = "";
            TypingIndicatorBorder.Opacity = 0;
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

            AnimateSidebarUpdateFlash();
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
            UpdateSendButtonState();
            UpdateInputGlowState();
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

            Color backgroundColor = Color.FromRgb(245, 245, 245);
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
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                RenderTransform = new TranslateTransform(0, 12)
            };

            if (isUserMessage)
            {
                bubble.BorderThickness = new Thickness(1);
                bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(234, 88, 12));
            }
            else if (!string.IsNullOrWhiteSpace(riskLevel))
            {
                bubble.BorderThickness = new Thickness(2);
                bubble.BorderBrush = GetRiskBrush(riskLevel);
            }
            else
            {
                bubble.BorderThickness = new Thickness(1);
                bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }

            bubble.Effect = BuildCleanBubbleShadow(isUserMessage);

            StackPanel messageStack = new StackPanel
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

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
                Opacity = 0.70,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 6, 0, 0)
            };

            ApplyCrispTextRendering(senderTextBlock);
            ApplyCrispTextRendering(messageTextBlock);
            ApplyCrispTextRendering(timeTextBlock);

            messageStack.Children.Add(senderTextBlock);
            messageStack.Children.Add(messageTextBlock);
            messageStack.Children.Add(timeTextBlock);

            bubble.Child = messageStack;

            ChatPanel.Children.Add(bubble);

            AnimateMessageBubble(bubble);

            if (keepChatAtTop)
            {
                ScrollChatToTop();
            }
            else
            {
                ScrollChatToBottom();
            }
        }

        private DropShadowEffect BuildCleanBubbleShadow(bool isUserMessage)
        {
            return new DropShadowEffect
            {
                BlurRadius = isUserMessage ? 3 : 2,
                ShadowDepth = 1,
                Direction = 270,
                Opacity = isUserMessage ? 0.12 : 0.08,
                Color = Colors.Black
            };
        }

        private void ApplyCrispTextRendering(TextBlock textBlock)
        {
            if (textBlock == null)
            {
                return;
            }

            textBlock.UseLayoutRounding = true;
            textBlock.SnapsToDevicePixels = true;

            TextOptions.SetTextFormattingMode(textBlock, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(textBlock, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(textBlock, TextHintingMode.Fixed);
            RenderOptions.SetClearTypeHint(textBlock, ClearTypeHint.Enabled);
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

            ApplyCrispTextRendering(textBlock);

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

        private void AnimateMessageBubble(Border bubble)
        {
            if (bubble == null)
            {
                return;
            }

            TranslateTransform translateTransform = bubble.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(0, 12);
                bubble.RenderTransform = translateTransform;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = 12,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            bubble.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void AnimateButtonPress(Button button)
        {
            if (button == null)
            {
                return;
            }

            ScaleTransform scaleTransform = button.RenderTransform as ScaleTransform;

            if (scaleTransform == null || scaleTransform.IsFrozen)
            {
                scaleTransform = new ScaleTransform(1, 1);
                button.RenderTransform = scaleTransform;
                button.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation shrinkAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.96,
                Duration = TimeSpan.FromMilliseconds(80),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkAnimation);
        }

        private void PremiumHoverCard_MouseEnter(object sender, MouseEventArgs e)
        {
            Border card = sender as Border;

            if (card == null)
            {
                return;
            }

            TranslateTransform translateTransform = card.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(0, 0);
                card.RenderTransform = translateTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation liftAnimation = new DoubleAnimation
            {
                To = -2,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, liftAnimation);
        }

        private void PremiumHoverCard_MouseLeave(object sender, MouseEventArgs e)
        {
            Border card = sender as Border;

            if (card == null)
            {
                return;
            }

            TranslateTransform translateTransform = card.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(0, -2);
                card.RenderTransform = translateTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            DoubleAnimation dropAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, dropAnimation);
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