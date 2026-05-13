using System;
using System.IO;
using System.Media;

namespace CybersecurityChatbotGUI.Services
{
    public class AudioPlayer
    {
        public void PlayWelcomeSound()
        {
            try
            {
                string audioPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "welcome.wav"
                );

                if (!File.Exists(audioPath))
                {
                    return;
                }

                using SoundPlayer player = new SoundPlayer(audioPath);
                player.Load();
                player.Play();
            }
            catch
            {
                // The app must not crash if the audio fails.
            }
        }
    }
}