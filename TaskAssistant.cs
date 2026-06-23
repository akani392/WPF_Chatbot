using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WPF_Chatbot
{
    // Start of namespace
    public class TaskAssistant
    {
        // Start of class

        TaskRepository repository = new TaskRepository();

        // Holds the title/description while we wait for the user to answer
        // the "Would you like a reminder?" question - same idea as the
        // activeMenu field already used elsewhere in MainWindow.
        TaskItem pendingTask = null;
        bool awaitingReminderResponse = false;

        // Call this first from send_question(). Returns true if the message was
        // task-related and has already been handled (reply is filled in).
        // Returns false if the message has nothing to do with tasks, so
        // MainWindow can carry on with its normal topic/sentiment handling.
        public bool TryHandle(string message, string username, out string reply)
        {
            // Start of try handle method
            reply = "";

            // Highest priority: we just asked "Would you like a reminder?"
            // so this message is the answer to that question, not a new command.
            if (awaitingReminderResponse)
            {
                reply = HandleReminderResponse(message, username);
                return true;
            }

            if (Regex.IsMatch(message, @"^add task"))
            {
                reply = HandleAddTask(message);
                return true;
            }

            if (message.Contains("view tasks") || message.Contains("show tasks") || message.Contains("my tasks"))
            {
                reply = HandleViewTasks(username);
                return true;
            }

            if (Regex.IsMatch(message, @"^delete task"))
            {
                reply = HandleDeleteTask(message, username);
                return true;
            }

            if (Regex.IsMatch(message, @"^complete task"))
            {
                reply = HandleCompleteTask(message, username);
                return true;
            }

            return false;
            // End of try handle method
        }

        // Parses something like "add task - review privacy settings"
        string HandleAddTask(string message)
        {
            // Start of handle add task method
            string title = Regex.Replace(message, @"^add task\s*-?\s*", "").Trim();

            if (string.IsNullOrWhiteSpace(title))
                return "Please tell me the task title, e.g. \"Add task - Enable two-factor authentication\".";

            title = CapitaliseFirst(title);
            string description = "Task added with the description \"" + title + " to keep your account secure.\"";

            pendingTask = new TaskItem();
            pendingTask.Title = title;
            pendingTask.Description = description;
            awaitingReminderResponse = true;

            return description + " Would you like a reminder?";
            // End of handle add task method
        }

        // Handles the user's answer to "Would you like a reminder?"
        // Supports replies like "yes, remind me in 3 days" or "no"
        string HandleReminderResponse(string message, string username)
        {
            // Start of handle reminder response method
            awaitingReminderResponse = false;
            DateTime? reminderDate = null;

            if (message.Contains("yes"))
            {
                Match match = Regex.Match(message, @"(\d+)\s*day");
                int days = match.Success ? int.Parse(match.Groups[1].Value) : 7; // default to 7 days if no number given
                reminderDate = DateTime.Now.AddDays(days);
            }

            repository.AddTask(username, pendingTask.Title, pendingTask.Description, reminderDate);
            pendingTask = null;

            if (reminderDate.HasValue)
                return "Got it! I'll remind you on " + reminderDate.Value.ToString("dd MMM yyyy") + ".";
            else
                return "No problem, I won't set a reminder. Your task has been saved.";
            // End of handle reminder response method
        }

        // Lists every task for this user, numbered so the user can reference them later
        string HandleViewTasks(string username)
        {
            // Start of handle view tasks method
            var tasks = repository.GetAllTasks(username);

            if (tasks.Count == 0)
                return "You don't have any tasks yet. Try \"Add task - Enable two-factor authentication\".";

            StringBuilder sb = new StringBuilder("Here are your tasks:\n\n");

            foreach (var task in tasks)
            {
                sb.Append(task.TaskId).Append(". ").Append(task.Title);

                if (task.IsCompleted)
                    sb.Append(" (completed)");

                if (task.ReminderDate.HasValue)
                    sb.Append(" — reminder set for ").Append(task.ReminderDate.Value.ToString("dd MMM yyyy"));

                sb.Append("\n");
            }

            sb.Append("\nType \"complete task [number]\" or \"delete task [number]\" to manage a task.");
            return sb.ToString();
            // End of handle view tasks method
        }

        // Deletes the task number the user mentioned
        string HandleDeleteTask(string message, string username)
        {
            // Start of handle delete task method
            int? id = ExtractTaskId(message);
            if (id == null)
                return "Please tell me which task number to delete, e.g. \"delete task 2\".";

            bool success = repository.DeleteTask(username, id.Value);

            if (success)
                return "Task " + id + " has been deleted.";
            else
                return "I couldn't find a task with that number.";
            // End of handle delete task method
        }

        // Marks the task number the user mentioned as completed
        string HandleCompleteTask(string message, string username)
        {
            // Start of handle complete task method
            int? id = ExtractTaskId(message);
            if (id == null)
                return "Please tell me which task number to mark as completed, e.g. \"complete task 2\".";

            bool success = repository.MarkTaskCompleted(username, id.Value);

            if (success)
                return "Nicely done! Task " + id + " has been marked as completed.";
            else
                return "I couldn't find a task with that number.";
            // End of handle complete task method
        }

        // Pulls the first number out of a message, e.g. "delete task 3" -> 3
        int? ExtractTaskId(string message)
        {
            // Start of extract task id method
            Match match = Regex.Match(message, @"\d+");
            return match.Success ? int.Parse(match.Value) : (int?)null;
            // End of extract task id method
        }

        // Capitalises the first letter of a task title for nicer display
        string CapitaliseFirst(string text)
        {
            // Start of capitalise first method
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
            // End of capitalise first method
        }

        // End of class
    }
    // End of namespace
}