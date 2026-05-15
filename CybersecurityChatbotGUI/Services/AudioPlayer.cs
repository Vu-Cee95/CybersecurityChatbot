using System;
using System.IO;
using System.Media;
using System.Runtime.Versioning;

namespace CybersecurityChatbotGUI.Services
{
    public class AudioPlayer
    {
        public void PlayWelcomeSound()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            PlayWelcomeSoundOnWindows();
        }

        [SupportedOSPlatform("windows")]
        private void PlayWelcomeSoundOnWindows()
        {
            try
            {
                string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "welcome.wav");

                if (!File.Exists(audioPath))
                {
                    return;
                }

                using SoundPlayer player = new SoundPlayer(audioPath);
                player.Load();
                player.PlaySync();
            }
            catch
            {
                // Audio should not crash the application.
            }
        }
    }
}