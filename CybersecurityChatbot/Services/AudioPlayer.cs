using System;
using System.IO;
using System.Media;

namespace CybersecurityChatbot.Services
{
    // Handles audio playback functionality for the chatbot application
    internal class AudioPlayer
    {
        // Plays the welcome greeting audio file synchronously when the chatbot starts
        public void PlayGreeting()
        {
            try
            {
                // Construct the path to the welcome audio file in the Assets directory
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "welcome.wav");

                if (File.Exists(path))
                {
                    SoundPlayer player = new SoundPlayer(path);
                    player.Load();
                    player.PlaySync(); // waits until audio finishes
                }
                else
                {
                    Console.WriteLine("Audio file not found at: " + path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing audio: " + ex.Message);
            }
        }
    }
}