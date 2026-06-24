using System;
using System.Collections.Generic;
using System.Text;

namespace WPF_Chatbot
{
    // Start of namespace


    public class ActivityLog
    {
        // Start of class

        // Maximum number of recent actions to show when displaying the log
        const int MaxDisplayEntries = 10;

        // Each entry stores what happened and when
        class LogEntry
        {
            // Start of log entry class
            public string Action { get; set; }
            public DateTime Timestamp { get; set; }
            // End of log entry class
        }

        // Full list of all actions taken - entries are never deleted,
        // we just only show the last MaxDisplayEntries when printing
        List<LogEntry> entries = new List<LogEntry>();

        // ── Public methods ────────────────────────────────────────────────────────

        // Adds a new action to the log with the current timestamp
        public void AddEntry(string action)
        {
            // Start of add entry method
            entries.Add(new LogEntry
            {
                Action = action,
                Timestamp = DateTime.Now
            });
            // End of add entry method
        }

        // Returns a numbered summary of the last 5-10 actions, newest last.
        // Matches the example format shown in the Task 4 brief.
        public string GetSummary()
        {
            // Start of get summary method
            if (entries.Count == 0)
                return "No actions have been recorded yet.\n" +
                       "Try adding a task, taking the quiz, or asking a cybersecurity question!";

            // Only show the most recent MaxDisplayEntries entries
            int startIndex = Math.Max(0, entries.Count - MaxDisplayEntries);
            int shown = entries.Count - startIndex;

            StringBuilder sb = new StringBuilder();
            sb.Append("Here's a summary of recent actions");

            if (entries.Count > MaxDisplayEntries)
                sb.Append(" (showing last " + shown + " of " + entries.Count + " total)");

            sb.Append(":\n\n");

            int number = 1;
            for (int i = startIndex; i < entries.Count; i++)
            {
                sb.Append(number)
                  .Append(". ")
                  .Append(entries[i].Action)
                  .Append(" — ")
                  .Append(entries[i].Timestamp.ToString("HH:mm"))
                  .Append("\n");

                number++;
            }

            return sb.ToString().TrimEnd();
            // End of get summary method
        }

        // Returns the total number of actions logged this session
        public int TotalCount => entries.Count;

        // End of class
    }
    // End of namespace
}