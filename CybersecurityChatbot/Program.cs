using CybersecurityChatbot.Controllers;

namespace CybersecurityChatbot
{
    // Main entry point class for the Cybersecurity Chatbot application
    internal class Program
    {
        // Application entry point that creates and starts the chatbot controller
        static void Main(string[] args)
        {
            ChatbotController chatbotController = new ChatbotController();
            chatbotController.Run();
        }
    }
}