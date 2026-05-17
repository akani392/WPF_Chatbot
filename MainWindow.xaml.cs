using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
  
namespace WPF_Chatbot
{
 // Start of namspace
    public partial class MainWindow : Window
    {
        //start of class
        public MainWindow()
        {
            //start of constructor

            InitializeComponent();

            //end of constructor
        }

        private void start_Chatbot (object sender, RoutedEventArgs e)
        {
            // start of start chatboot method

            //set the logo page to hidden
            logo_grid.Visibility = Visibility.Hidden;

            //set username grid to visable
            username_grid.Visibility = Visibility.Visible;

            //end of start chatbot method
        }

        private void submit_names(object sender, RoutedEventArgs e)
        {
            // start of submit name method

            //get the username from the text box
            string username = user_name.Text.ToString();

            //check is username is empty
            if(username != "")
            {
                //start of if

                //show message
                MessageBox.Show(" wellcome " + user_name);

                //set the username grid hidden and chats visable
                username_grid.Visibility = Visibility.Hidden;
                chats_grid.Visibility = Visibility.Visible;

                //add the message to the chats list
                chats_list.Items.Add("ChatBot: hello " + user_name);

                //end of if
            }
            else
            {
                // start of else
                MessageBox.Show("Please enter a username");
            }

        }
        private void send_question(object sender, RoutedEventArgs e)
        {

        }

       



        //end of class
    }

    // End of namespace
}
