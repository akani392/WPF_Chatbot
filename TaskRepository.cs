using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace WPF_Chatbot
{
    //start of namspace
    public class TaskRepository
    {
        //start of class

        //start of class

        string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=cybersecurity_chatbot;Integrated Security=True;";

        //puts in new table and return auto generates TaskID
        public int AddTask(string username, string title, string description, DateTime? reminderDate)
        {
            // start of add task method
            using (SqlConnection conn = new SqlConnection(connectionString) )
            {
                conn.Open();

                string query = "INSERT INTO tasks (username, Title, Decription, ReminderDate IsCompleted, CreatedDate) "
                    + "Output Inserted.TaskId"
                    + "@username, @title, @description, @reminderDate, 0, @createdDate);";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@reminderDate", reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@createdDate", DateTime.Now);

                // OUTPUT INSERTED.TaskId hands back the new row's ID directly

                return (int)cmd.ExecuteScalar();
            }
            // End of add task method
        }

        // Returns every task belonging to this user, oldest first
        public List<TaskItem> GetAllTasks(string username)
        {
            // Start of get all tasks method
            List<TaskItem> tasks = new List<TaskItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT TaskId, Username, Title, Description, ReminderDate, IsCompleted, CreatedDate " +
                               "FROM tasks WHERE Username = @username ORDER BY TaskId;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TaskItem task = new TaskItem();
                    task.TaskId = reader.GetInt32(reader.GetOrdinal("TaskId"));
                    task.Username = reader.GetString(reader.GetOrdinal("Username"));
                    task.Title = reader.GetString(reader.GetOrdinal("Title"));
                    task.Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description"));
                    task.ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ReminderDate"));
                    task.IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted"));
                    task.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));

                    tasks.Add(task);
                }
            }

            return tasks;
            // End of get all tasks method
        }

        // Deletes a task by id, scoped to the given user so people can't delete each other's tasks
        public bool DeleteTask(string username, int taskId)
        {
            // Start of delete task method
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "DELETE FROM tasks WHERE TaskId = @taskId AND Username = @username;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@taskId", taskId);
                cmd.Parameters.AddWithValue("@username", username);

                return cmd.ExecuteNonQuery() > 0;
            }
            // End of delete task method
        }

        // Marks a task as completed
        public bool MarkTaskCompleted(string username, int taskId)
        {
            // Start of mark task completed method
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "UPDATE tasks SET IsCompleted = 1 WHERE TaskId = @taskId AND Username = @username;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@taskId", taskId);
                cmd.Parameters.AddWithValue("@username", username);

                return cmd.ExecuteNonQuery() > 0;
            }


            // End of mark task completed method
        }

        // End of class
    }


    // End of namespace
}
