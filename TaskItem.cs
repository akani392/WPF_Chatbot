using System;

namespace WPF_Chatbot
{
    // Start of namespace

    public class TaskItem
    {

        // Start of class

        public int TaskId { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }   
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }



        // End of class
    }



    // End of namespace
}