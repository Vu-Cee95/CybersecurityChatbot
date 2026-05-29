// Required namespaces for UI, animations, threading, and application services
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
    // Main window class handling all UI logic, animations, and user interactions
    public partial class MainWindow : Window
    {
        // Service instances for audio playback and chatbot logic
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        private readonly ChatbotEngine chatbotEngine = new ChatbotEngine();
        private readonly Validator validator = new Validator();

        // Timer for animating typing indicator dots
        private readonly DispatcherTimer typingDotsTimer = new DispatcherTimer();
        // Random number generator for selecting random typing messages
        private readonly Random random = new Random();

        // Storyboard for loading progress bar animation
        private Storyboard loadingProgressStoryboard;

        // State flags tracking chat session status
        private bool isChatEnded = false;
        private bool isBotTyping = false;
        private bool keepChatAtTop = true;

        // Counter for name validation attempts (max 3)
        private int nameAttempts = 0;
        // Counter for typing dot animation cycle
        private int typingDotCount = 0;

        // Current user name and typing indicator message
        private string userName = "User";
        private string currentTypingMessage = "CyberBot is typing";

        // Array of random messages shown while bot generates response
        private readonly string[] typingMessages =
        {
            "CyberBot is analysing your message",
            "CyberBot is checking the risk level",
            "CyberBot is reviewing possible red flags",
            "CyberBot is preparing safe guidance",
            "CyberBot is connecting the conversation context",
            "CyberBot is thinking securely"
        };

        // Constructor: initializes components and sets up event handlers
        public MainWindow()
        {
            InitializeComponent();

            // Window loaded event for post-initialization setup
            Loaded += MainWindow_Loaded;

            // Configure typing dots timer (fires every 420ms)
            typingDotsTimer.Interval = TimeSpan.FromMilliseconds(420);
            typingDotsTimer.Tick += TypingDotsTimer_Tick;

            // Focus events for welcome name input field
            WelcomeNameTextBox.GotFocus += WelcomeNameTextBox_GotFocus;
            WelcomeNameTextBox.LostFocus += WelcomeNameTextBox_LostFocus;
        }

        // Main window loaded handler: sets up visual quality and starts animations
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Enable layout rounding for crisp rendering
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            // Configure ClearType text rendering settings
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

            // Show welcome page on startup
            ShowWelcomePage();

            // Start ambient animations (status dots, avatar breathing)
            StartOnlineStatusPulse();
            StartBotAvatarBreathing();

            // Play welcome sound asynchronously
            await Task.Run(() => audioPlayer.PlayWelcomeSound());

            // Update UI state for input fields and glow effects
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
            UpdateAllTextBarGlowStates();

            // Set focus to name input field
            WelcomeNameTextBox.Focus();
        }

        // Shows welcome page and hides chat page immediately (no transition)
        private void ShowWelcomePage()
        {
            WelcomePage.Visibility = Visibility.Visible;
            WelcomePage.Opacity = 1;

            ChatPage.Visibility = Visibility.Collapsed;
            ChatPage.Opacity = 0;
        }

        // Animated transition from chat page back to welcome page
        private async Task ShowWelcomePageWithTransitionAsync()
        {
            WelcomePage.Visibility = Visibility.Visible;
            WelcomePage.Opacity = 0;

            // Fade out and slide chat page away
            await FadeSlideElementAsync(ChatPage, 1, 0, 0, 18, 280);
            ChatPage.Visibility = Visibility.Collapsed;

            // Fade in and slide welcome page in from above
            await FadeSlideElementAsync(WelcomePage, 0, 1, 18, 0, 360);

            UpdateAllTextBarGlowStates();
            WelcomeNameTextBox.Focus();
        }

        // Animated transition from welcome page to chat page
        private async Task ShowChatPageWithTransitionAsync()
        {
            ChatPage.Visibility = Visibility.Visible;
            ChatPage.Opacity = 0;

            // Fade out and slide welcome page away
            await FadeSlideElementAsync(WelcomePage, 1, 0, 0, -18, 280);
            WelcomePage.Visibility = Visibility.Collapsed;

            // Fade in and slide chat page in from below
            await FadeSlideElementAsync(ChatPage, 0, 1, 20, 0, 360);

            UpdateAllTextBarGlowStates();
            UserInputTextBox.Focus();
            ScrollChatToTop();
        }

        // Shows loading overlay with status message and entrance animations
        private async Task ShowLoadingOverlayAsync(string statusMessage)
        {
            LoadingStatusTextBlock.Text = statusMessage;
            LoadingOverlay.Visibility = Visibility.Visible;

            // Reset card opacity before entrance animation
            if (LoadingCardBorder != null)
            {
                LoadingCardBorder.Opacity = 0;
            }

            // Fade in the overlay background
            await FadeElementAsync(LoadingOverlay, 0, 1, 180);

            // Start loading card and progress bar animations
            AnimateLoadingCardIn();
            StartLoadingProgressAnimation();
            StartLoadingIconPulse();
        }

        // Hides loading overlay with exit animation
        private async Task HideLoadingOverlayAsync()
        {
            StopLoadingProgressAnimation();

            // Animate card out and wait briefly
            if (LoadingCardBorder != null)
            {
                AnimateLoadingCardOut();
                await Task.Delay(180);
            }

            // Fade out the overlay background
            await FadeElementAsync(LoadingOverlay, 1, 0, 200);
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        // Simple fade animation for any UIElement
        private Task FadeElementAsync(UIElement element, double from, double to, int milliseconds)
        {
            // Use TaskCompletionSource to await animation completion
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Signal completion when animation finishes
            animation.Completed += (sender, args) =>
            {
                completionSource.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            return completionSource.Task;
        }

        // Combined fade and slide animation for page transitions
        private Task FadeSlideElementAsync(
            UIElement element,
            double fromOpacity,
            double toOpacity,
            double fromY,
            double toY,
            int milliseconds)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            // Get or create translate transform for vertical movement
            TranslateTransform translateTransform = element.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform();
                element.RenderTransform = translateTransform;
            }

            // Opacity animation
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Slide animation (Y-axis movement)
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = fromY,
                To = toY,
                Duration = TimeSpan.FromMilliseconds(milliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Signal completion when opacity animation finishes
            opacityAnimation.Completed += (sender, args) =>
            {
                completionSource.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);

            return completionSource.Task;
        }

        // Entrance animation for loading card (scale up and slide in)
        private void AnimateLoadingCardIn()
        {
            if (LoadingCardBorder == null)
            {
                return;
            }

            LoadingCardBorder.Opacity = 0;

            // Set up transform group (scale + translate)
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

            // Ensure transforms are writable
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

            // Fade in animation
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Scale up with slight bounce effect
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

            // Slide up from below
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = 18,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(320),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Start all animations
            LoadingCardBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        // Exit animation for loading card (fade out and shrink)
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

            // Fade out
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Scale down slightly
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 0.97,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Slide down slightly
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

        // Looping progress bar animation (slides orange bar across)
        private void StartLoadingProgressAnimation()
        {
            if (LoadingProgressBar == null)
            {
                return;
            }

            StopLoadingProgressAnimation();

            // Get or create translate transform for horizontal movement
            TranslateTransform translateTransform = LoadingProgressBar.RenderTransform as TranslateTransform;

            if (translateTransform == null || translateTransform.IsFrozen)
            {
                translateTransform = new TranslateTransform(-100, 0);
                LoadingProgressBar.RenderTransform = translateTransform;
            }

            // Slide from left to right, repeating forever
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

        // Stops and cleans up progress bar animation
        private void StopLoadingProgressAnimation()
        {
            if (loadingProgressStoryboard != null)
            {
                loadingProgressStoryboard.Stop();
                loadingProgressStoryboard = null;
            }
        }

        // Pulse animation for shield icon during loading (scales up/down 3 times)
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

        // Starts pulsing animation for online status indicator dots
        private void StartOnlineStatusPulse()
        {
            AnimateOnlineDot(WelcomeOnlineDot);
            AnimateOnlineDot(ChatOnlineDot);
        }

        // Animates a single online status dot (scale pulse + opacity pulse)
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

            // Scale animation: grows and shrinks
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.55,
                Duration = TimeSpan.FromMilliseconds(1100),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            // Opacity animation: fades in and out
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

        // Breathing animation for bot avatar (gentle scale pulse)
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

            // Slow breathing scale animation
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

        // Glow effect for chat input border when focused or has text
        private void AnimateInputFocusGlow(bool shouldGlow)
        {
            if (InputOuterBorder == null)
            {
                return;
            }

            AnimateTextBarGlow(InputOuterBorder, shouldGlow);
        }

        // Glow effect for welcome name input border
        private void AnimateWelcomeNameTextBarGlow(bool shouldGlow)
        {
            Border welcomeNameBorder = FindParentBorder(WelcomeNameTextBox);

            if (welcomeNameBorder == null)
            {
                return;
            }

            AnimateTextBarGlow(welcomeNameBorder, shouldGlow);
        }

        // Core glow animation: changes border color and adds drop shadow effect
        private void AnimateTextBarGlow(Border targetBorder, bool shouldGlow)
        {
            if (targetBorder == null)
            {
                return;
            }

            // Active glow uses orange, inactive uses gray
            Color activeBorderColor = Color.FromRgb(249, 115, 22);
            Color inactiveBorderColor = Color.FromRgb(221, 221, 221);

            SolidColorBrush borderBrush = new SolidColorBrush(shouldGlow
                ? activeBorderColor
                : inactiveBorderColor);

            targetBorder.BorderBrush = borderBrush;

            // Drop shadow for glow effect
            DropShadowEffect glowEffect = new DropShadowEffect
            {
                BlurRadius = shouldGlow ? 5 : 0,
                ShadowDepth = 0,
                Opacity = shouldGlow ? 0.18 : 0,
                Color = activeBorderColor
            };

            targetBorder.Effect = glowEffect;

            // Animate blur radius
            DoubleAnimation blurAnimation = new DoubleAnimation
            {
                To = shouldGlow ? 5 : 0,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Animate shadow opacity
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = shouldGlow ? 0.18 : 0,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnimation);
            glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);
        }

        // Walks up visual tree to find the parent Border of a control
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

        // Updates glow states for all input borders
        private void UpdateAllTextBarGlowStates()
        {
            UpdateWelcomeNameGlowState();
            UpdateInputGlowState();
        }

        // Updates welcome name input glow based on text/focus state
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

        // Updates chat input glow based on text/focus state
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

        // Flash animation on sidebar session info border (orange to green)
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

        // Starts bouncing animation for all three typing indicator dots
        private void StartBouncingTypingDots()
        {
            AnimateTypingDot(TypingDot1, 0);    // No delay
            AnimateTypingDot(TypingDot2, 140);  // 140ms delay
            AnimateTypingDot(TypingDot3, 280);  // 280ms delay
        }

        // Animates a single typing dot with staggered bounce effect
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

            // Vertical bounce animation
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

            // Opacity pulse animation
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

        // Stops all typing dot animations
        private void StopBouncingTypingDots()
        {
            StopTypingDot(TypingDot1);
            StopTypingDot(TypingDot2);
            StopTypingDot(TypingDot3);
        }

        // Stops animation on a single typing dot and resets position
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

        // Handler for "Start Secure Chat" button click
        private async void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            await TryStartChatAsync();
        }

        // Handler for Enter key in welcome name textbox
        private async void WelcomeNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && StartChatButton.IsEnabled)
            {
                await TryStartChatAsync();
            }
        }

        // Updates UI state when welcome name text changes
        private void WelcomeNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
            UpdateWelcomeNameGlowState();
        }

        // Handler for welcome name textbox gaining focus
        private void WelcomeNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeNameGlowState();
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
        }

        // Handler for welcome name textbox losing focus
        private void WelcomeNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeNameGlowState();
            UpdateWelcomePlaceholderState();
            UpdateStartChatButtonState();
        }

        // Validates name and initiates chat session or shows error
        private async Task TryStartChatAsync()
        {
            string enteredName = WelcomeNameTextBox.Text.Trim();

            // Name is valid - start chat session
            if (validator.IsValidName(enteredName))
            {
                userName = enteredName;
                chatbotEngine.SetUserName(userName);

                nameAttempts = 0;
                isChatEnded = false;

                // Show success message
                WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52));
                WelcomeValidationTextBlock.Text = $"Welcome, {userName}. Preparing your secure chat.";

                // Show loading overlay with animations
                await ShowLoadingOverlayAsync("Setting up CyberBot and preparing your cybersecurity guidance.");
                await Task.Delay(900);

                // Prepare and show chat session
                PrepareChatSession();

                LoadingStatusTextBlock.Text = "Opening your secure chat.";
                await Task.Delay(500);

                await ShowChatPageWithTransitionAsync();
                await HideLoadingOverlayAsync();

                return;
            }

            // Name is invalid - increment attempt counter
            nameAttempts++;

            int attemptsLeft = 3 - nameAttempts;

            WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28));

            // Still has attempts remaining
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
            // All attempts used - lock out
            else
            {
                WelcomeValidationTextBlock.Text =
                    "You have used all your attempts for this step. Goodbye for now.";

                WelcomeNameTextBox.IsEnabled = false;
                StartChatButton.IsEnabled = false;
                UpdateWelcomeNameGlowState();
            }
        }

        // Initializes chat session with welcome messages
        private void PrepareChatSession()
        {
            ChatPanel.Children.Clear();
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;

            keepChatAtTop = true;

            UpdateSessionPanel();

            // Add initial bot messages
            AddBotMessage($"Welcome, {userName}. I am CyberBot, your cybersecurity awareness assistant.");
            AddBotMessage("You can ask me about passwords, phishing, scams, privacy, safe browsing, malware, 2FA, or what to do if something suspicious already happened.");
            AddBotMessage("For example, you can type: 'What is phishing?', 'How do I avoid scams?', 'I clicked a suspicious link', or 'Generate report'.");

            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateAllTextBarGlowStates();
        }

        // Resets all welcome page state to defaults
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

            // Reset chat input state
            UserInputTextBox.Clear();
            UserInputTextBox.IsEnabled = true;
            SendButton.IsEnabled = false;
            MenuDotsButton.IsEnabled = true;

            // Reset welcome name input
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

        // Shows/hides placeholder text for welcome name field
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

        // Enables/disables start chat button based on input and attempts
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

        // Exit button handler with confirmation dialog
        private async void ExitApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            await ExitApplicationAsync();
        }

        // Confirms exit and closes application
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

        // Send button click handler
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            await SendUserMessageAsync();
        }

        // Enter key handler for chat input
        private async void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SendButton.IsEnabled)
            {
                await SendUserMessageAsync();
            }
        }

        // Updates UI when chat input text changes
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateInputGlowState();
        }

        // Chat input got focus handler
        private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateInputGlowState();
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        // Chat input lost focus handler
        private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateInputGlowState();
            UpdatePlaceholderState();
            UpdateSendButtonState();
        }

        // Three-dot menu button click handler (shows context menu)
        private void MenuDotsButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);

            ChatOptionsContextMenu.PlacementTarget = MenuDotsButton;
            ChatOptionsContextMenu.IsOpen = true;
        }

        // Help menu item handler
        private async void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await ShowHelpMessageAsync();
        }

        // New chat menu item handler
        private void NewChatMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartNewChat();
        }

        // Logout menu item handler
        private async void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await LogoutAsync();
        }

        // Shows help message with example queries
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

        // Starts a new chat session while keeping user name
        private void StartNewChat()
        {
            ChatPanel.Children.Clear();
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;

            keepChatAtTop = true;

            // Prevent new chat if session has ended
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

        // Logs user out with confirmation and returns to welcome page
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

        // Processes user message and generates bot response
        private async Task SendUserMessageAsync()
        {
            // Block sending if chat ended or bot is already typing
            if (isChatEnded || isBotTyping)
            {
                return;
            }

            keepChatAtTop = false;

            string userInput = UserInputTextBox.Text.Trim();

            // Validate empty input
            if (validator.IsEmpty(userInput))
            {
                await ShowBotReplyAsync("Please type a message first.");
                UpdatePlaceholderState();
                UpdateSendButtonState();
                UpdateInputGlowState();
                UserInputTextBox.Focus();
                return;
            }

            // Add user message bubble
            AddUserMessage(userInput);

            UserInputTextBox.Clear();
            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateInputGlowState();

            // Check for meaningful input
            if (!validator.IsMeaningfulInput(userInput))
            {
                await ShowBotReplyAsync("I could not understand that clearly. Try asking about passwords, phishing, scams, privacy, malware, safe browsing, or 2FA.");
                UpdateSessionPanel();
                UserInputTextBox.Focus();
                return;
            }

            // Process message through chatbot engine
            string botResponse = chatbotEngine.ProcessMessage(userInput);

            await ShowBotReplyAsync(botResponse);

            UpdateSessionPanel();
            UpdatePlaceholderState();
            UpdateSendButtonState();
            UpdateInputGlowState();

            UserInputTextBox.Focus();
        }

        // Shows bot reply with typing animation and calculated delay
        private async Task ShowBotReplyAsync(string message)
        {
            isBotTyping = true;
            UpdateSendButtonState();
            UpdateInputGlowState();

            // Start typing indicator animation
            StartTypingAnimation();

            // Calculate delay based on message length
            int delay = CalculateTypingDelay(message);
            await Task.Delay(delay);

            // Stop typing and add message
            StopTypingAnimation();

            AddBotMessage(message);

            isBotTyping = false;
            UpdateSendButtonState();
            UpdateInputGlowState();
        }

        // Starts typing indicator with random message and bouncing dots
        private void StartTypingAnimation()
        {
            typingDotCount = 0;

            // Pick random typing message
            currentTypingMessage = typingMessages[random.Next(typingMessages.Length)];

            TypingIndicatorBorder.Visibility = Visibility.Visible;
            TypingIndicatorBorder.Opacity = 0;
            TypingIndicatorTextBlock.Text = currentTypingMessage;

            _ = FadeElementAsync(TypingIndicatorBorder, 0, 1, 180);

            StartBouncingTypingDots();

            typingDotsTimer.Start();
        }

        // Stops typing animation and hides indicator
        private void StopTypingAnimation()
        {
            typingDotsTimer.Stop();
            StopBouncingTypingDots();

            TypingIndicatorTextBlock.Text = "";
            TypingIndicatorBorder.Opacity = 0;
            TypingIndicatorBorder.Visibility = Visibility.Collapsed;
        }

        // Timer tick: cycles through 0-3 dots appended to typing message
        private void TypingDotsTimer_Tick(object sender, EventArgs e)
        {
            typingDotCount++;

            if (typingDotCount > 3)
            {
                typingDotCount = 0;
            }

            TypingIndicatorTextBlock.Text = currentTypingMessage + new string('.', typingDotCount);
        }

        // Calculates realistic typing delay based on message length and risk level
        private int CalculateTypingDelay(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return 1700;
            }

            // Base delay: 18ms per character
            int delay = message.Length * 18;

            // Extra delay for high-risk messages
            if (message.Contains("Risk Level: HIGH") || message.Contains("Risk Level: EMERGENCY"))
            {
                delay += 700;
            }

            // Clamp between 1700ms and 4800ms
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

        // Updates sidebar session info panel with current data
        private void UpdateSessionPanel()
        {
            SessionUserTextBlock.Text = $"User: {userName}";
            SessionLastTopicTextBlock.Text = $"Last topic: {chatbotEngine.LastTopicDisplay}";
            SessionMoodTextBlock.Text = $"Mood: {chatbotEngine.LastSentimentDisplay}";

            AnimateSidebarUpdateFlash();
        }

        // Ends chat session (disables input and send)
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

        // Shows/hides placeholder text for chat input
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

        // Enables/disables send button based on input and session state
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

        // Adds a user message bubble to chat panel
        private void AddUserMessage(string message)
        {
            AddMessageBubble(
                userName,
                message,
                DateTime.Now.ToString("HH:mm"),
                Color.FromRgb(249, 115, 22),  // Orange background
                Colors.White,                   // White text
                HorizontalAlignment.Right,      // Right aligned
                true);                          // Is user message
        }

        // Adds a bot message bubble with risk-level coloring
        private void AddBotMessage(string message)
        {
            string riskLevel = DetectRiskLevelFromMessage(message);

            Color backgroundColor = Color.FromRgb(245, 245, 245);  // Default light gray
            Color foregroundColor = Color.FromRgb(17, 17, 17);     // Dark text

            // Adjust colors based on risk level
            if (riskLevel == "Emergency")
            {
                backgroundColor = Color.FromRgb(255, 237, 213);  // Light red-orange
            }
            else if (riskLevel == "High")
            {
                backgroundColor = Color.FromRgb(255, 247, 237);  // Very light orange
            }
            else if (riskLevel == "Medium")
            {
                backgroundColor = Color.FromRgb(255, 251, 235);  // Light yellow
            }

            AddMessageBubble(
                "CyberBot",
                message,
                DateTime.Now.ToString("HH:mm"),
                backgroundColor,
                foregroundColor,
                HorizontalAlignment.Left,   // Left aligned
                false);                     // Not user message
        }

        // Core method: creates and animates a message bubble
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

            // Margins differ based on sender (user = left margin, bot = right margin)
            Thickness bubbleMargin = isUserMessage
                ? new Thickness(100, 7, 0, 7)
                : new Thickness(0, 7, 100, 7);

            string riskLevel = isUserMessage ? "" : DetectRiskLevelFromMessage(message);

            // Create message bubble border
            Border bubble = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(15, 10, 15, 9),
                Margin = bubbleMargin,
                HorizontalAlignment = alignment,
                MinWidth = 175,
                MaxWidth = maxBubbleWidth,
                Opacity = 0,  // Start invisible for animation
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                RenderTransform = new TranslateTransform(0, 12)  // Start shifted down
            };

            // User bubbles have orange border
            if (isUserMessage)
            {
                bubble.BorderThickness = new Thickness(1);
                bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(234, 88, 12));
            }
            // Risk-level bot bubbles have colored border
            else if (!string.IsNullOrWhiteSpace(riskLevel))
            {
                bubble.BorderThickness = new Thickness(2);
                bubble.BorderBrush = GetRiskBrush(riskLevel);
            }
            // Normal bot bubbles have light border
            else
            {
                bubble.BorderThickness = new Thickness(1);
                bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }

            // Add subtle shadow
            bubble.Effect = BuildCleanBubbleShadow(isUserMessage);

            // Stack panel for message content
            StackPanel messageStack = new StackPanel
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            // Add risk banner for bot messages with risk level
            if (!isUserMessage && riskLevel != "")
            {
                Border riskBanner = BuildRiskBanner(riskLevel);
                messageStack.Children.Add(riskBanner);
            }

            // Sender name text
            TextBlock senderTextBlock = new TextBlock
            {
                Text = sender,
                Foreground = new SolidColorBrush(foregroundColor),
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 3)
            };

            // Message body text
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

            // Timestamp text
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

            // Apply crisp text rendering to all text elements
            ApplyCrispTextRendering(senderTextBlock);
            ApplyCrispTextRendering(messageTextBlock);
            ApplyCrispTextRendering(timeTextBlock);

            // Assemble message stack
            messageStack.Children.Add(senderTextBlock);
            messageStack.Children.Add(messageTextBlock);
            messageStack.Children.Add(timeTextBlock);

            bubble.Child = messageStack;

            ChatPanel.Children.Add(bubble);

            // Animate bubble entrance
            AnimateMessageBubble(bubble);

            // Scroll based on current view preference
            if (keepChatAtTop)
            {
                ScrollChatToTop();
            }
            else
            {
                ScrollChatToBottom();
            }
        }

        // Creates subtle drop shadow for message bubbles
        private DropShadowEffect BuildCleanBubbleShadow(bool isUserMessage)
        {
            return new DropShadowEffect
            {
                BlurRadius = isUserMessage ? 3 : 2,
                ShadowDepth = 1,
                Direction = 270,  // Shadow falls downward
                Opacity = isUserMessage ? 0.12 : 0.08,
                Color = Colors.Black
            };
        }

        // Applies ClearType and layout rounding to text for crisp rendering
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

        // Builds colored risk banner for high-risk messages
        private Border BuildRiskBanner(string riskLevel)
        {
            string bannerText = "CYBER RISK DETECTED";

            // Customize banner text based on risk level
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

        // Returns color brush for each risk level
        private SolidColorBrush GetRiskBrush(string riskLevel)
        {
            switch (riskLevel)
            {
                case "Emergency":
                    return new SolidColorBrush(Color.FromRgb(185, 28, 28));   // Dark red

                case "High":
                    return new SolidColorBrush(Color.FromRgb(234, 88, 12));   // Orange

                case "Medium":
                    return new SolidColorBrush(Color.FromRgb(249, 115, 22));  // Light orange

                case "Low":
                    return new SolidColorBrush(Color.FromRgb(34, 197, 94));   // Green

                default:
                    return new SolidColorBrush(Color.FromRgb(249, 115, 22));  // Default orange
            }
        }

        // Parses message text to detect risk level keywords
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

        // Entrance animation for message bubbles (fade in + slide up)
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

            // Fade in
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Slide up
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

        // Button press animation (quick scale shrink and bounce back)
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

        // Hover card mouse enter: lifts card up slightly
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
                To = -2,  // Move up 2 pixels
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, liftAnimation);
        }

        // Hover card mouse leave: drops card back down
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
                To = 0,  // Return to original position
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, dropAnimation);
        }

        // Calculates responsive max width for message bubbles
        private double GetResponsiveBubbleMaxWidth()
        {
            double chatWidth = ChatScrollViewer.ActualWidth;

            if (chatWidth <= 0)
            {
                return 560;
            }

            double calculatedWidth = chatWidth * 0.68;  // 68% of available width

            // Clamp between 360 and 650
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

        // Scrolls chat to top (used for welcome messages)
        private void ScrollChatToTop()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToTop();
        }

        // Scrolls chat to bottom (used for new messages)
        private void ScrollChatToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }
    }
}