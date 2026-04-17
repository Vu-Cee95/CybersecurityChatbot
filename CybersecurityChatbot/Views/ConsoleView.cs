using System;
using System.Threading;

namespace CybersecurityChatbot.Views
{
    // This class handles all console-based UI (User Interface) interactions
    // It is responsible for displaying messages, animations, and formatting output
    internal static class ConsoleView
    {
        // Displays a loading/progress bar when the chatbot is starting
        public static void ShowLoadingBar()
        {
            // Set text color to Magenta for styling the loading message
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nInitializing Cyber-Bot...\n");
            Console.ResetColor(); // Reset color back to default

            int barWidth = 30;          // Total width of the loading bar
            int totalTime = 5000;       // Total duration of loading (in milliseconds)
            int delay = totalTime / 100; // Delay between each percentage update

            // Loop from 0% to 100% to simulate loading progress
            for (int i = 0; i <= 100; i++)
            {
                int progress = (i * barWidth) / 100; // Calculate how many blocks should be filled
                char[] bar = new char[barWidth];     // Create an array representing the bar

                // Fill the loading bar with blocks (█) or spaces
                for (int j = 0; j < barWidth; j++)
                {
                    if (j < progress)
                        bar[j] = '█'; // Filled portion
                    else
                        bar[j] = ' '; // Empty portion
                }

                string percentText = " " + i + "%"; // Create percentage text (e.g., " 45%")
                int percentPosition = progress;     // Default position for percentage text

                // Ensure percentage text does not overflow the bar
                if (percentPosition + percentText.Length >= barWidth)
                    percentPosition = barWidth - percentText.Length;

                // Ensure position is not negative
                if (percentPosition < 0)
                    percentPosition = 0;

                // Insert percentage text into the bar array
                for (int k = 0; k < percentText.Length; k++)
                {
                    bar[percentPosition + k] = percentText[k];
                }

                Console.CursorLeft = 0; // Move cursor to start of line (overwrite previous frame)
                Console.Write("[" + new string(bar) + "]"); // Display the loading bar

                Thread.Sleep(delay); // Pause to simulate loading progression
            }

            Thread.Sleep(1000); // Pause briefly after loading completes
            Console.WriteLine("\n"); // Move to next line after completion
        }

        // Displays a chatbot response in a styled format
        public static void BotReply(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan; // Set bot name color
            Console.Write("Cyber-Bot: ");
            Console.ResetColor(); // Reset color for message text
            TypeText(message);    // Display message with typing animation
        }

        // Displays the user's message in a styled format
        public static void UserReply(string userName, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green; // Set user name color
            Console.Write(userName + ": ");
            Console.ResetColor(); // Reset color for message text
            Console.WriteLine(message); // Display user message normally
        }

        // Simulates typing effect by printing characters one at a time
        public static void TypeText(string text)
        {
            foreach (char letter in text)
            {
                Console.Write(letter);     // Print each character
                Thread.Sleep(20);          // Delay to simulate typing speed
            }
            Console.WriteLine(); // Move to next line after message is complete
        }

        // Centers text output horizontally in the console window
        public static void CenterWriteLine(string text)
        {
            int width = Console.WindowWidth; // Get console window width
            int leftPadding = Math.Max(0, (width - text.Length) / 2); // Calculate padding
            Console.WriteLine(new string(' ', leftPadding) + text);   // Print centered text
        }
    }
}