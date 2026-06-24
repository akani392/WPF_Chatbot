using System.Collections.Generic;
using System.Text;

namespace WPF_Chatbot
{
    // Start of namespace
    public class QuizGame
    {
        // Start of class

        // Tracks whether the quiz is currently running
        bool quizActive = false;

        // Index of the question currently being asked
        int currentQuestionIndex = 0;

        // Number of correct answers so far
        int score = 0;

        class Question
        {
            // Start of question class
            public string Text { get; set; }  // the question shown to the user
            public string Options { get; set; }  // formatted A/B/C/D or True/False options
            public string Answer { get; set; }  // correct answer key e.g. "c" or "true"
            public string Explanation { get; set; } // short explanation shown after answering
            // End of question class
        }

        // The full list of quiz questions - more than 10 as required, mixing
        // multiple-choice (A/B/C/D) and true/false formats
        List<Question> questions = new List<Question>()
        {
            // Multiple choice 
            new Question
            {
                Text    = "What should you do if you receive an email asking for your password?",
                Options = "  A) Reply with your password\n  B) Delete the email\n  C) Report the email as phishing\n  D) Ignore it",
                Answer  = "c",
                Explanation = "Correct answer: C — Reporting phishing emails helps prevent scams and protects others."
            },
            new Question
            {
                Text    = "Which of the following is the strongest password?",
                Options = "  A) password123\n  B) MyDog2010\n  C) qwerty\n  D) T#9kL!mZ2@vQ",
                Answer  = "d",
                Explanation = "Correct answer: D — A strong password uses a mix of uppercase, lowercase, numbers, and symbols with no dictionary words."
            },
            new Question
            {
                Text    = "What does malware stand for?",
                Options = "  A) Managed software\n  B) Malicious software\n  C) Mobile application\n  D) Monitored application",
                Answer  = "b",
                Explanation = "Correct answer: B — Malware is short for 'malicious software', designed to harm or exploit devices."
            },
            new Question
            {
                Text    = "Which of the following best describes a firewall?",
                Options = "  A) A program that speeds up your internet\n  B) A barrier that filters network traffic\n  C) A type of antivirus software\n  D) A tool for recovering deleted files",
                Answer  = "b",
                Explanation = "Correct answer: B — A firewall monitors and filters incoming and outgoing network traffic based on security rules."
            },
            new Question
            {
                Text    = "What is phishing?",
                Options = "  A) A type of antivirus software\n  B) A way to speed up your computer\n  C) A scam that tricks users into giving up personal information\n  D) A method of encrypting files",
                Answer  = "c",
                Explanation = "Correct answer: C — Phishing uses fake emails or websites to trick users into revealing sensitive data."
            },
            new Question
            {
                Text    = "What should you do before clicking a link in an email?",
                Options = "  A) Click it immediately\n  B) Forward it to friends\n  C) Hover over it to preview the real URL\n  D) Reply to the sender first",
                Answer  = "c",
                Explanation = "Correct answer: C — Hovering over a link reveals the actual destination URL, helping you spot fake links."
            },
            new Question
            {
                Text    = "Which attack encrypts your files and demands payment to unlock them?",
                Options = "  A) Spyware\n  B) Ransomware\n  C) Adware\n  D) Worm",
                Answer  = "b",
                Explanation = "Correct answer: B — Ransomware encrypts a victim's files and demands a ransom (usually cryptocurrency) for the decryption key."
            },
            new Question
            {
                Text    = "What is two-factor authentication (2FA)?",
                Options = "  A) Using two different browsers\n  B) Logging in with two passwords\n  C) A second verification step added to login\n  D) A firewall feature",
                Answer  = "c",
                Explanation = "Correct answer: C — 2FA adds a second layer of security (e.g. a phone code) so a stolen password alone is not enough to log in."
            },
            new Question
            {
                Text    = "Which of these is a sign that your device may be infected with malware?",
                Options = "  A) Your device charges faster than normal\n  B) Your antivirus was suddenly disabled\n  C) Your screen brightness increased\n  D) Files load faster than usual",
                Answer  = "b",
                Explanation = "Correct answer: B — Malware often disables antivirus tools to prevent detection and removal."
            },
            new Question
            {
                Text    = "What is social engineering in cybersecurity?",
                Options = "  A) Building secure software systems\n  B) Manipulating people into revealing confidential information\n  C) A type of firewall configuration\n  D) Encrypting network traffic",
                Answer  = "b",
                Explanation = "Correct answer: B — Social engineering exploits human psychology rather than technical vulnerabilities to gain access."
            },
 
            // True or false
            new Question
            {
                Text    = "TRUE or FALSE: Using the same password for multiple accounts is safe as long as it is strong.",
                Options = "  Type: true  or  false",
                Answer  = "false",
                Explanation = "Correct answer: FALSE — Reusing passwords means a single breach can compromise all your accounts at once."
            },
            new Question
            {
                Text    = "TRUE or FALSE: A padlock icon in your browser's address bar means the website is completely safe to use.",
                Options = "  Type: true  or  false",
                Answer  = "false",
                Explanation = "Correct answer: FALSE — HTTPS only means the connection is encrypted. Scam websites can and do use HTTPS too."
            },
            new Question
            {
                Text    = "TRUE or FALSE: Keeping your software updated helps protect against malware.",
                Options = "  Type: true  or  false",
                Answer  = "true",
                Explanation = "Correct answer: TRUE — Software updates patch known security vulnerabilities that malware often exploits."
            }
        };

