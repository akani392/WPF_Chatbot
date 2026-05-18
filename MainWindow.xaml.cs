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


        //class-level variables
        string username = "";
        Random random = new Random();

        //start of dictinary for chatbot responses
        Dictionary<string, string[]> chatbotRespones = new Dictionary<string, string[]>()
        {
            //start of dictinary for chatbot responses

            {
                "phishing",
                new string []
                {
                    "Phishing is a form of social engineering where attackers deceive users into divulging sensitive data.",
                    "Attackers use 'lures' like fake emails to trick you into clicking malicious links.",
                    "Always check the sender's email address; phishing often uses 'look-alike' domains.",
                    "Phishing is the most common way hackers gain initial access to a network.",
                    "If an email creates a sense of extreme urgency or fear, it's likely a phishing attempt."
                }
            },

            {
                "malware",
                new string []
                {
                    "Malware is software designed to harm or exploit any programmable device or network.",
                    "Common types of malware include viruses, worms, trojans, ransomware, and spyware.",
                    "Malware can steal sensitive information, damage files, or even take control of your device.",
                    "Always keep your software updated to protect against known malware vulnerabilities.",
                    "Using reputable antivirus software can help detect and remove malware from your system."
                }
            },

            {
                "password",
                new string []
                {
                    "A password is a secret word, phrase, or string of characters that a user must enter to gain access to a computer system, website, application, or digital account. It acts as a digital key to verify the user's identity.",
                    "In computer security, a password is a form of knowledge-based authentication (something you know). It is a shared secret between a user and a system used to authenticate a claimed identity, protecting sensitive data and restricted resources from unauthorized access.",
                    "Technically, a password is a variable-length string of alphanumeric and special characters that is typically passed through a cryptographic hash function before being stored in a database. During authentication, the system hashes the entered password and compares it to the stored hash value to grant or deny access without storing the actual plaintext characters.",
                    "An IT-centric definition views a password as a fundamental access control mechanism. It establishes accountability within a network by linking specific digital actions to a unique user profile or account.",
                    "Historically, a password (or watchword) is a pre-arranged secret word or phrase spoken to a guard or sentry to prove loyalty or membership, allowing safe passage through a secure physical checkpoint"

                }
            },

            {
                "firewall",
                new string []
                {
                    "A firewall is a digital security barrier that protects a private network or device from unauthorized access. Like a security guard at the entrance of a building, it checks the incoming and outgoing digital traffic and decides what is safe to let in and what should be blocked.",
                    "In network security, a firewall is an access-control mechanism positioned between a trusted internal network (like a company's private network) and an untrusted external network (like the internet). It analyzes data packets based on a predetermined set of security rules to prevent malicious traffic from penetrating the network.",
                    "Technically, a firewall is a software or hardware component that inspects network traffic at specific layers of the OSI model. It examines the metadata of data packets—such as source and destination IP addresses, protocols (TCP/UDP), and port numbers—and filters them by matching them against an Access Control List (ACL).",
                    "From an IT architecture perspective, a firewall is a foundational perimeter defense system that can be deployed as dedicated hardware appliances, cloud-native services (Firewall-as-a-Service), or software agents on individual hosts. It is designed to enforce organizational security policies, segment internal networks, and log traffic patterns for audit and compliance purposes.",
                    "Originally, a firewall is a physical, fireproof wall built within a building or vehicle (such as a car or ship) designed to prevent the spread of a physical fire from one compartment to another. The tech industry adopted this term to describe digital barriers meant to contain and stop cyber threats from spreading."
                }
            },



            //end of dictinary for chatbot responses
        };

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
