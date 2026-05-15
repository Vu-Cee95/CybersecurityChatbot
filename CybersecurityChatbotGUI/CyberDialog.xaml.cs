using System.Windows;

namespace CybersecurityChatbotGUI
{
    public partial class CyberDialog : Window
    {
        public bool UserConfirmed { get; private set; }

        public CyberDialog(string title, string message, bool showCancelButton)
        {
            InitializeComponent();

            DialogTitleTextBlock.Text = title;
            DialogMessageTextBlock.Text = message;

            if (!showCancelButton)
            {
                NoButton.Visibility = Visibility.Collapsed;
                YesButton.Content = "OK";
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = false;
            DialogResult = false;
            Close();
        }

        public static bool ShowConfirmation(Window owner, string title, string message)
        {
            CyberDialog dialog = new CyberDialog(title, message, true);
            dialog.Owner = owner;
            dialog.ShowDialog();

            return dialog.UserConfirmed;
        }

        public static void ShowMessage(Window owner, string title, string message)
        {
            CyberDialog dialog = new CyberDialog(title, message, false);
            dialog.Owner = owner;
            dialog.ShowDialog();
        }
    }
}