        //Public interface 
        public bool TryHandle(string message, out string reply)
        {
            // Start of try handle method
            reply = "";

            // Start the quiz if user types "quiz" and it is not already running
            if (!quizActive && message.Contains("quiz"))
            {
                reply = StartQuiz();
                return true;
            }

            // If the quiz is running, every message is treated as an answer
            if (quizActive)
            {
                reply = HandleAnswer(message);
                return true;
            }

            return false;
            // End of try handle method
        }

        // Resets state and shows the first question
        string StartQuiz()
        {
            // Start of start quiz method
            quizActive = true;
            currentQuestionIndex = 0;
            score = 0;

            return "Welcome to the Cybersecurity Quiz!\n\n" +
                   "There are " + questions.Count + " questions.\n" +
                   "For multiple-choice questions type A, B, C, or D.\n" +
                   "For true/false questions type true or false.\n" +
                   "Type \"quit quiz\" at any time to stop.\n\n" +
                   ShowCurrentQuestion();
            // End of start quiz method
        }

        // Checks the user's answer then moves to the next question (or ends the quiz)
        string HandleAnswer(string message)
        {
            // Start of handle answer method

            // Allow the user to exit early
            if (message.Contains("quit quiz") || message.Contains("stop quiz"))
            {
                quizActive = false;
                return "Quiz stopped. You answered " + currentQuestionIndex +
                       " question(s) and got " + score + " correct. Type \"quiz\" to try again!";
            }

            Question current = questions[currentQuestionIndex];
            string answer = message.Trim().ToLower();
            bool correct = answer == current.Answer;

            if (correct)
                score++;

            // Build the feedback message
            StringBuilder feedback = new StringBuilder();
            feedback.Append(correct ? "✓ Correct! " : "✗ Wrong. ");
            feedback.Append(current.Explanation);
            feedback.Append("\n\nScore so far: " + score + " / " + (currentQuestionIndex + 1));

            currentQuestionIndex++;

            // Check if all questions have been answered
            if (currentQuestionIndex >= questions.Count)
            {
                quizActive = false;
                feedback.Append("\n\n");
                feedback.Append(BuildFinalScore());
                return feedback.ToString();
            }

            // More questions remain - show the next one
            feedback.Append("\n\n─────────────────────\n\n");
            feedback.Append(ShowCurrentQuestion());
            return feedback.ToString();
            // End of handle answer method
        }

        // Formats the current question and its options
        string ShowCurrentQuestion()
        {
            // Start of show current question method
            Question q = questions[currentQuestionIndex];
            int number = currentQuestionIndex + 1;

            return "Question " + number + " of " + questions.Count + ":\n\n" +
                   q.Text + "\n\n" + q.Options;
            // End of show current question method
        }

        // Returns a score summary and motivational message shown at the end
        string BuildFinalScore()
        {
            // Start of build final score method
            int total = questions.Count;
            string percent = ((score * 100) / total) + "%";

            StringBuilder result = new StringBuilder();
            result.Append("==================================\n");
            result.Append("Quiz Complete!\n");
            result.Append("Final Score: " + score + " / " + total + "  (" + percent + ")\n");
            result.Append("===================================\n\n");

            // Score-based motivational feedback as required by the brief
            if (score == total)
                result.Append("Perfect score! You are a cybersecurity pro!");
            else if (score >= total * 0.8)
                result.Append("Great job! You have a strong understanding of cybersecurity.");
            else if (score >= total * 0.5)
                result.Append("Good effort! Keep learning to stay safe online.");
            else
                result.Append("Keep learning to stay safe online! Type \"quiz\" to try again.");

            return result.ToString();
            // End of build final score method
        }

        // End of class
    }
    // End of namespace
}