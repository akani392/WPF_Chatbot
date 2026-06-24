using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WPF_Chatbot
{
    // Start of namespace

    public class TaskAssistant
    {
        // Start of class

        TaskRepository repository = new TaskRepository();

        // State flags - only one is true at a time, same idea as activeMenu in MainWindow
        bool awaitingMenuChoice = false;  
        bool awaitingTaskTitle = false;  
        bool awaitingReminderResponse = false;  

        // Holds the task being built across multiple turns of conversation
        TaskItem pendingTask = null;

        // The menu text shown to the user - kept as a constant so it is easy to update
        const string TaskMenuText =
            "Task Assistant - What would you like to do?\n\n" +
            "  Reply 1  -  Add a new task\n" +
            "  Reply 2  -  View my tasks\n" +
            "  Reply 3  -  Complete a task\n" +
            "  Reply 4  -  Delete a task";
        public bool TryHandle(string message, string username, out string reply)
        {
            // Start of try handle method
            reply = "";

            // Priority 1: bot is waiting for the user to pick a menu option
            if (awaitingMenuChoice)
            {
                reply = HandleMenuChoice(message, username);
                return true;
            }

            // Priority 2: bot asked for the task title - next message IS the title
            if (awaitingTaskTitle)
            {
                reply = HandleTitleReceived(message);
                return true;
            }

            // Priority 3: bot asked "would you like a reminder?" - next message IS the answer
            if (awaitingReminderResponse)
            {
                reply = HandleReminderResponse(message, username);
                return true;
            }

            // Show the task menu whenever the user mentions "task" or "tasks"
            if (message.Contains("task") || message.Contains("tasks"))
            {
                awaitingMenuChoice = true;
                reply = TaskMenuText;
                return true;
            }

            return false;
            // End of try handle method
        }

        // Routes the numbered menu choice to the right handler
        string HandleMenuChoice(string message, string username)
        {
            // Start of handle menu choice method
            awaitingMenuChoice = false;

            switch (message.Trim())
            {
                case "1":
                    // Ask for the task title
                    pendingTask = new TaskItem();
                    awaitingTaskTitle = true;
                    return "Sure! What is the title of the task you would like to add?\n" +
                           "(e.g. \"Enable two-factor authentication\" or \"Review account privacy settings\")";

                case "2":
                    return HandleViewTasks(username);

                case "3":
                    return HandleViewTasks(username) +
                           "\n\nType the task number you want to mark as completed, e.g. \"complete 2\".";

                case "4":
                    return HandleViewTasks(username) +
                           "\n\nType the task number you want to delete, e.g. \"delete 2\".";

                default:
                    // Not a valid option - re-show the menu so the user can try again
                    awaitingMenuChoice = true;
                    return "Please reply with 1, 2, 3, or 4.\n\n" + TaskMenuText;
            }
            // End of handle menu choice method
        }

        // Called when awaitingTaskTitle is true - the whole message IS the task title
        string HandleTitleReceived(string message)
        {
            // Start of handle title received method
            awaitingTaskTitle = false;
            string title = message.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                awaitingTaskTitle = true; 
                return "Please enter a valid task title.";
            }

            return BuildPendingTaskAndAskReminder(title);
            // End of handle title received method
        }

        // Shared helper: stores the pending task and asks about the reminder
        string BuildPendingTaskAndAskReminder(string title)
        {
            // Start of build pending task and ask reminder method
            title = CapitaliseFirst(title);
            string description = "Task added: \"" + title + " to keep your account secure.\"";

            pendingTask = new TaskItem();
            pendingTask.Title = title;
            pendingTask.Description = description;
            awaitingReminderResponse = true;

            return description + "\n\nWould you like a reminder?\n" +
                   "  Yes - type \"yes, remind me in X days\"\n" +
                   "  No  - type \"no\"";
            // End of build pending task and ask reminder method
        }

        // Handles the user's answer to "Would you like a reminder?"
        string HandleReminderResponse(string message, string username)
        {
            // Start of handle reminder response method
            awaitingReminderResponse = false;
            DateTime? reminderDate = null;

            if (message.Contains("yes"))
            {
                Match match = Regex.Match(message, @"(\d+)\s*day");
                int days = match.Success ? int.Parse(match.Groups[1].Value) : 7; 
                reminderDate = DateTime.Now.AddDays(days);
            }

            repository.AddTask(username, pendingTask.Title, pendingTask.Description, reminderDate);
            pendingTask = null;

            string saved = reminderDate.HasValue
                ? "Got it! I'll remind you on " + reminderDate.Value.ToString("dd MMM yyyy") + "."
                : "No problem, I won't set a reminder. Your task has been saved.";

            // After saving, offer to go back to the menu
            return saved + "\n\nType \"tasks\" at any time to manage your tasks.";
            // End of handle reminder response method
        }

        // Lists every task for this user, numbered so the user can reference them
        string HandleViewTasks(string username)
        {
            // Start of handle view tasks method
            var tasks = repository.GetAllTasks(username);

            if (tasks.Count == 0)
                return "You don't have any tasks yet.\n\nType \"tasks\" and choose option 1 to add one.";

            StringBuilder sb = new StringBuilder("Your tasks:\n\n");

            foreach (var task in tasks)
            {
                // Show the task number, title, and status
                sb.Append(task.TaskId).Append(". ").Append(task.Title);

                if (task.IsCompleted)
                    sb.Append("  ✓ (completed)");

                if (task.ReminderDate.HasValue)
                    sb.Append("  — reminder: ").Append(task.ReminderDate.Value.ToString("dd MMM yyyy"));

                sb.Append("\n");
            }

            return sb.ToString().TrimEnd();
            // End of handle view tasks method
        }

        // Marks the task number the user mentioned as completed
        // Supports "complete 2" or "complete task 2"
        public bool TryCompleteTask(string message, string username, out string reply)
        {
            // Start of try complete task method
            reply = "";

            if (!Regex.IsMatch(message, @"^complete"))
                return false;

            int? id = ExtractTaskId(message);

            if (id == null)
            {
                reply = "Please include the task number, e.g. \"complete 2\".";
                return true;
            }

            bool success = repository.MarkTaskCompleted(username, id.Value);
            reply = success
                ? "Nicely done! Task " + id + " has been marked as completed.\n\nType \"tasks\" to see your updated list."
                : "I couldn't find task " + id + ". Type \"tasks\" and choose option 2 to see your list.";

            return true;
            // End of try complete task method
        }

        public bool TryDeleteTask(string message, string username, out string reply)
        {
            // Start of try delete task method
            reply = "";

            if (!Regex.IsMatch(message, @"^delete"))
                return false;

            int? id = ExtractTaskId(message);

            if (id == null)
            {
                reply = "Please include the task number, e.g. \"delete 2\".";
                return true;
            }

            bool success = repository.DeleteTask(username, id.Value);
            reply = success
                ? "Task " + id + " has been deleted.\n\nType \"tasks\" to see your updated list."
                : "I couldn't find task " + id + ". Type \"tasks\" and choose option 2 to see your list.";

            return true;
            // End of try delete task method
        }

        // Pulls the first number out of a message e.g. "complete 3" -> 3
        int? ExtractTaskId(string message)
        {
            // Start of extract task id method
            Match match = Regex.Match(message, @"\d+");
            return match.Success ? int.Parse(match.Value) : (int?)null;
            // End of extract task id method
        }

        
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