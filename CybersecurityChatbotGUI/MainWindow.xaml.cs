using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private DatabaseHelper databaseHelper;
        private TaskAssistant taskAssistant;
        private QuizManager quizManager;
        private NLPSimulator nlpSimulator;
        private ActivityLogger activityLogger;

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
            InitializePart3Services();

            Loaded += MainWindow_Loaded;

            typingDotsTimer.Interval = TimeSpan.FromMilliseconds(420);
            typingDotsTimer.Tick += TypingDotsTimer_Tick;

            WelcomeNameTextBox.GotFocus += WelcomeNameTextBox_GotFocus;
            WelcomeNameTextBox.LostFocus += WelcomeNameTextBox_LostFocus;

            TaskTitleTextBox.GotFocus += TaskTitleTextBox_GotFocus;
            TaskTitleTextBox.LostFocus += TaskTitleTextBox_LostFocus;
            TaskDescriptionTextBox.GotFocus += TaskDescriptionTextBox_GotFocus;
            TaskDescriptionTextBox.LostFocus += TaskDescriptionTextBox_LostFocus;
            TaskReminderTextBox.GotFocus += TaskReminderTextBox_GotFocus;
            TaskReminderTextBox.LostFocus += TaskReminderTextBox_LostFocus;
        }

        private void InitializePart3Services()
        {
            try
            {
                databaseHelper = new DatabaseHelper();
                activityLogger = new ActivityLogger();
                taskAssistant = new TaskAssistant(databaseHelper, activityLogger);
                quizManager = new QuizManager();
                nlpSimulator = new NLPSimulator();
                activityLogger.LogActivity("Part 3 services initialized successfully", "System");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize services: {ex.Message}\n\nPlease ensure MySQL is running and the database exists.",
                    "Service Initialization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

        // ==================== PREMIUM ANIMATIONS ====================
        private void StartLogoRotation()
        {
            if (SidebarLogoBorder == null) return;
            RotateTransform rt = new RotateTransform(0);
            SidebarLogoBorder.RenderTransform = rt;
            SidebarLogoBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation da = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(20));
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
            DropShadowEffect effect = SidebarLogoBorder.Effect as DropShadowEffect;
            if (effect != null)
            {
                DoubleAnimation glowBlur = new DoubleAnimation(8, 20, TimeSpan.FromSeconds(2));
                glowBlur.AutoReverse = true; glowBlur.RepeatBehavior = RepeatBehavior.Forever;
                effect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowBlur);
                DoubleAnimation glowOpacity = new DoubleAnimation(0.4, 0.9, TimeSpan.FromSeconds(2));
                glowOpacity.AutoReverse = true; glowOpacity.RepeatBehavior = RepeatBehavior.Forever;
                effect.BeginAnimation(DropShadowEffect.OpacityProperty, glowOpacity);
            }
        }

        private void AnimateGradientBorder(Border border, bool isFocused)
        {
            if (border == null) return;
            Color targetColor = isFocused ? Color.FromRgb(249, 115, 22) : Color.FromRgb(221, 221, 221);
            SolidColorBrush brush = border.BorderBrush as SolidColorBrush;
            if (brush == null) { brush = new SolidColorBrush(Color.FromRgb(221, 221, 221)); border.BorderBrush = brush; }
            ColorAnimation ca = new ColorAnimation(targetColor, TimeSpan.FromMilliseconds(300));
            ca.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            if (isFocused)
            {
                DropShadowEffect glow = new DropShadowEffect { Color = Color.FromRgb(249, 115, 22), BlurRadius = 0, ShadowDepth = 0, Opacity = 0 };
                border.Effect = glow;
                DoubleAnimation blurAnim = new DoubleAnimation(0, 10, TimeSpan.FromMilliseconds(300));
                blurAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
                DoubleAnimation opacityAnim = new DoubleAnimation(0, 0.4, TimeSpan.FromMilliseconds(300));
                opacityAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                glow.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnim);
            }
            else
            {
                DropShadowEffect glow = border.Effect as DropShadowEffect;
                if (glow != null)
                {
                    DoubleAnimation blurAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                    DoubleAnimation opacityAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                    opacityAnim.Completed += (s, ev) => { border.Effect = null; };
                    glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
                    glow.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnim);
                }
            }
        }

        private async void ShowConfettiBurst()
        {
            if (ConfettiCanvas == null) return;
            ConfettiCanvas.Visibility = Visibility.Visible; ConfettiCanvas.Children.Clear();
            Color[] colors = new[] { Color.FromRgb(249, 115, 22), Color.FromRgb(34, 197, 94), Color.FromRgb(59, 130, 246), Color.FromRgb(251, 191, 36), Color.FromRgb(239, 68, 68), Color.FromRgb(168, 85, 247) };
            Random rand = new Random();
            double w = this.ActualWidth > 0 ? this.ActualWidth : 1000;
            double h = this.ActualHeight > 0 ? this.ActualHeight : 700;
            for (int i = 0; i < 50; i++)
            {
                Ellipse particle = new Ellipse { Width = rand.Next(8, 16), Height = rand.Next(8, 16), Fill = new SolidColorBrush(colors[rand.Next(colors.Length)]) };
                Canvas.SetLeft(particle, rand.Next(0, (int)w)); Canvas.SetTop(particle, rand.Next(-100, -20));
                ConfettiCanvas.Children.Add(particle);
                TransformGroup tg = new TransformGroup(); TranslateTransform tt = new TranslateTransform(0, 0); RotateTransform rt2 = new RotateTransform(0);
                tg.Children.Add(rt2); tg.Children.Add(tt); particle.RenderTransform = tg; particle.RenderTransformOrigin = new Point(0.5, 0.5);
                DoubleAnimation fall = new DoubleAnimation(Canvas.GetTop(particle), h + 100, TimeSpan.FromMilliseconds(rand.Next(2500, 5000))) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
                DoubleAnimation sway = new DoubleAnimation(0, rand.Next(-120, 120), TimeSpan.FromMilliseconds(rand.Next(1500, 3000))) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(2), EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } };
                DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1500)) { BeginTime = TimeSpan.FromMilliseconds(rand.Next(2000, 3500)) };
                DoubleAnimation spin = new DoubleAnimation(0, rand.Next(360, 1080), TimeSpan.FromMilliseconds(rand.Next(2000, 4000)));
                tt.BeginAnimation(TranslateTransform.YProperty, fall); tt.BeginAnimation(TranslateTransform.XProperty, sway);
                particle.BeginAnimation(UIElement.OpacityProperty, fade); rt2.BeginAnimation(RotateTransform.AngleProperty, spin);
            }
            await Task.Delay(6000);
            ConfettiCanvas.Children.Clear(); ConfettiCanvas.Visibility = Visibility.Collapsed;
        }

        private async Task SlideTabTransitionAsync(Grid fromTab, Grid toTab, bool slideRight)
        {
            if (fromTab == null || toTab == null) return;
            fromTab.RenderTransform = new TranslateTransform(0, 0); toTab.RenderTransform = new TranslateTransform(slideRight ? 400 : -400, 0);
            toTab.Visibility = Visibility.Visible; toTab.Opacity = 0;
            TranslateTransform tf = fromTab.RenderTransform as TranslateTransform; TranslateTransform tt2 = toTab.RenderTransform as TranslateTransform;
            DoubleAnimation outSlide = new DoubleAnimation(0, slideRight ? -400 : 400, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            DoubleAnimation outFade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            DoubleAnimation inSlide = new DoubleAnimation(slideRight ? 400 : -400, 0, TimeSpan.FromMilliseconds(350)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 } };
            DoubleAnimation inFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            outSlide.Completed += (s, ev) => { fromTab.Visibility = Visibility.Collapsed; fromTab.Opacity = 1; tf.X = 0; };
            tf.BeginAnimation(TranslateTransform.XProperty, outSlide); fromTab.BeginAnimation(UIElement.OpacityProperty, outFade);
            tt2.BeginAnimation(TranslateTransform.XProperty, inSlide); toTab.BeginAnimation(UIElement.OpacityProperty, inFade);
            await Task.Delay(400);
        }

        private void PlaySuccessPulse(Border element)
        {
            if (element == null) return;
            ScaleTransform st = new ScaleTransform(1, 1); element.RenderTransform = st; element.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation pulse = new DoubleAnimation(1, 1.04, TimeSpan.FromMilliseconds(150)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, pulse); st.BeginAnimation(ScaleTransform.ScaleYProperty, pulse);
            DropShadowEffect greenGlow = new DropShadowEffect { Color = Color.FromRgb(34, 197, 94), BlurRadius = 0, ShadowDepth = 0, Opacity = 0 };
            element.Effect = greenGlow;
            DoubleAnimation blurUp = new DoubleAnimation(0, 15, TimeSpan.FromMilliseconds(400)) { AutoReverse = true };
            DoubleAnimation opacityUp = new DoubleAnimation(0, 0.9, TimeSpan.FromMilliseconds(400)) { AutoReverse = true };
            opacityUp.Completed += (s, ev) => { element.Effect = null; };
            greenGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurUp);
            greenGlow.BeginAnimation(DropShadowEffect.OpacityProperty, opacityUp);
        }

        // ==================== TASK TAB FOCUS EFFECTS ====================
        private void TaskTitleTextBox_GotFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskTitleTextBox); if (pb != null) AnimateGradientBorder(pb, true); }
        private void TaskTitleTextBox_LostFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskTitleTextBox); if (pb != null) AnimateGradientBorder(pb, false); }
        private void TaskDescriptionTextBox_GotFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskDescriptionTextBox); if (pb != null) AnimateGradientBorder(pb, true); }
        private void TaskDescriptionTextBox_LostFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskDescriptionTextBox); if (pb != null) AnimateGradientBorder(pb, false); }
        private void TaskReminderTextBox_GotFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskReminderTextBox); if (pb != null) AnimateGradientBorder(pb, true); }
        private void TaskReminderTextBox_LostFocus(object sender, RoutedEventArgs e) { Border pb = FindParentBorder(TaskReminderTextBox); if (pb != null) AnimateGradientBorder(pb, false); }

        // ==================== PAGE NAVIGATION ====================
        private void ShowWelcomePage() { WelcomePage.Visibility = Visibility.Visible; WelcomePage.Opacity = 1; ChatPage.Visibility = Visibility.Collapsed; ChatPage.Opacity = 0; }
        private async Task ShowWelcomePageWithTransitionAsync() { WelcomePage.Visibility = Visibility.Visible; WelcomePage.Opacity = 0; await FadeSlideElementAsync(ChatPage, 1, 0, 0, 18, 280); ChatPage.Visibility = Visibility.Collapsed; await FadeSlideElementAsync(WelcomePage, 0, 1, 18, 0, 360); UpdateAllTextBarGlowStates(); WelcomeNameTextBox.Focus(); }
        private async Task ShowChatPageWithTransitionAsync() { ChatPage.Visibility = Visibility.Visible; ChatPage.Opacity = 0; await FadeSlideElementAsync(WelcomePage, 1, 0, 0, -18, 280); WelcomePage.Visibility = Visibility.Collapsed; await FadeSlideElementAsync(ChatPage, 0, 1, 20, 0, 360); UpdateAllTextBarGlowStates(); UserInputTextBox.Focus(); ScrollChatToTop(); }
        private async Task ShowLoadingOverlayAsync(string msg) { LoadingStatusTextBlock.Text = msg; LoadingOverlay.Visibility = Visibility.Visible; if (LoadingCardBorder != null) LoadingCardBorder.Opacity = 0; await FadeElementAsync(LoadingOverlay, 0, 1, 180); AnimateLoadingCardIn(); StartLoadingProgressAnimation(); StartLoadingIconPulse(); }
        private async Task HideLoadingOverlayAsync() { StopLoadingProgressAnimation(); if (LoadingCardBorder != null) { AnimateLoadingCardOut(); await Task.Delay(180); } await FadeElementAsync(LoadingOverlay, 1, 0, 200); LoadingOverlay.Visibility = Visibility.Collapsed; }

        // ==================== TAB NAVIGATION ====================
        private async void ShowTab(Grid tabToShow)
        {
            if (tabToShow == null) return;
            Grid currentTab = null;
            if (ChatTab.Visibility == Visibility.Visible) currentTab = ChatTab;
            else if (TaskTab.Visibility == Visibility.Visible) currentTab = TaskTab;
            else if (QuizTab.Visibility == Visibility.Visible) currentTab = QuizTab;
            else if (ActivityLogTab.Visibility == Visibility.Visible) currentTab = ActivityLogTab;
            if (currentTab == tabToShow) return;
            bool slideRight = false;
            if (currentTab == ChatTab && (tabToShow == TaskTab || tabToShow == QuizTab || tabToShow == ActivityLogTab)) slideRight = true;
            if (currentTab == TaskTab && (tabToShow == QuizTab || tabToShow == ActivityLogTab)) slideRight = true;
            if (currentTab == QuizTab && tabToShow == ActivityLogTab) slideRight = true;
            if (currentTab != null) await SlideTabTransitionAsync(currentTab, tabToShow, slideRight);
            else { ChatTab.Visibility = Visibility.Collapsed; TaskTab.Visibility = Visibility.Collapsed; QuizTab.Visibility = Visibility.Collapsed; ActivityLogTab.Visibility = Visibility.Collapsed; tabToShow.Visibility = Visibility.Visible; }
            UpdateNavButtonStyles(tabToShow);
        }

        private void UpdateNavButtonStyles(Grid activeTab) { Color a = Color.FromRgb(249, 115, 22); Color i = Color.FromRgb(229, 229, 229); Color it = Color.FromRgb(17, 17, 17); SetNavButtonStyle(NavChatButton, activeTab == ChatTab, a, i, it); SetNavButtonStyle(NavTasksButton, activeTab == TaskTab, a, i, it); SetNavButtonStyle(NavQuizButton, activeTab == QuizTab, a, i, it); SetNavButtonStyle(NavLogButton, activeTab == ActivityLogTab, a, i, it); }
        private void SetNavButtonStyle(Button btn, bool isActive, Color a, Color i, Color it) { btn.Background = new SolidColorBrush(isActive ? a : i); btn.Foreground = isActive ? new SolidColorBrush(Colors.White) : new SolidColorBrush(it); }
        private void NavChatButton_Click(object sender, RoutedEventArgs e) { ShowTab(ChatTab); activityLogger?.LogActivity("Switched to Chat tab", "System"); }
        private void NavTasksButton_Click(object sender, RoutedEventArgs e) { ShowTab(TaskTab); RefreshTaskList(); activityLogger?.LogActivity("Switched to Tasks tab", "System"); }
        private void NavQuizButton_Click(object sender, RoutedEventArgs e) { ShowTab(QuizTab); activityLogger?.LogActivity("Switched to Quiz tab", "System"); }
        private void NavLogButton_Click(object sender, RoutedEventArgs e) { ShowTab(ActivityLogTab); RefreshActivityLog(); activityLogger?.LogActivity("Switched to Activity Log tab", "System"); }
        private void TasksButton_Click(object sender, RoutedEventArgs e) { ShowTab(TaskTab); RefreshTaskList(); activityLogger?.LogActivity("Opened Task Assistant", "System"); }
        private void QuizButton_Click(object sender, RoutedEventArgs e) { ShowTab(QuizTab); activityLogger?.LogActivity("Opened Quiz", "System"); }
        private void ActivityLogButton_Click(object sender, RoutedEventArgs e) { ShowTab(ActivityLogTab); RefreshActivityLog(); activityLogger?.LogActivity("Opened Activity Log", "System"); }
        private void BackToChatButton_Click(object sender, RoutedEventArgs e) { ShowTab(ChatTab); UserInputTextBox.Focus(); activityLogger?.LogActivity("Returned to Chat", "System"); }

        // ==================== TASK ASSISTANT ====================
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            string reminderText = TaskReminderTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title)) { ShowTaskFeedback("Please enter a task title.", false); return; }
            DateTime? reminderDate = null;
            if (!string.IsNullOrWhiteSpace(reminderText)) reminderDate = taskAssistant.ParseReminderDate("remind me " + reminderText);
            string result = taskAssistant.AddTask(title, description, reminderDate);
            bool isSuccess = result.Contains("successfully");
            ShowTaskFeedback(result, isSuccess);
            if (isSuccess) { TaskTitleTextBox.Clear(); TaskDescriptionTextBox.Clear(); TaskReminderTextBox.Clear(); }
            RefreshTaskList();
        }

        private async void ShowTaskFeedback(string message, bool isSuccess)
        {
            StackPanel parentStack = null;
            DependencyObject current = AddTaskButton.Parent;
            while (current != null) { if (current is StackPanel sp) { parentStack = sp; break; } current = VisualTreeHelper.GetParent(current); }
            if (parentStack == null) return;
            List<UIElement> toRemove = new List<UIElement>();
            foreach (UIElement child in parentStack.Children) { if (child is Border b && b.Tag != null && b.Tag.ToString() == "TaskFeedback") toRemove.Add(child); }
            foreach (var item in toRemove) parentStack.Children.Remove(item);

            Border feedbackBubble = new Border
            {
                Background = isSuccess ? new SolidColorBrush(Color.FromRgb(240, 253, 244)) : new SolidColorBrush(Color.FromRgb(254, 242, 242)),
                BorderBrush = isSuccess ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 8, 0, 0),
                Opacity = 0,
                Tag = "TaskFeedback"
            };

            StackPanel feedbackStack = new StackPanel();
            StackPanel headerStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            TextBlock titleText = new TextBlock { Text = isSuccess ? "Success!" : "Error", Foreground = isSuccess ? new SolidColorBrush(Color.FromRgb(22, 101, 52)) : new SolidColorBrush(Color.FromRgb(185, 28, 28)), FontSize = 16, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
            headerStack.Children.Add(titleText);
            TextBlock messageText = new TextBlock { Text = message, Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)), FontSize = 13, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, LineHeight = 20 };
            feedbackStack.Children.Add(headerStack); feedbackStack.Children.Add(messageText); feedbackBubble.Child = feedbackStack;
            parentStack.Children.Add(feedbackBubble);

            feedbackBubble.RenderTransform = new TranslateTransform(0, 10);
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation slideUp = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            feedbackBubble.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            (feedbackBubble.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, slideUp);

            await Task.Delay(5000);

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            fadeOut.Completed += (s, ev) => { if (parentStack.Children.Contains(feedbackBubble)) parentStack.Children.Remove(feedbackBubble); };
            feedbackBubble.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void RefreshTasksButton_Click(object sender, RoutedEventArgs e) { RefreshTaskList(); }

        private void RefreshTaskList()
        {
            TaskListPanel.Children.Clear();
            List<TaskItem> tasks;
            try { tasks = databaseHelper.GetAllTasks(); }
            catch { TaskListPanel.Children.Add(new TextBlock { Text = "Unable to load tasks. Check database connection.", Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28)), FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 10, 0, 0) }); return; }
            if (tasks.Count == 0) { TaskListPanel.Children.Add(new TextBlock { Text = "No tasks yet.", Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)), FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 10, 0, 0) }); return; }

            int taskNumber = 1;
            foreach (var task in tasks)
            {
                TaskListPanel.Children.Add(CreateTaskCard(task, taskNumber));
                taskNumber++;
            }
        }

        private ControlTemplate CreateRoundedButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "ButtonBorder";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            border.SetValue(Border.PaddingProperty, new Thickness(0));
            Binding bgBinding = new Binding("Background");
            bgBinding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            border.SetBinding(Border.BackgroundProperty, bgBinding);
            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            return template;
        }

        private Border CreateTaskCard(TaskItem task, int displayNumber = 0)
        {
            Border card = new Border
            {
                Background = task.IsCompleted ? new SolidColorBrush(Color.FromRgb(240, 253, 244)) : new SolidColorBrush(Colors.White),
                BorderBrush = task.IsCompleted ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 8)
            };

            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel infoStack = new StackPanel();
            infoStack.SetValue(Grid.ColumnProperty, 0);

            TextBlock titleBlock = new TextBlock
            {
                Text = task.IsCompleted ? $"Task {displayNumber}: {task.Title} (Done)" : $"Task {displayNumber}: {task.Title}",
                Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 17)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            infoStack.Children.Add(titleBlock);

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                TextBlock descBlock = new TextBlock
                {
                    Text = task.Description,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                infoStack.Children.Add(descBlock);
            }

            if (task.ReminderDate.HasValue)
            {
                TextBlock reminderBlock = new TextBlock
                {
                    Text = $"Reminder: {task.ReminderDate.Value:yyyy-MM-dd HH:mm}",
                    Foreground = new SolidColorBrush(Color.FromRgb(249, 115, 22)),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };
                infoStack.Children.Add(reminderBlock);
            }

            cardGrid.Children.Add(infoStack);

            if (!task.IsCompleted)
            {
                Button completeBtn = new Button
                {
                    Width = 44,
                    Height = 28,
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(8, 0, 4, 0),
                    ToolTip = "Mark as complete"
                };

                Style completeStyle = new Style(typeof(Button));
                completeStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(34, 197, 94))));
                completeStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
                completeStyle.Setters.Add(new Setter(Button.TemplateProperty, CreateRoundedButtonTemplate()));

                Trigger hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(21, 128, 61))));
                completeStyle.Triggers.Add(hoverTrigger);

                Trigger pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
                pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(20, 83, 45))));
                completeStyle.Triggers.Add(pressedTrigger);

                completeBtn.Style = completeStyle;

                completeBtn.Content = new Viewbox
                {
                    Width = 14,
                    Height = 14,
                    Child = new System.Windows.Shapes.Path
                    {
                        Fill = new SolidColorBrush(Colors.White),
                        Stretch = Stretch.Uniform,
                        Data = Geometry.Parse("M9,16.17L4.83,12L3.41,13.41L9,19L21,7L19.59,5.59L9,16.17Z")
                    }
                };

                completeBtn.SetValue(Grid.ColumnProperty, 1);
                int completeTaskId = task.TaskID;
                completeBtn.Click += (s, ev) =>
                {
                    taskAssistant.CompleteTask(completeTaskId);
                    PlaySuccessPulse(card);
                    RefreshTaskList();
                };
                cardGrid.Children.Add(completeBtn);
            }

            Button deleteBtn = new Button
            {
                Width = 44,
                Height = 28,
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand,
                Margin = new Thickness(4, 0, 0, 0),
                ToolTip = "Delete task"
            };

            Style deleteStyle = new Style(typeof(Button));
            deleteStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(239, 68, 68))));
            deleteStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            deleteStyle.Setters.Add(new Setter(Button.TemplateProperty, CreateRoundedButtonTemplate()));

            Trigger deleteHover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            deleteHover.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(185, 28, 28))));
            deleteStyle.Triggers.Add(deleteHover);

            Trigger deletePressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            deletePressed.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(127, 29, 29))));
            deleteStyle.Triggers.Add(deletePressed);

            deleteBtn.Style = deleteStyle;

            deleteBtn.Content = new Viewbox
            {
                Width = 14,
                Height = 14,
                Child = new System.Windows.Shapes.Path
                {
                    Fill = new SolidColorBrush(Colors.White),
                    Stretch = Stretch.Uniform,
                    Data = Geometry.Parse("M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z")
                }
            };

            deleteBtn.SetValue(Grid.ColumnProperty, 2);
            int deleteTaskId = task.TaskID;
            deleteBtn.Click += (s, ev) =>
            {
                MessageBoxResult confirm = MessageBox.Show(
                    $"Delete task '{task.Title}'?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    taskAssistant.DeleteTask(deleteTaskId);
                    RefreshTaskList();
                }
            };
            cardGrid.Children.Add(deleteBtn);

            card.Child = cardGrid;
            return card;
        }

        // ==================== QUIZ ====================
        private void StartQuizButton_Click(object sender, RoutedEventArgs e) { quizManager.StartNewQuiz(); QuizStartPanel.Visibility = Visibility.Collapsed; QuizResultsPanel.Visibility = Visibility.Collapsed; QuizActivePanel.Visibility = Visibility.Visible; activityLogger.LogActivity("Quiz started", "Quiz"); DisplayCurrentQuestion(); }

        private void DisplayCurrentQuestion()
        {
            QuizQuestion question = quizManager.GetCurrentQuestion();
            if (question == null) { ShowQuizResults(); return; }
            QuizProgressTextBlock.Text = $"Question {quizManager.CurrentQuestionNumber} of {quizManager.TotalQuestions}";
            QuizQuestionTextBlock.Text = question.Question;
            QuizOptionsPanel.Children.Clear(); QuizFeedbackTextBlock.Visibility = Visibility.Collapsed; QuizNextButton.Visibility = Visibility.Collapsed;
            for (int i = 0; i < question.Options.Count; i++) { int ai = i; Border ob = CreateQuizOptionBorder(question, i, ai); QuizOptionsPanel.Children.Add(ob); }
        }

        private Border CreateQuizOptionBorder(QuizQuestion question, int i, int answerIndex)
        {
            Border optionBorder = new Border { Background = new SolidColorBrush(Colors.White), BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(14), Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand, Tag = i };
            Grid optionGrid = new Grid(); optionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); optionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Border letterBadge = new Border { Width = 36, Height = 36, Background = new SolidColorBrush(Color.FromRgb(249, 115, 22)), CornerRadius = new CornerRadius(10), Margin = new Thickness(8, 8, 12, 8), VerticalAlignment = VerticalAlignment.Center };
            letterBadge.Child = new TextBlock { Text = ((char)('A' + i)).ToString(), Foreground = new SolidColorBrush(Colors.White), FontSize = 15, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            letterBadge.SetValue(Grid.ColumnProperty, 0);
            TextBlock optionText = new TextBlock { Text = question.Options[i], Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 17)), FontSize = 14, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(0, 10, 14, 10) };
            optionText.SetValue(Grid.ColumnProperty, 1);
            optionGrid.Children.Add(letterBadge); optionGrid.Children.Add(optionText); optionBorder.Child = optionGrid;
            optionBorder.MouseLeftButtonDown += (s, ev) => ProcessQuizAnswer(answerIndex);
            optionBorder.MouseEnter += (s, ev) => { Border b = s as Border; if (b != null && b.IsEnabled) { b.Background = new SolidColorBrush(Color.FromRgb(255, 247, 237)); b.BorderBrush = new SolidColorBrush(Color.FromRgb(249, 115, 22)); } };
            optionBorder.MouseLeave += (s, ev) => { Border b = s as Border; if (b != null && b.IsEnabled) { b.Background = new SolidColorBrush(Colors.White); b.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)); } };
            return optionBorder;
        }

        private void ProcessQuizAnswer(int selectedIndex)
        {
            foreach (Border ob in QuizOptionsPanel.Children)
            {
                ob.IsEnabled = false; ob.Cursor = Cursors.Arrow; int ti = (int)ob.Tag; bool isCorrectOption = ti == quizManager.GetCurrentQuestion().CorrectAnswerIndex; bool isSelected = ti == selectedIndex; Grid og = ob.Child as Grid; Border lb = og?.Children[0] as Border;
                if (isCorrectOption) { ob.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244)); ob.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 197, 94)); if (lb != null) { lb.Background = new SolidColorBrush(Color.FromRgb(34, 197, 94)); ScaleTransform st2 = new ScaleTransform(1, 1); lb.RenderTransform = st2; lb.RenderTransformOrigin = new Point(0.5, 0.5); DoubleAnimation p2 = new DoubleAnimation(1, 1.3, TimeSpan.FromMilliseconds(300)) { AutoReverse = true, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } }; st2.BeginAnimation(ScaleTransform.ScaleXProperty, p2); st2.BeginAnimation(ScaleTransform.ScaleYProperty, p2); } }
                else if (isSelected && !isCorrectOption) { ob.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242)); ob.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68)); if (lb != null) { lb.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)); TranslateTransform tt3 = new TranslateTransform(0, 0); lb.RenderTransform = tt3; DoubleAnimation sh = new DoubleAnimation(-5, 5, TimeSpan.FromMilliseconds(50)) { RepeatBehavior = new RepeatBehavior(4), AutoReverse = true }; tt3.BeginAnimation(TranslateTransform.XProperty, sh); } }
                else ob.Opacity = 0.5;
            }
            bool isCorrect = quizManager.SubmitAnswer(selectedIndex);
            Border feedbackBubble = new Border { Background = isCorrect ? new SolidColorBrush(Color.FromRgb(240, 253, 244)) : new SolidColorBrush(Color.FromRgb(254, 242, 242)), BorderBrush = isCorrect ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(239, 68, 68)), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(16), Padding = new Thickness(16, 12, 16, 12), Margin = new Thickness(0, 8, 0, 8), Opacity = 0 };
            StackPanel fs = new StackPanel();
            StackPanel hs = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            TextBlock titleText = new TextBlock { Text = isCorrect ? "Correct!" : "Incorrect", Foreground = isCorrect ? new SolidColorBrush(Color.FromRgb(22, 101, 52)) : new SolidColorBrush(Color.FromRgb(185, 28, 28)), FontSize = 16, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
            hs.Children.Add(titleText);
            TextBlock explanationText = new TextBlock { Text = isCorrect ? quizManager.GetCurrentExplanation() : $"The correct answer is: {quizManager.GetCorrectAnswerText()}. {quizManager.GetCurrentExplanation()}", Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)), FontSize = 13, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, LineHeight = 20 };
            fs.Children.Add(hs); fs.Children.Add(explanationText); feedbackBubble.Child = fs;
            QuizActivePanel.Children.Insert(QuizActivePanel.Children.IndexOf(QuizNextButton), feedbackBubble);
            feedbackBubble.RenderTransform = new TranslateTransform(0, 10);
            DoubleAnimation bfi = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation bsu = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            feedbackBubble.BeginAnimation(UIElement.OpacityProperty, bfi); (feedbackBubble.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, bsu);
            QuizNextButton.Visibility = Visibility.Visible;
            activityLogger.LogActivity($"Quiz question {quizManager.CurrentQuestionNumber} answered {(isCorrect ? "correct" : "incorrect")}", "Quiz");
        }

        private void QuizNextButton_Click(object sender, RoutedEventArgs e) { List<UIElement> toRemove = new List<UIElement>(); foreach (UIElement child in QuizActivePanel.Children) { if (child is Border bc && bc.Tag == null) toRemove.Add(child); } foreach (var item in toRemove) QuizActivePanel.Children.Remove(item); if (quizManager.MoveToNextQuestion()) DisplayCurrentQuestion(); else ShowQuizResults(); }

        private void ShowQuizResults() { QuizActivePanel.Visibility = Visibility.Collapsed; QuizStartPanel.Visibility = Visibility.Collapsed; QuizResultsPanel.Visibility = Visibility.Visible; int score = quizManager.GetScore(); int total = quizManager.GetTotalQuestions(); QuizResultsTitleTextBlock.Text = "Quiz Complete!"; QuizResultsScoreTextBlock.Text = $"{score} / {total}"; QuizResultsFeedbackTextBlock.Text = quizManager.GetFinalFeedback(); activityLogger.LogActivity($"Quiz completed - {score}/{total}", "Quiz"); if (score == total) Dispatcher.Invoke(() => ShowConfettiBurst()); }

        // ==================== ACTIVITY LOG ====================
        private void RefreshLogButton_Click(object sender, RoutedEventArgs e) { RefreshActivityLog(); }
        private void RefreshActivityLog() { ActivityLogPanel.Children.Clear(); var entries = activityLogger.GetLastEntries(10); ActivityCountTextBlock.Text = $"{activityLogger.GetCount()} actions tracked"; if (entries.Count == 0) { ActivityLogPanel.Children.Add(new TextBlock { Text = "No activities recorded yet.", Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)), FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 10, 0, 0) }); return; } int index = 1; foreach (var entry in entries) { Border ec = new Border { Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)), BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 6) }; StackPanel es = new StackPanel(); TextBlock hb = new TextBlock { Text = $"{index}. [{entry.Timestamp:HH:mm:ss}] {entry.ActionType}", Foreground = new SolidColorBrush(Color.FromRgb(249, 115, 22)), FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 2) }; TextBlock db = new TextBlock { Text = entry.Description, Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 17)), FontSize = 13, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap }; es.Children.Add(hb); es.Children.Add(db); ec.Child = es; ActivityLogPanel.Children.Add(ec); index++; } }

        // ==================== CHAT ====================
        private async Task SendUserMessageAsync() { if (isChatEnded || isBotTyping) return; keepChatAtTop = false; string ui = UserInputTextBox.Text.Trim(); if (validator.IsEmpty(ui)) { await ShowBotReplyAsync("Please type a message first."); UpdatePlaceholderState(); UpdateSendButtonState(); UpdateInputGlowState(); UserInputTextBox.Focus(); return; } AddUserMessage(ui); UserInputTextBox.Clear(); UpdatePlaceholderState(); UpdateSendButtonState(); UpdateInputGlowState(); NLPResult nlr = nlpSimulator.ProcessInput(ui); if (nlr.IsCommand) { activityLogger.LogActivity($"NLP: '{nlr.DetectedIntent}'", "NLP"); string cr = HandlePart3Command(nlr, ui); if (!string.IsNullOrWhiteSpace(cr)) { await ShowBotReplyAsync(cr); UpdateSessionPanel(); UserInputTextBox.Focus(); return; } } if (!validator.IsMeaningfulInput(ui)) { await ShowBotReplyAsync("I could not understand. Try passwords, phishing, scams, privacy, malware, safe browsing, or 2FA."); UpdateSessionPanel(); UserInputTextBox.Focus(); return; } string br = chatbotEngine.ProcessMessage(ui); await ShowBotReplyAsync(br); UpdateSessionPanel(); UpdatePlaceholderState(); UpdateSendButtonState(); UpdateInputGlowState(); UserInputTextBox.Focus(); }

        private string HandlePart3Command(NLPResult nlr, string ui) { switch (nlr.DetectedIntent) { case "add_task": return HandleAddTaskCommand(ui); case "view_tasks": return taskAssistant.ViewAllTasks(); case "complete_task": int cid = ExtractTaskId(ui); return cid > 0 ? taskAssistant.CompleteTask(cid) : "Which task?"; case "delete_task": int did = ExtractTaskId(ui); return did > 0 ? taskAssistant.DeleteTask(did) : "Which task?"; case "start_quiz": Dispatcher.Invoke(() => { ShowTab(QuizTab); StartQuizButton_Click(null, null); }); return "Opening quiz."; case "show_activity": Dispatcher.Invoke(() => { ShowTab(ActivityLogTab); RefreshActivityLog(); }); return activityLogger.GetFormattedLog(10); case "help": return nlpSimulator.GetHelpMenu(); default: return null; } }

        private string HandleAddTaskCommand(string ui) { string t = ui; foreach (string p in new[] { "add task to ", "add task ", "create task ", "new task " }) if (t.ToLower().StartsWith(p)) { t = t.Substring(p.Length); break; } t = t.Trim(); if (string.IsNullOrWhiteSpace(t)) return "Specify task. Example: Add task to enable 2FA."; DateTime? rd = taskAssistant.ParseReminderDate(ui); string r = taskAssistant.AddTask(t, "", rd); Dispatcher.Invoke(() => { if (TaskTab.Visibility == Visibility.Visible) RefreshTaskList(); }); return r; }
        private int ExtractTaskId(string ui) { foreach (string p in ui.Split(' ')) if (int.TryParse(p, out int id)) return id; var m = System.Text.RegularExpressions.Regex.Match(ui, @"task\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase); return m.Success ? int.Parse(m.Groups[1].Value) : -1; }

        // ==================== WELCOME / START CHAT ====================
        private async void StartChatButton_Click(object sender, RoutedEventArgs e) { AnimateButtonPress(sender as Button); await TryStartChatAsync(); }
        private async void WelcomeNameTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter && StartChatButton.IsEnabled) await TryStartChatAsync(); }
        private void WelcomeNameTextBox_TextChanged(object sender, TextChangedEventArgs e) { UpdateWelcomePlaceholderState(); UpdateStartChatButtonState(); UpdateWelcomeNameGlowState(); }
        private void WelcomeNameTextBox_GotFocus(object sender, RoutedEventArgs e) { UpdateWelcomeNameGlowState(); UpdateWelcomePlaceholderState(); UpdateStartChatButtonState(); Border wb = FindParentBorder(WelcomeNameTextBox); if (wb != null) AnimateGradientBorder(wb, true); }
        private void WelcomeNameTextBox_LostFocus(object sender, RoutedEventArgs e) { UpdateWelcomeNameGlowState(); UpdateWelcomePlaceholderState(); UpdateStartChatButtonState(); Border wb = FindParentBorder(WelcomeNameTextBox); if (wb != null) AnimateGradientBorder(wb, false); }

        private async Task TryStartChatAsync() { string en = WelcomeNameTextBox.Text.Trim(); if (validator.IsValidName(en)) { userName = en; chatbotEngine.SetUserName(userName); nameAttempts = 0; isChatEnded = false; WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52)); WelcomeValidationTextBlock.Text = $"Welcome, {userName}."; await ShowLoadingOverlayAsync("Setting up."); await Task.Delay(900); PrepareChatSession(); LoadingStatusTextBlock.Text = "Opening."; await Task.Delay(500); await ShowChatPageWithTransitionAsync(); await HideLoadingOverlayAsync(); activityLogger.LogActivity($"User '{userName}' logged in", "System"); return; } nameAttempts++; int al = 3 - nameAttempts; WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28)); if (nameAttempts < 3) { WelcomeValidationTextBlock.Text = $"Invalid. Attempts left: {al}."; WelcomeNameTextBox.Clear(); UpdateWelcomePlaceholderState(); UpdateStartChatButtonState(); UpdateWelcomeNameGlowState(); WelcomeNameTextBox.Focus(); } else { WelcomeValidationTextBlock.Text = "All attempts used."; WelcomeNameTextBox.IsEnabled = false; StartChatButton.IsEnabled = false; } }
        private void PrepareChatSession() { ChatPanel.Children.Clear(); TypingIndicatorBorder.Visibility = Visibility.Collapsed; keepChatAtTop = true; UpdateSessionPanel(); ShowTab(ChatTab); Dispatcher.BeginInvoke(new Action(() => StartLogoRotation()), DispatcherPriority.Loaded); AddBotMessage($"Welcome, {userName}."); AddBotMessage("Ask me about passwords, phishing, scams, privacy, safe browsing, malware, or 2FA."); AddBotMessage("Try: Add task or Start quiz."); UpdatePlaceholderState(); UpdateSendButtonState(); UpdateAllTextBarGlowStates(); }
        private void ResetWelcomePage() { nameAttempts = 0; userName = "User"; isChatEnded = false; isBotTyping = false; typingDotsTimer.Stop(); chatbotEngine.ResetConversationButKeepUser(); chatbotEngine.SetUserName(userName); ChatPanel.Children.Clear(); TypingIndicatorBorder.Visibility = Visibility.Collapsed; UserInputTextBox.Clear(); UserInputTextBox.IsEnabled = true; SendButton.IsEnabled = false; MenuDotsButton.IsEnabled = true; WelcomeNameTextBox.Clear(); WelcomeNameTextBox.IsEnabled = true; StartChatButton.IsEnabled = false; WelcomeValidationTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); WelcomeValidationTextBlock.Text = "Name must be at least 3 characters."; UpdateWelcomePlaceholderState(); UpdateStartChatButtonState(); UpdatePlaceholderState(); UpdateAllTextBarGlowStates(); }
        private void UpdateWelcomePlaceholderState() { if (WelcomeNamePlaceholderTextBlock != null && WelcomeNameTextBox != null) WelcomeNamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed; }
        private void UpdateStartChatButtonState() { if (StartChatButton != null && WelcomeNameTextBox != null) StartChatButton.IsEnabled = !string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text) && nameAttempts < 3; }

        // ==================== EXIT ====================
        private async void ExitApplicationButton_Click(object sender, RoutedEventArgs e) { AnimateButtonPress(sender as Button); await ExitApplicationAsync(); }
        private async Task ExitApplicationAsync() { if (!CyberDialog.ShowConfirmation(this, "Confirm Exit", "Exit?")) { WelcomeNameTextBox.Focus(); return; } CyberDialog.ShowMessage(this, "Signing Off", "Stay safe!"); await Task.Delay(150); Application.Current.Shutdown(); }

        // ==================== SEND ====================
        private async void SendButton_Click(object sender, RoutedEventArgs e) { AnimateButtonPress(sender as Button); await SendUserMessageAsync(); }
        private async void UserInputTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter && SendButton.IsEnabled) await SendUserMessageAsync(); }
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e) { UpdatePlaceholderState(); UpdateSendButtonState(); UpdateInputGlowState(); }
        private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e) { UpdateInputGlowState(); UpdatePlaceholderState(); UpdateSendButtonState(); AnimateGradientBorder(InputOuterBorder, true); }
        private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e) { UpdateInputGlowState(); UpdatePlaceholderState(); UpdateSendButtonState(); AnimateGradientBorder(InputOuterBorder, false); }

        // ==================== MENU ====================
        private void MenuDotsButton_Click(object sender, RoutedEventArgs e) { AnimateButtonPress(sender as Button); ChatOptionsContextMenu.PlacementTarget = MenuDotsButton; ChatOptionsContextMenu.IsOpen = true; }
        private async void HelpMenuItem_Click(object sender, RoutedEventArgs e) { await ShowHelpMessageAsync(); }
        private void NewChatMenuItem_Click(object sender, RoutedEventArgs e) { StartNewChat(); }
        private async void LogoutMenuItem_Click(object sender, RoutedEventArgs e) { await LogoutAsync(); }
        private async Task ShowHelpMessageAsync() { if (isChatEnded || isBotTyping) return; keepChatAtTop = false; await ShowBotReplyAsync(nlpSimulator.GetHelpMenu()); UserInputTextBox.Focus(); }
        private void StartNewChat() { ChatPanel.Children.Clear(); TypingIndicatorBorder.Visibility = Visibility.Collapsed; keepChatAtTop = true; if (isChatEnded) { AddBotMessage("Session ended."); ScrollChatToTop(); return; } chatbotEngine.ResetConversationButKeepUser(); AddBotMessage($"New chat, {userName}."); UserInputTextBox.Clear(); UpdateSessionPanel(); UpdatePlaceholderState(); UpdateSendButtonState(); UpdateInputGlowState(); ScrollChatToTop(); }
        private async Task LogoutAsync() { if (isBotTyping) { StopTypingAnimation(); isBotTyping = false; } if (!CyberDialog.ShowConfirmation(this, "Log Out", "Log out?")) { UserInputTextBox.Focus(); return; } CyberDialog.ShowMessage(this, "Logged Out", $"Stay safe, {userName}!"); ResetWelcomePage(); await ShowWelcomePageWithTransitionAsync(); }

        // ==================== BOT REPLY ====================
        private async Task ShowBotReplyAsync(string m) { isBotTyping = true; UpdateSendButtonState(); UpdateInputGlowState(); StartTypingAnimation(); await Task.Delay(CalculateTypingDelay(m)); StopTypingAnimation(); AddBotMessage(m); isBotTyping = false; UpdateSendButtonState(); UpdateInputGlowState(); }
        private void StartTypingAnimation() { typingDotCount = 0; currentTypingMessage = typingMessages[random.Next(typingMessages.Length)]; TypingIndicatorBorder.Visibility = Visibility.Visible; TypingIndicatorBorder.Opacity = 0; TypingIndicatorTextBlock.Text = currentTypingMessage; _ = FadeElementAsync(TypingIndicatorBorder, 0, 1, 180); StartBouncingTypingDots(); typingDotsTimer.Start(); }
        private void StopTypingAnimation() { typingDotsTimer.Stop(); StopBouncingTypingDots(); TypingIndicatorTextBlock.Text = ""; TypingIndicatorBorder.Opacity = 0; TypingIndicatorBorder.Visibility = Visibility.Collapsed; }
        private void TypingDotsTimer_Tick(object sender, EventArgs e) { typingDotCount++; if (typingDotCount > 3) typingDotCount = 0; TypingIndicatorTextBlock.Text = currentTypingMessage + new string('.', typingDotCount); }
        private int CalculateTypingDelay(string m) { if (string.IsNullOrWhiteSpace(m)) return 1700; int d = m.Length * 18; if (m.Contains("HIGH") || m.Contains("EMERGENCY")) d += 700; if (d < 1700) d = 1700; if (d > 4800) d = 4800; return d; }
        private void UpdateSessionPanel() { SessionUserTextBlock.Text = $"User: {userName}"; SessionLastTopicTextBlock.Text = $"Last topic: {chatbotEngine.LastTopicDisplay}"; SessionMoodTextBlock.Text = $"Mood: {chatbotEngine.LastSentimentDisplay}"; AnimateSidebarUpdateFlash(); }
        private void UpdatePlaceholderState() { if (PlaceholderTextBlock != null && UserInputTextBox != null) PlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text) ? Visibility.Visible : Visibility.Collapsed; }
        private void UpdateSendButtonState() { if (SendButton != null && UserInputTextBox != null) SendButton.IsEnabled = !string.IsNullOrWhiteSpace(UserInputTextBox.Text.Trim()) && !isChatEnded && !isBotTyping; }

        // ==================== MESSAGE BUBBLES ====================
        private void AddUserMessage(string m) { AddMessageBubble(userName, m, DateTime.Now.ToString("HH:mm"), Color.FromRgb(249, 115, 22), Colors.White, HorizontalAlignment.Right, true); }
        private void AddBotMessage(string m) { string rl = DetectRiskLevelFromMessage(m); Color bg = Color.FromRgb(245, 245, 245); if (rl == "Emergency") bg = Color.FromRgb(255, 237, 213); else if (rl == "High") bg = Color.FromRgb(255, 247, 237); else if (rl == "Medium") bg = Color.FromRgb(255, 251, 235); AddMessageBubble("CyberBot", m, DateTime.Now.ToString("HH:mm"), bg, Color.FromRgb(17, 17, 17), HorizontalAlignment.Left, false); }
        private void AddMessageBubble(string s, string m, string t, Color bg, Color fg, HorizontalAlignment a, bool iu) { double mw = GetResponsiveBubbleMaxWidth(); Thickness mg = iu ? new Thickness(100, 7, 0, 7) : new Thickness(0, 7, 100, 7); string rl = iu ? "" : DetectRiskLevelFromMessage(m); Border b = new Border { Background = new SolidColorBrush(bg), CornerRadius = new CornerRadius(20), Padding = new Thickness(15, 10, 15, 9), Margin = mg, HorizontalAlignment = a, MinWidth = 175, MaxWidth = mw, Opacity = 0, RenderTransform = new TranslateTransform(0, 12) }; if (iu) { b.BorderThickness = new Thickness(1); b.BorderBrush = new SolidColorBrush(Color.FromRgb(234, 88, 12)); } else if (!string.IsNullOrWhiteSpace(rl)) { b.BorderThickness = new Thickness(2); b.BorderBrush = GetRiskBrush(rl); } else { b.BorderThickness = new Thickness(1); b.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)); } b.Effect = new DropShadowEffect { BlurRadius = iu ? 3 : 2, ShadowDepth = 1, Direction = 270, Opacity = iu ? 0.12 : 0.08, Color = Colors.Black }; StackPanel sp = new StackPanel(); if (!iu && rl != "") sp.Children.Add(BuildRiskBanner(rl)); TextBlock stb = new TextBlock { Text = s, Foreground = new SolidColorBrush(fg), FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 3) }; TextBlock mtb = new TextBlock { Text = m, Foreground = new SolidColorBrush(fg), TextWrapping = TextWrapping.Wrap, FontSize = 14, FontWeight = FontWeights.SemiBold, LineHeight = 20, MaxWidth = mw - 30 }; TextBlock ttb = new TextBlock { Text = t, Foreground = new SolidColorBrush(fg), FontSize = 9.5, FontWeight = FontWeights.SemiBold, Opacity = 0.70, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 6, 0, 0) }; sp.Children.Add(stb); sp.Children.Add(mtb); sp.Children.Add(ttb); b.Child = sp; ChatPanel.Children.Add(b); AnimateMessageBubble(b); if (keepChatAtTop) ScrollChatToTop(); else ScrollChatToBottom(); }
        private Border BuildRiskBanner(string rl) { string txt = "CYBER RISK"; if (rl == "Emergency") txt = "EMERGENCY"; else if (rl == "High") txt = "HIGH RISK"; else if (rl == "Medium") txt = "MEDIUM RISK"; else if (rl == "Low") txt = "LOW RISK"; Border bn = new Border { Background = GetRiskBrush(rl), CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 0, 0, 8), HorizontalAlignment = HorizontalAlignment.Left }; TextBlock tb2 = new TextBlock { Text = txt, Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeights.Bold }; bn.Child = tb2; return bn; }
        private SolidColorBrush GetRiskBrush(string r) { switch (r) { case "Emergency": return new SolidColorBrush(Color.FromRgb(185, 28, 28)); case "High": return new SolidColorBrush(Color.FromRgb(234, 88, 12)); case "Medium": return new SolidColorBrush(Color.FromRgb(249, 115, 22)); case "Low": return new SolidColorBrush(Color.FromRgb(34, 197, 94)); default: return new SolidColorBrush(Color.FromRgb(249, 115, 22)); } }
        private string DetectRiskLevelFromMessage(string m) { if (string.IsNullOrWhiteSpace(m)) return ""; string lm = m.ToLower(); if (lm.Contains("emergency")) return "Emergency"; if (lm.Contains("risk level: high")) return "High"; if (lm.Contains("risk level: medium")) return "Medium"; if (lm.Contains("risk level: low")) return "Low"; return ""; }

        // ==================== CORE ANIMATIONS ====================
        private Task FadeElementAsync(UIElement e, double f, double t, int ms) { TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(); DoubleAnimation da = new DoubleAnimation(f, t, TimeSpan.FromMilliseconds(ms)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; da.Completed += (s, ev) => tcs.SetResult(true); e.BeginAnimation(UIElement.OpacityProperty, da); return tcs.Task; }
        private Task FadeSlideElementAsync(UIElement e, double fo, double to, double fy, double ty, int ms) { TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(); TranslateTransform tt = e.RenderTransform as TranslateTransform; if (tt == null || tt.IsFrozen) { tt = new TranslateTransform(0, 0); e.RenderTransform = tt; } DoubleAnimation oa = new DoubleAnimation(fo, to, TimeSpan.FromMilliseconds(ms)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; DoubleAnimation sa = new DoubleAnimation(fy, ty, TimeSpan.FromMilliseconds(ms)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } }; oa.Completed += (s, ev) => tcs.SetResult(true); e.BeginAnimation(UIElement.OpacityProperty, oa); tt.BeginAnimation(TranslateTransform.YProperty, sa); return tcs.Task; }
        private void AnimateLoadingCardIn() { if (LoadingCardBorder == null) return; LoadingCardBorder.Opacity = 0; TransformGroup tg = LoadingCardBorder.RenderTransform as TransformGroup; if (tg == null || tg.IsFrozen) { tg = new TransformGroup(); tg.Children.Add(new ScaleTransform(0.96, 0.96)); tg.Children.Add(new TranslateTransform(0, 18)); LoadingCardBorder.RenderTransform = tg; LoadingCardBorder.RenderTransformOrigin = new Point(0.5, 0.5); } ScaleTransform st = tg.Children[0] as ScaleTransform; TranslateTransform tt2 = tg.Children[1] as TranslateTransform; if (st == null || st.IsFrozen) { st = new ScaleTransform(0.96, 0.96); tg.Children[0] = st; } if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(0, 18); tg.Children[1] = tt2; } LoadingCardBorder.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }); st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(320)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.22 } }); st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(320)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.22 } }); tt2.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(18, 0, TimeSpan.FromMilliseconds(320)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } }); }
        private void AnimateLoadingCardOut() { if (LoadingCardBorder == null) return; TransformGroup tg = LoadingCardBorder.RenderTransform as TransformGroup; if (tg == null || tg.IsFrozen) return; ScaleTransform st = tg.Children[0] as ScaleTransform; TranslateTransform tt2 = tg.Children[1] as TranslateTransform; LoadingCardBorder.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(180))); if (st != null && !st.IsFrozen) { st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.97, TimeSpan.FromMilliseconds(180))); st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.97, TimeSpan.FromMilliseconds(180))); } if (tt2 != null && !tt2.IsFrozen) tt2.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(10, TimeSpan.FromMilliseconds(180))); }
        private void StartLoadingProgressAnimation() { if (LoadingProgressBar == null) return; StopLoadingProgressAnimation(); TranslateTransform tt2 = LoadingProgressBar.RenderTransform as TranslateTransform; if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(-100, 0); LoadingProgressBar.RenderTransform = tt2; } DoubleAnimation pa = new DoubleAnimation(-110, 390, TimeSpan.FromMilliseconds(1350)) { RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } }; loadingProgressStoryboard = new Storyboard(); loadingProgressStoryboard.Children.Add(pa); Storyboard.SetTarget(pa, LoadingProgressBar); Storyboard.SetTargetProperty(pa, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)")); loadingProgressStoryboard.Begin(); }
        private void StopLoadingProgressAnimation() { if (loadingProgressStoryboard != null) { loadingProgressStoryboard.Stop(); loadingProgressStoryboard = null; } }
        private void StartLoadingIconPulse() { if (LoadingShieldIconBorder == null) return; ScaleTransform st = LoadingShieldIconBorder.RenderTransform as ScaleTransform; if (st == null || st.IsFrozen) { st = new ScaleTransform(1, 1); LoadingShieldIconBorder.RenderTransform = st; LoadingShieldIconBorder.RenderTransformOrigin = new Point(0.5, 0.5); } DoubleAnimation pa = new DoubleAnimation(1, 1.07, TimeSpan.FromMilliseconds(650)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3), EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } }; st.BeginAnimation(ScaleTransform.ScaleXProperty, pa); st.BeginAnimation(ScaleTransform.ScaleYProperty, pa); }
        private void StartOnlineStatusPulse() { AnimateOnlineDot(WelcomeOnlineDot); AnimateOnlineDot(ChatOnlineDot); }
        private void AnimateOnlineDot(Ellipse dot) { if (dot == null) return; ScaleTransform st = dot.RenderTransform as ScaleTransform; if (st == null || st.IsFrozen) { st = new ScaleTransform(1, 1); dot.RenderTransform = st; dot.RenderTransformOrigin = new Point(0.5, 0.5); } DoubleAnimation sa = new DoubleAnimation(1, 1.55, TimeSpan.FromMilliseconds(1100)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }; DoubleAnimation oa = new DoubleAnimation(1, 0.45, TimeSpan.FromMilliseconds(1100)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }; st.BeginAnimation(ScaleTransform.ScaleXProperty, sa); st.BeginAnimation(ScaleTransform.ScaleYProperty, sa); dot.BeginAnimation(UIElement.OpacityProperty, oa); }
        private void StartBotAvatarBreathing() { if (HeaderBotAvatarBorder == null) return; ScaleTransform st = HeaderBotAvatarBorder.RenderTransform as ScaleTransform; if (st == null || st.IsFrozen) { st = new ScaleTransform(1, 1); HeaderBotAvatarBorder.RenderTransform = st; HeaderBotAvatarBorder.RenderTransformOrigin = new Point(0.5, 0.5); } DoubleAnimation ba = new DoubleAnimation(1, 1.045, TimeSpan.FromMilliseconds(1600)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }; st.BeginAnimation(ScaleTransform.ScaleXProperty, ba); st.BeginAnimation(ScaleTransform.ScaleYProperty, ba); }
        private void AnimateInputFocusGlow(bool sg) { if (InputOuterBorder != null) AnimateTextBarGlow(InputOuterBorder, sg); }
        private void AnimateWelcomeNameTextBarGlow(bool sg) { Border wb = FindParentBorder(WelcomeNameTextBox); if (wb != null) AnimateTextBarGlow(wb, sg); }
        private void AnimateTextBarGlow(Border t, bool sg) { if (t == null) return; Color a = Color.FromRgb(249, 115, 22); Color i = Color.FromRgb(221, 221, 221); t.BorderBrush = new SolidColorBrush(sg ? a : i); DropShadowEffect ge = new DropShadowEffect { BlurRadius = sg ? 5 : 0, ShadowDepth = 0, Opacity = sg ? 0.18 : 0, Color = a }; t.Effect = ge; DoubleAnimation ba = new DoubleAnimation(sg ? 5 : 0, TimeSpan.FromMilliseconds(160)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; DoubleAnimation oa = new DoubleAnimation(sg ? 0.18 : 0, TimeSpan.FromMilliseconds(160)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; ge.BeginAnimation(DropShadowEffect.BlurRadiusProperty, ba); ge.BeginAnimation(DropShadowEffect.OpacityProperty, oa); }
        private Border FindParentBorder(DependencyObject c) { while (c != null) { c = VisualTreeHelper.GetParent(c); if (c is Border b) return b; } return null; }
        private void UpdateAllTextBarGlowStates() { UpdateWelcomeNameGlowState(); UpdateInputGlowState(); }
        private void UpdateWelcomeNameGlowState() { if (WelcomeNameTextBox != null) AnimateWelcomeNameTextBarGlow(!string.IsNullOrWhiteSpace(WelcomeNameTextBox.Text) || WelcomeNameTextBox.IsKeyboardFocusWithin); }
        private void UpdateInputGlowState() { if (UserInputTextBox != null) AnimateInputFocusGlow(!string.IsNullOrWhiteSpace(UserInputTextBox.Text) || UserInputTextBox.IsKeyboardFocusWithin); }
        private void AnimateSidebarUpdateFlash() { if (SessionInfoBorder == null) return; SolidColorBrush fb = new SolidColorBrush(Color.FromRgb(249, 115, 22)); SessionInfoBorder.BorderBrush = fb; ColorAnimation ca = new ColorAnimation(Color.FromRgb(249, 115, 22), Color.FromRgb(34, 197, 94), TimeSpan.FromMilliseconds(650)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; fb.BeginAnimation(SolidColorBrush.ColorProperty, ca); }
        private void StartBouncingTypingDots() { AnimateTypingDot(TypingDot1, 0); AnimateTypingDot(TypingDot2, 140); AnimateTypingDot(TypingDot3, 280); }
        private void AnimateTypingDot(Ellipse dot, int d) { if (dot == null) return; TranslateTransform tt2 = dot.RenderTransform as TranslateTransform; if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(0, 0); dot.RenderTransform = tt2; dot.RenderTransformOrigin = new Point(0.5, 0.5); } DoubleAnimation ba = new DoubleAnimation(0, -5, TimeSpan.FromMilliseconds(360)) { BeginTime = TimeSpan.FromMilliseconds(d), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }; DoubleAnimation oa = new DoubleAnimation(0.45, 1, TimeSpan.FromMilliseconds(360)) { BeginTime = TimeSpan.FromMilliseconds(d), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }; tt2.BeginAnimation(TranslateTransform.YProperty, ba); dot.BeginAnimation(UIElement.OpacityProperty, oa); }
        private void StopBouncingTypingDots() { StopTypingDot(TypingDot1); StopTypingDot(TypingDot2); StopTypingDot(TypingDot3); }
        private void StopTypingDot(Ellipse dot) { if (dot == null) return; if (dot.RenderTransform is TranslateTransform tt2) { tt2.BeginAnimation(TranslateTransform.YProperty, null); tt2.Y = 0; } dot.BeginAnimation(UIElement.OpacityProperty, null); dot.Opacity = 1; }
        private void AnimateMessageBubble(Border b) { if (b == null) return; TranslateTransform tt2 = b.RenderTransform as TranslateTransform; if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(0, 12); b.RenderTransform = tt2; } DoubleAnimation oa = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; DoubleAnimation sa = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } }; b.BeginAnimation(UIElement.OpacityProperty, oa); tt2.BeginAnimation(TranslateTransform.YProperty, sa); }
        private void AnimateButtonPress(Button btn) { if (btn == null) return; ScaleTransform st = btn.RenderTransform as ScaleTransform; if (st == null || st.IsFrozen) { st = new ScaleTransform(1, 1); btn.RenderTransform = st; btn.RenderTransformOrigin = new Point(0.5, 0.5); } DoubleAnimation sa = new DoubleAnimation(1, 0.96, TimeSpan.FromMilliseconds(80)) { AutoReverse = true, EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }; st.BeginAnimation(ScaleTransform.ScaleXProperty, sa); st.BeginAnimation(ScaleTransform.ScaleYProperty, sa); }
        private void PremiumHoverCard_MouseEnter(object sender, MouseEventArgs e) { Border c = sender as Border; if (c == null) return; TranslateTransform tt2 = c.RenderTransform as TranslateTransform; if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(0, 0); c.RenderTransform = tt2; c.RenderTransformOrigin = new Point(0.5, 0.5); } tt2.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-2, TimeSpan.FromMilliseconds(160)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }); }
        private void PremiumHoverCard_MouseLeave(object sender, MouseEventArgs e) { Border c = sender as Border; if (c == null) return; TranslateTransform tt2 = c.RenderTransform as TranslateTransform; if (tt2 == null || tt2.IsFrozen) { tt2 = new TranslateTransform(0, -2); c.RenderTransform = tt2; c.RenderTransformOrigin = new Point(0.5, 0.5); } tt2.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(160)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } }); }
        private double GetResponsiveBubbleMaxWidth() { double cw = ChatScrollViewer.ActualWidth; if (cw <= 0) return 560; double calc = cw * 0.68; if (calc < 360) return 360; if (calc > 650) return 650; return calc; }
        private void ScrollChatToTop() { ChatScrollViewer.UpdateLayout(); ChatScrollViewer.ScrollToTop(); }
        private void ScrollChatToBottom() { ChatScrollViewer.UpdateLayout(); ChatScrollViewer.ScrollToEnd(); }
    }
}