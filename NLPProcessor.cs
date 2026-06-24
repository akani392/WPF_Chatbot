using System;
using System.Text.RegularExpressions;

namespace WPF_Chatbot
{
    // Start of namespace

    public class NLPProcessor
    {
        // Start of class

        
        public enum Intent
        {
            Unknown,        // could not map to anything specific
            AddTask,        // user wants to create a task
            ViewTasks,      // user wants to see their task list
            CompleteTask,   // user wants to mark a task done
            DeleteTask,     // user wants to remove a task
            StartQuiz,      // user wants to play the quiz
            AskCyber,       // user is asking a cybersecurity question
            ViewSummary     // user wants the activity log summary
        }

        
        public class NLPResult
        {
            // Start of NLP result class
            public Intent DetectedIntent { get; set; } = Intent.Unknown;
            public string ExtractedTitle { get; set; } = "";   // task title if found
            public int? ExtractedDays { get; set; } = null; // reminder days if found
            public string NormalisedCommand { get; set; } = ""; // ready-to-use command string
            // End of NLP result class
        }

        
        // Phrases that signal the user wants to add a task or set a reminder
        string[] addTaskPhrases = {
            "add a task", "add task", "create a task", "create task",
            "remind me to", "set a reminder", "set reminder",
            "can you remind me", "i need to remember", "don't let me forget",
            "make a note", "note that", "i want to", "help me remember"
        };

        // Phrases that signal the user wants to see their tasks
        string[] viewTaskPhrases = {
            "view tasks", "show tasks", "show my tasks", "list tasks",
            "what tasks", "my tasks", "see my tasks", "tasks i have",
            "what do i need to do", "what's on my list"
        };

        // Phrases that signal the user wants to complete a task
        string[] completeTaskPhrases = {
            "complete task", "mark as done", "mark task", "finished task",
            "done with task", "task complete", "i completed", "i finished"
        };

        // Phrases that signal the user wants to delete a task
        string[] deleteTaskPhrases = {
            "delete task", "remove task", "cancel task", "get rid of task",
            "erase task", "drop task"
        };

        // Phrases that signal the user wants the quiz
        string[] quizPhrases = {
            "quiz", "test my knowledge", "test me", "play quiz",
            "start quiz", "cyber quiz", "take a quiz", "want a quiz"
        };

        // Phrases that signal the user wants the activity summary
        string[] summaryPhrases = {
            "what have you done", "recent actions", "activity log",
            "summary", "what did you do", "history", "show log",
            "what actions", "what have you done for me"
        };

        // Cybersecurity topic keywords — same topics the chatbot already covers
        string[] cyberKeywords = {
            "phishing", "malware", "virus", "ransomware", "trojan",
            "password", "firewall", "scam", "fraud", "hacker",
            "cybersecurity", "security", "encryption", "2fa",
            "authentication", "breach", "spyware", "social engineering"
        };

        
        public NLPResult Analyse(string message)
        {
            // Start of analyse method
            string msg = message.ToLower().Trim();
            NLPResult result = new NLPResult();

            // Check intents in priority order so more specific ones win
            if (MatchesAny(msg, summaryPhrases))
            {
                result.DetectedIntent = Intent.ViewSummary;
                result.NormalisedCommand = "summary";
                return result;
            }

            if (MatchesAny(msg, quizPhrases))
            {
                result.DetectedIntent = Intent.StartQuiz;
                result.NormalisedCommand = "quiz";
                return result;
            }

            if (MatchesAny(msg, completeTaskPhrases))
            {
                result.DetectedIntent = Intent.CompleteTask;
                result.NormalisedCommand = "complete " + ExtractNumber(msg);
                return result;
            }

            if (MatchesAny(msg, deleteTaskPhrases))
            {
                result.DetectedIntent = Intent.DeleteTask;
                result.NormalisedCommand = "delete " + ExtractNumber(msg);
                return result;
            }

            if (MatchesAny(msg, viewTaskPhrases))
            {
                result.DetectedIntent = Intent.ViewTasks;
                result.NormalisedCommand = "view tasks";
                return result;
            }

            if (MatchesAny(msg, addTaskPhrases))
            {
                result.DetectedIntent = Intent.AddTask;
                result.ExtractedTitle = ExtractTaskTitle(msg);
                result.ExtractedDays = ExtractDays(msg);

                // Build a normalised command that TaskAssistant already understands
                if (!string.IsNullOrWhiteSpace(result.ExtractedTitle))
                    result.NormalisedCommand = "add task - " + result.ExtractedTitle;
                else
                    result.NormalisedCommand = "add task";

                return result;
            }

            if (MatchesAny(msg, cyberKeywords))
            {
                result.DetectedIntent = Intent.AskCyber;
                result.NormalisedCommand = msg; // pass through unchanged
                return result;
            }

            // No intent matched
            result.DetectedIntent = Intent.Unknown;
            result.NormalisedCommand = msg;
            return result;
            // End of analyse method
        }

        
        bool MatchesAny(string message, string[] phrases)

        {
            // Start of matches any method
            foreach (string phrase in phrases)
            {
                if (message.Contains(phrase))
                    return true;
            }
            return false;
            // End of matches any method
        }

       
        string ExtractTaskTitle(string message)
        {
            // Start of extract task title method

            // Ordered from most specific to least specific so greedy patterns win
            string[] prefixesToStrip = {
                "can you remind me to",
                "remind me to",
                "don't let me forget to",
                "i need to remember to",
                "help me remember to",
                "set a reminder to",
                "set a reminder for",
                "set reminder to",
                "set reminder for",
                "add a task to",
                "add a task for",
                "create a task to",
                "create a task for",
                "add task to",
                "add task for",
                "add task -",
                "make a note to",
                "make a note about",
                "note that",
                "i want to",
                "i need to"
            };

            foreach (string prefix in prefixesToStrip)
            {
                if (message.Contains(prefix))
                {
                    int idx = message.IndexOf(prefix) + prefix.Length;
                    string title = message.Substring(idx).Trim();

                    // Remove trailing time phrases so title stays clean
                    title = Regex.Replace(title, @"\s*(tomorrow|today|next week|in \d+ days?)\s*$", "").Trim();
                    title = Regex.Replace(title, @"\s*\.\s*$", "").Trim();

                    return title;
                }
            }

            return "";
            // End of extract task title method
        }

        
        int? ExtractDays(string message)
        {
            // Start of extract days method
            if (message.Contains("tomorrow"))
                return 1;

            if (message.Contains("today"))
                return 0;

            if (message.Contains("next week"))
                return 7;

            // Match "in X days" or "in X day"
            Match match = Regex.Match(message, @"in\s+(\d+)\s+days?");
            if (match.Success)
                return int.Parse(match.Groups[1].Value);

            return null;
            // End of extract days method
        }

        // Pulls the first number from a message for complete/delete commands
        string ExtractNumber(string message)
        {
            // Start of extract number method
            Match match = Regex.Match(message, @"\d+");
            return match.Success ? match.Value : "";
            // End of extract number method
        }

        // End of class
    }
    // End of namespace
}