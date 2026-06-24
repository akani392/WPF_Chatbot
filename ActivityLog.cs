using System;
using System.Collections.Generic;
using System.Text;

namespace WPF_Chatbot
{
    // Start of namespace
    public class ActivityLog
    {
        // Start of class

        // Each entry stores the action text and the time it happened
        class LogEntry
        {
            // Start of log entry class
            public string Action { get; set; }
            public DateTime Time { get; set; }
            // End of log entry class
        }

        // The list of all actions taken this session
        List<LogEntry> entries = new List<LogEntry>();

        // Adds a new action to the log
        public void AddEntry(string action)
        {
            // Start of add entry method
            entries.Add(new LogEntry
            {
                Action = action,
                Time = DateTime.Now
            });
            // End of add entry method
        }

        // Returns a numbered summary of all actions taken, newest last
        public string GetSummary()
        {
            // Start of get summary method
            if (entries.Count == 0)
                return "No actions have been recorded yet this session.";

            StringBuilder sb = new StringBuilder("Here's a summary of recent actions:\n\n");

            for (int i = 0; i < entries.Count; i++)
            {
                sb.Append(i + 1)
                  .Append(". ")
                  .Append(entries[i].Action)
                  .Append("  (")
                  .Append(entries[i].Time.ToString("HH:mm"))
                  .Append(")\n");
            }

            return sb.ToString().TrimEnd();
            // End of get summary method
        }

        // End of class
    }
    // End of namespace
}