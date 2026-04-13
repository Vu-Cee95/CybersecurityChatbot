using System;
using System.Threading;

namespace CybersecurityChatbot.Views
{
    internal static class ConsoleView
    {
        public static void ShowLoadingBar()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nInitializing Cyber-Bot...\n");
            Console.ResetColor();

            int barWidth = 30;
            int totalTime = 5000;
            int delay = totalTime / 100;

            for (int i = 0; i <= 100; i++)
            {
                int progress = (i * barWidth) / 100;
                char[] bar = new char[barWidth];

                for (int j = 0; j < barWidth; j++)
                {
                    if (j < progress)
                        bar[j] = '█';
                    else
                        bar[j] = ' ';
                }

                string percentText = " " + i + "%";
                int percentPosition = progress;

                if (percentPosition + percentText.Length >= barWidth)
                    percentPosition = barWidth - percentText.Length;

                if (percentPosition < 0)
                    percentPosition = 0;

                for (int k = 0; k < percentText.Length; k++)
                {
                    bar[percentPosition + k] = percentText[k];
                }

                Console.CursorLeft = 0;
                Console.Write("[" + new string(bar) + "]");
                Thread.Sleep(delay);
            }

            Thread.Sleep(1000);
            Console.WriteLine("\n");
        }

        public static void BotReply(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Cyber-Bot: ");
            Console.ResetColor();
            TypeText(message);
        }

        public static void UserReply(string userName, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(userName + ": ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void TypeText(string text)
        {
            foreach (char letter in text)
            {
                Console.Write(letter);
                Thread.Sleep(20);
            }
            Console.WriteLine();
        }

        public static void CenterWriteLine(string text)
        {
            int width = Console.WindowWidth;
            int leftPadding = Math.Max(0, (width - text.Length) / 2);
            Console.WriteLine(new string(' ', leftPadding) + text);
        }
    }
}
