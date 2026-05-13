using System.Windows;
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
        }
    }
}