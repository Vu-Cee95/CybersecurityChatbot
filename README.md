# 🔐 Cybersecurity Awareness Chatbot (Part 1)

## 📌 Project Overview
The **Cybersecurity Awareness Chatbot** is a C# console-based application developed for the PROG6221 Portfolio of Evidence (Part 1).

The purpose of this chatbot is to educate users on important cybersecurity topics such as:
- Password safety
- Phishing awareness
- Safe browsing practices

The chatbot simulates a real-life conversation, providing users with practical advice on how to stay safe online.

---

## 🎯 Objectives
- Create an interactive chatbot using a console application
- Apply string manipulation and input handling
- Implement user-friendly interaction
- Enhance the console UI with multimedia elements
- Structure code using Object-Oriented Programming (OOP)

---

## 🚀 Features

### 🔊 Voice Greeting
- Plays a **WAV audio file** when the application starts
- Welcomes the user with a personalised cybersecurity message

### 🎨 ASCII Art Display
- Displays a cybersecurity-themed ASCII logo at startup
- Enhances the visual appeal of the console interface

### 👤 Personalised User Interaction
- Prompts the user for their name
- Uses the name throughout the conversation for a personalised experience

### 💬 Basic Response System
The chatbot responds to common questions such as:
- "How are you?"
- "What is your purpose?"
- "What can I ask you?"

It also provides cybersecurity guidance on:
- Password safety
- Phishing
- Safe browsing

### ⚠️ Input Validation
- Handles empty input
- Detects unknown queries
- Responds with:
  > "I didn’t quite understand that. Could you rephrase?"

### 🖥️ Enhanced Console UI
- Uses colours (Console.ForegroundColor)
- Includes borders and separators
- Implements spacing for readability
- Simulates typing effect for realism

### 🧠 Code Structure
- Clean separation of concerns using:
  - **Models**
  - **Services**
  - **Views**
  - **Controllers**
- Keeps `Program.cs` minimal and readable

---

## 🛠️ Technologies Used
- C#
- .NET Console Application
- System.Media (for audio playback)
- Git & GitHub
- GitHub Actions (CI)

---

## 📂 Project Structure
CybersecurityChatbot/
│── README.md
│── .gitignore
│── CybersecurityChatbot.sln
│
└── CybersecurityChatbot/
│── Program.cs
│
├── Controllers/
├── Models/
├── Services/
├── Views/
├── Utils/
│
└── Assets/
└── welcome.wav


---

## ▶️ How to Run the Project

### Prerequisites
- Visual Studio (recommended)
- .NET SDK installed

### Steps
1. Clone the repository:
   ```bash
   https://github.com/Vu-Cee95/CybersecurityChatbot

   ## 📌 Update Log
- README created and structured
