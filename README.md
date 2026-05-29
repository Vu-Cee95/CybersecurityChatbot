# 🔐 Cybersecurity Awareness Chatbot (Part 1)

## 📌 Project Overview

The **Cybersecurity Awareness Chatbot** is a C# console-based application developed for the **PROG6221 Portfolio of Evidence (Part 1)**.

The purpose of this chatbot is to educate users on important cybersecurity topics such as:

* Password safety
* Phishing awareness
* Safe browsing practices

The chatbot simulates a real-life conversation, providing users with practical advice on how to stay safe online.

---

## 🎯 Objectives

* Create an interactive chatbot using a console application
* Apply string manipulation and input handling
* Implement user-friendly interaction
* Enhance the console UI with multimedia elements
* Structure code using Object-Oriented Programming (OOP)

---

## 🚀 Features

### 🔊 Voice Greeting

* Plays a **WAV audio file** when the application starts
* Welcomes the user with a personalised cybersecurity message

### 🎨 ASCII Art Display

* Displays a cybersecurity-themed ASCII logo at startup
* Enhances the visual appeal of the console interface

### 👤 Personalised User Interaction

* Prompts the user for their name
* Uses the name throughout the conversation

### 💬 Basic Response System

The chatbot responds to:

* "How are you?"
* "What is your purpose?"
* "What can I ask you?"

It also provides guidance on:

* Password safety
* Phishing
* Safe browsing

### ⚠️ Input Validation

* Handles empty input
* Detects unknown queries
* Responds with:
  *"I didn’t quite understand that. Could you rephrase?"*

### 🖥️ Enhanced Console UI

* Uses colours (`Console.ForegroundColor`)
* Includes borders and separators
* Uses spacing for readability
* Simulates typing effect

### 🧠 Code Structure

Organised using:

* Controllers
* Models
* Services
* Views
* Utils

`Program.cs` is kept minimal and clean.

---


## 🧠 Code Quality

The project follows clean coding practices:
- Modular structure using MVC pattern
- Separation of concerns
- Readable and maintainable code
- Use of methods and classes for scalability

---

## 🛠️ Technologies Used

* C#
* .NET Console Application
* System.Media (audio playback)
* Git & GitHub
* GitHub Actions (CI)

---

## 📂 Project Structure

```text
CybersecurityChatbot/
│── README.md
│── .gitignore
│── .gitattributes
│── CybersecurityChatbot.slnx
│
└── CybersecurityChatbot/
    │── Program.cs
    │
    ├── Controllers/
    │   └── ChatbotController.cs
    │
    ├── Models/
    │   ├── User.cs
    │   ├── Message.cs
    │   └── ChatSession.cs
    │
    ├── Services/
    │   ├── ChatbotService.cs
    │   ├── ResponseService.cs
    │   └── NavigationService.cs
    │
    ├── Views/
    │   └── ConsoleView.cs
    │
    ├── Utils/
    │   ├── InputValidator.cs
    │   └── Helper.cs
    │
    └── Assets/
        └── welcome.wav
```

---

## ▶️ How to Run the Project

### Prerequisites

* Visual Studio
* .NET SDK installed

### Steps

1. Clone the repository:

```bash
git clone https://github.com/Vu-Cee95/CybersecurityChatbot.git
```

2. Open the folder:

```bash
cd CybersecurityChatbot
```

3. Open the solution:

* Open `CybersecurityChatbot.slnx` in Visual Studio

4. Build the project:

* Press `Ctrl + Shift + B`

5. Run the application:

* Press `F5`

6. Ensure:

* `welcome.wav` is inside the **Assets** folder

---

## 💡 Example Usage

```text
Bot: Hello! Welcome to the Cybersecurity Awareness Chatbot.
Bot: What is your name?
User: Vusimuzi

Bot: Nice to meet you, Vusimuzi! How can I help you today?

User: Tell me about phishing
Bot: Phishing is a scam where attackers try to trick you into revealing personal information through fake emails or websites.
```

---

## 🔁 GitHub Version Control

This project uses GitHub with meaningful commits.

### Example Commit Messages

* Initial project setup
* Added voice greeting
* Implemented ASCII art
* Added chatbot responses
* Implemented input validation
* Added README and improvements

---

## ⚙️ Continuous Integration (CI)

GitHub Actions is used to:

* Build the project automatically
* Detect errors on each push

### CI Status

```text
[CI STATUS] https://github.com/Vu-Cee95/CybersecurityChatbot/blob/master/GitCIWorkflow.png


```

---

## 🎥 Video Demonstration

```text
Watch the full project walkthrough below:

[YOUTUBE PRESENTATION LINK] https://youtu.be/A_N5yeuaaAg
```

This video includes:

- Program execution
- Code structure explanation
- Logic and flow
- Feature demonstration
---

## ✅ Alignment with Requirements

This project includes:

* Voice greeting
* ASCII art
* Personalised interaction
* Cybersecurity responses
* Input validation
* Enhanced console UI
* Structured code using classes
* GitHub version control
* CI workflow support

---

## ✅ Submission Checklist

* [x] Source code included
* [x] README file included
* [x] WAV audio file included
* [x] ASCII art implemented
* [x] GitHub Actions CI working
* [x] CI screenshot added
* [x] YouTube video link added

---

## 👨‍💻 Author

**Vusimuzi Khanyile**
Diploma in Information Technology (Systems Development)
Rosebank College

---

## 📌 Notes

* This project is for academic purposes
* All work follows academic integrity policies

---

## 🚀 Future Improvements

* GUI (WinForms / WPF)
* Keyword recognition
* Memory & recall
* Sentiment detection
* Cybersecurity quiz
* Task assistant

---

## 📌 Update Log

* README created and structured
* Project prepared for Part 1 submission

------------------------------------------
# CyberBot – Cybersecurity Awareness Chatbot

CyberBot is a WPF-based cybersecurity awareness chatbot that helps users learn about online safety topics such as passwords, phishing, scams, privacy, malware, safe browsing, and two-factor authentication.

---

## Student Details

**Student Name:** Vusimuzi Khanyile  
**Student Number:** ST10468302  
**Module:** Programming 2A  
**Module Code:** PROG6221/w  
**Assessment:** Portfolio of Evidence – Part 2  
**Application Type:** WPF Desktop Application  
**Application Name:** CyberBot  

---

## Project Overview

CyberBot is a graphical cybersecurity awareness chatbot developed using C# and Windows Presentation Foundation. The application extends the Part 1 console chatbot into a full GUI application while keeping the original Part 1 features such as the voice greeting, ASCII art, and personalised user interaction.

The Part 2 version adds keyword recognition, random responses, memory and recall, sentiment detection, and improved conversation flow.

CyberBot is designed to educate users about cybersecurity in a simple, friendly, and interactive way.

-----------------------------------
---

## Part 2 Features Implemented

This project implements the required Part 2 chatbot features as follows:

### 1. Graphical User Interface

CyberBot was converted from a console-based chatbot into a WPF desktop application. The GUI includes:

- A welcome page
- A chatbot name and branding area
- ASCII art display
- Name input field
- Main chat screen
- Chat history area
- User and bot message bubbles
- Text input box
- Send button
- Scrollable chat area
- Help, New Chat, and Logout menu options

---

### 2. Voice Greeting

CyberBot plays a voice greeting when the application starts.

The greeting audio file is stored in:

```text
Assets/welcome.wav