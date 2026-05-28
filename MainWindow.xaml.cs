using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Media;

namespace WPF_Chatbot
{
    // Start of namespace
    public partial class MainWindow : Window
    {

        // Start of class
        

        //voice recording
        public void PlayVoiceGreeting()
        {
            //start voice recording

            try
            {
                SoundPlayer player = new SoundPlayer(@"recording/Recording2.wav");
                player.Load();
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing sound: " + ex.Message);
            }

            //end of voice recording
        }


        // Class-level variables
        string username = "";
        Random random = new Random();
        string memoryFile = "memory.txt";  
        string currentTopic = "";

        // Colour for chat messages
        readonly Brush botColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));  // blue  – ChatBot
        readonly Brush userColor = new SolidColorBrush(Color.FromRgb(244, 67, 54));  // red   – user

        
        Dictionary<string, string> activeMenu = null;

        // Keyword lookup – maps topic names to trigger words
        Dictionary<string, string[]> topicKeyWords = new Dictionary<string, string[]>()
        {
            { 
                "phishing",
                new string[] 
                { "phishing", "email", "lure"}
            },
            { "malware", 
               new string[] 
               { "malware",  "virus", "trojan","ransomware" } 
            },
            { "password", 
               new string[] 
               { "password", "credentials", "passphrase"   } },
            { "firewall", 
               new string[] 
               { "firewall", "network barrier", "acl"      } },
            { "scam",     
               new string[] 
               { "scam", "fraud", "social engineering"     } }
        };

        // Main response dictionary
        // Each topic has an array of researched responses; one is picked at random
        Dictionary<string, string[]> chatbotResponses = new Dictionary<string, string[]>()
        {
            {
                "phishing",
                new string[]
                {
                    "Phishing is a form of social engineering where attackers deceive users into divulging sensitive data.",
                    "Attackers use 'lures' like fake emails to trick you into clicking malicious links.",
                    "Always check the sender's email address; phishing often uses 'look-alike' domains.",
                    "Phishing is the most common way hackers gain initial access to a network.",
                    "If an email creates a sense of extreme urgency or fear, it is likely a phishing attempt."
                }
            },
            {
                "malware",
                new string[]
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
                new string[]
                {
                    "A password is a secret word or string of characters a user must enter to gain access to a system, website, or digital account. It acts as a digital key to verify the user's identity.",
                    "In computer security, a password is a form of knowledge-based authentication (something you know). It is a shared secret between a user and a system used to protect sensitive data from unauthorized access.",
                    "Technically, a password is passed through a cryptographic hash function before being stored in a database. During authentication the system hashes the entered password and compares it to the stored hash to grant or deny access.",
                    "An IT-centric view treats a password as a fundamental access control mechanism that links specific digital actions to a unique user profile or account within a network.",
                    "Historically, a password (or watchword) was a pre-arranged secret phrase spoken to a guard to prove loyalty, allowing safe passage through a secure checkpoint."
                }
            },
            {
                "firewall",
                new string[]
                {
                    "A firewall is a digital security barrier that protects a private network or device from unauthorized access, much like a security guard checking who can enter a building.",
                    "In network security, a firewall is positioned between a trusted internal network and an untrusted external one. It analyzes data packets against security rules to prevent malicious traffic.",
                    "Technically, a firewall inspects network traffic at specific OSI layers, examining source/destination IP addresses, protocols (TCP/UDP), and port numbers, then filters them against an Access Control List (ACL).",
                    "From an IT architecture perspective, a firewall is a foundational perimeter defense that can be hardware, cloud-native (Firewall-as-a-Service), or a software agent. It enforces security policies and logs traffic for audit purposes.",
                    "The original 'firewall' was a physical, fireproof wall built in a building or vehicle to stop the spread of fire. The tech industry borrowed the term to describe barriers that contain cyber threats."
                }
            },
            {
                "scam",
                new string[]
                {
                    "In cybersecurity, a scam is a form of social engineering that exploits trust, fear, urgency, or greed to trick people into breaking normal security procedures, rather than relying on technical exploits.",
                    "A scam is a malicious campaign delivered via email (phishing), SMS (smishing), voice calls (vishing), or social media, where attackers impersonate legitimate entities to steal credentials or deliver malware.",
                    "From an identity-security perspective, a scam is a credential-harvesting trap. Spoofed websites mirror real brands to steal usernames, passwords, and even two-factor authentication codes.",
                    "Within cybercrime operations, a scam can be a financial fraud scheme such as Business Email Compromise (BEC), where attackers deceive employees into routing legitimate payments into attacker-controlled accounts.",
                    "From a technical delivery perspective, a scam wraps malicious payloads in deceptive narratives — fake invoices, urgent software updates — convincing users to bypass security warnings themselves."
                }
            }
        };

        // Sub-topic deep-dive responses
        Dictionary<string, string> subTopicResponses = new Dictionary<string, string>()
        {
            // Malware sub-topics
            { "malware types",
              "Types of Malware:" +
                "\n\nRansomware: Like a digital kidnapper, ransomware encrypts the victim's files and demands payment (usually in cryptocurrency) for the decryption key." +
                "\n\nSpyware: A hidden observer that runs quietly in the background, recording keystrokes to steal bank logins and passwords." +
                "\n\nKey difference: Ransomware disrupts to force a quick payout; spyware uses stealth to gather data over time." },
            { "malware detect",
              "How to Detect Malware:\n\n- Slow device performance: Malware secretly consumes CPU and memory.\n- Unexpected pop-ups or browser redirects: A sign of adware or spyware.\n- Antivirus suddenly disabled: Malware often disables security tools.\n- Unusual network activity: Malware may be sending your data to a remote attacker.\n- Files missing or encrypted: A strong sign of ransomware." },
            { "malware prevent",
              "How to Prevent Malware:\n\n- Keep software updated: Updates patch known vulnerabilities.\n- Use reputable antivirus software.\n- Avoid suspicious links and attachments.\n- Download only from trusted sources.\n- Back up your data regularly." },
            // Phishing sub-topics
            { "phishing types",
              "Types of Phishing:" +
                "\n\nSpear Phishing: Targeted at a specific individual using personalised details." +
                "\n\nSmishing: Phishing via SMS text messages." +
                "\n\nVishing: Voice phishing over phone calls impersonating banks or government." +
                "\n\nWhaling: Targeted at senior executives or high-value targets." },
            { "phishing detect",
              "How to Detect Phishing:" +
                "\n\n- Check sender email addresses for subtle misspellings." +
                "\n- Be suspicious of urgent language demanding immediate action." +
                "\n- Hover over links to preview the destination URL." +
                "\n- Look for generic greetings like 'Dear Customer'." +
                "\n- Watch for poor grammar or unusual formatting." },
            { "phishing prevent",
              "How to Prevent Phishing:" +
                "\n\n- Never click links in unsolicited emails." +
                "\n- Enable multi-factor authentication (MFA) on all accounts." +
                "\n- Use an email spam filter." +
                "\n- Report suspicious emails to your IT department." +
                "\n- Train yourself regularly to recognise phishing red flags." },
            // Password sub-topics
            { "password types",
              "Types of Passwords and Authentication:" +
                "\n\nPassphrase: A sequence of random words that is long and memorable." +
                "\n\nOne-Time Password (OTP): A temporary code valid for a single login." +
                "\n\nBiometric: Uses a physical trait (fingerprint, face ID)." +
                "\n\nHardware Key: A physical device like a YubiKey." },
            { "password detect",
              "How to Detect a Compromised Password:" +
                "\n\n- Unexpected login notifications or account activity." +
                "\n- Security alerts about credentials appearing in a data breach." +
                "\n- Use 'Have I Been Pwned' (haveibeenpwned.com) to check your email." +
                "\n- Sudden logouts or 'someone else is using your account' messages." },
            { "password prevent",
              "How to Keep Passwords Secure:" +
                "\n\n- Use a unique, strong password for every account (12+ characters)." +
                "\n- Use a password manager." +
                "\n- Enable multi-factor authentication." +
                "\n- Never share passwords via email or chat." +
                "\n- Change passwords immediately if you suspect a breach." },
            // Firewall sub-topics
            { "firewall types",
              "Types of Firewalls:" +
                "\n\nPacket-Filtering: Inspects individual packets against rules." +
                "\n\nStateful: Tracks active connections and session state." +
                "\n\nApplication Layer (Proxy): Deep content filtering at the application level." +
                "\n\nNext-Generation (NGFW): Combines IPS, SSL inspection, and application awareness." },
            { "firewall detect",
              "How to Know If Your Firewall Is Working:" +
                "\n\n- Check firewall logs for blocked or suspicious attempts." +
                "\n- Use an online port scanner to confirm unwanted ports are closed." +
                "\n- Monitor alerts from your firewall's management console." +
                "\n- Run periodic penetration tests to verify rules are enforced." },
            { "firewall prevent",
              "Firewall Best Practices:" +
                "\n\n- Keep firmware and rule sets up to date." +
                "\n- Block everything by default — allow only what is needed." +
                "\n- Segment your network so a breach cannot spread freely." +
                "\n- Audit firewall rules regularly to remove outdated entries." +
                "\n- Combine with an IDS/IPS for layered defence." },
            // Scam sub-topics
            { "scam types",
              "Types of Scams:" +
                "\n\nAdvance-Fee Scam: Promises a large reward for a small upfront payment." +
                "\n\nTech Support Scam: Fake alerts trick you into paying for unnecessary 'fixes'." +
                "\n\nRomance Scam: Attackers build a fake relationship before requesting money." +
                "\n\nBusiness Email Compromise (BEC): Impersonates executives to redirect payments." },
            { "scam detect",
              "How to Detect a Scam:" +
                "\n\n- Unsolicited contact asking for money or personal info is a red flag." +
                "\n- Verify unexpected requests through official channels." +
                "\n- Be sceptical of deals that seem too good to be true." +
                "\n- Watch for pressure tactics: 'Act now or lose your account!'" +
                "\n- Legitimate organisations never ask for gift cards or wire transfers." },
            { "scam prevent",
              "How to Avoid Scams:" +
                "\n\n- Never send money to someone you have not verified." +
                "\n- Enable spam filters on email and block suspicious numbers." +
                "\n- Educate yourself and others about common scam tactics." +
                "\n- Report scams to your national consumer protection authority." +
                "\n- Slow down — scammers rely on urgency to bypass your judgement." }
        };

        // Topic ask counter
        // Tracks how many times each topic has been asked so we can offer deep-dives
        Dictionary<string, int> topicAskCount = new Dictionary<string, int>();

      
        public MainWindow()
        {
            // Start of constructor
            InitializeComponent();
            // End of constructor
        }

        // Start chatbot – hide logo grid, show username entry grid
        private void start_Chatbot(object sender, RoutedEventArgs e)
        {
            logo_grid.Visibility = Visibility.Hidden;
            username_grid.Visibility = Visibility.Visible;

            
        }

        // Allow pressing Enter in the name box to submit
        private void user_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                submit_names(sender, e);
        }

        // Submit username – validate input then move to the chat screen
        private void submit_names(object sender, RoutedEventArgs e)
        {
            // Start of submit names method
            username = user_name.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter a username.");
                return;
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z]+$"))
            {
                ShowError("Please enter a valid name (letters only).");
                return;
            }

            // Clear any previous validation error
            error_text.Visibility = Visibility.Collapsed;
            username_grid.Visibility = Visibility.Hidden;
            chats_grid.Visibility = Visibility.Visible;

            PlayVoiceGreeting();

            AddBotMessage("Welcome, " + username + "! Ask me about phishing, malware, passwords, firewalls, or scams.");
            // End of submit names method
        }

        // Allow pressing Enter in the chat box to send
        private void questions_textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                send_question(sender, e);
        }

        // Send button – read input, display it, then get and display a bot reply
        private void send_question(object sender, RoutedEventArgs e)
        {
            // Start of send question method
            string message = questions_textbox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(message))
                return;

            // Show what the user typed in red
            AddUserMessage(message);

            //Numbered menu: if a menu is active check if the user typed 1, 2, or 3
            if (activeMenu != null)
            {
                string subKey;
                if (activeMenu.TryGetValue(message, out subKey))
                {
                    // Valid choice – show the deep-dive and close the menu
                    activeMenu = null;
                    AddBotMessage(subTopicResponses[subKey]);
                    questions_textbox.Clear();
                    return;
                }
                else
                {
                    // Not 1/2/3 – close the menu and fall through to normal handling
                    activeMenu = null;
                    AddBotMessage("That wasn't a valid option. Continuing with your question...");
                }
            }

            // Memory: save favourite topic 
            if (message.Contains("interested in"))
            {
                SaveToFile(message);
                questions_textbox.Clear();
                return;
            }

            // Memory: recall favourite topic
            if (message.Contains("favorite topic") || message.Contains("favourite topic"))
            {
                if (File.Exists(memoryFile))
                {
                    string savedTopic = File.ReadAllText(memoryFile);
                    AddBotMessage("Your favourite topic is: " + savedTopic);
                }
                else
                {
                    AddBotMessage("I don't know your favourite topic yet.");
                }

                questions_textbox.Clear();
                return;
            }

            // Get bot reply and display it
            string botReply = GetChatbotResponse(message);
            AddBotMessage(botReply);

            questions_textbox.Clear();
            // End of send question method
        }


        // Main entry point – ties together topic detection, sentiment, and follow-up handling
        public string GetChatbotResponse(string message)
        {
            // Start of chatbot response method
            string sentiment = DetectSentiment(message);
            bool moreInfo = IsFollowUp(message);
            string topic = DetectTopic(message);

            // If no topic found but user is asking for more info, reuse the last topic
            if (string.IsNullOrEmpty(topic) && moreInfo && !string.IsNullOrEmpty(currentTopic))
                topic = currentTopic;

            // If a topic was identified, build and return a full response
            if (!string.IsNullOrEmpty(topic))
            {
                currentTopic = topic;
                return BuildResponse(topic, sentiment, moreInfo);
            }

            // Check for sub-topic keywords typed directly (still supported as fallback)
            string subReply = GetSubTopicResponse(message);
            if (!subReply.StartsWith("Sorry"))
                return subReply;

            // Sentiment-only reply when no topic is detected
            if (!string.IsNullOrEmpty(sentiment))
                return GetSentimentSupport(sentiment) + " Feel free to ask me about phishing, malware, passwords, firewalls, or scams.";

            return "I am built to answer cybersecurity questions. Try asking about: phishing, malware, passwords, firewalls, or scams.";
            // End of chatbot response method
        }

        // Builds a response and shows a numbered menu after 3 asks on the same topic
        public string BuildResponse(string topic, string sentiment, bool moreInfo)
        {
            // Start of build response method

            // Track how many times this topic has been asked
            if (!topicAskCount.ContainsKey(topic))
                topicAskCount[topic] = 0;

            topicAskCount[topic]++;

            // After 3 questions on the same topic show a numbered deep-dive menu
            if (topicAskCount[topic] == 3)
            {
                // Store the menu so the next reply of "1", "2", or "3" is handled in send_question
                activeMenu = new Dictionary<string, string>();
                activeMenu["1"] = topic + " types";
                activeMenu["2"] = topic + " detect";
                activeMenu["3"] = topic + " prevent";

                return "You have asked a lot about " + topic + "! Would you like to go deeper?\n\n" +
                       "  Reply 1  -  Types of " + topic + "\n" +
                       "  Reply 2  -  How to detect " + topic + "\n" +
                       "  Reply 3  -  How to prevent " + topic;
            }

            // Pick a random response for this topic
            string[] responses = chatbotResponses[topic];
            int index = random.Next(responses.Length);
            string response = responses[index];

            // Prepend an empathetic opener if a sentiment was detected
            string support = GetSentimentSupport(sentiment);
            return string.IsNullOrEmpty(support) ? response : support + "\n\n" + response;

            // End of build response method
        }

        // Returns a deep-dive answer for sub-topic keys (also a fallback if typed directly)
        public string GetSubTopicResponse(string message)
        {
            // Start of sub-topic response method
            foreach (var entry in subTopicResponses)
            {
                if (message.Contains(entry.Key))
                    return entry.Value;
            }

            return "Sorry, I don't understand that. Try asking about: phishing, malware, passwords, firewalls, or scams.";
            // End of sub-topic response method
        }

        // Scans the message for known topic keywords and returns the matching topic name
        public string DetectTopic(string message)
        {
            // Start of detect topic method
            foreach (var topic in topicKeyWords)
            {
                if (topic.Value.Any(word => message.Contains(word)))
                    return topic.Key;
            }
            return "";
            // End of detect topic method
        }

        // Returns "positive", "negative", or "" based on emotional keywords in the message
        public string DetectSentiment(string message)
        {
            // Start of detect sentiment method
            string[] negativeWords = { "worried", "unsure", "anxious", "nervous", "afraid", "scared", "confused" };
            string[] positiveWords = { "happy", "excited", "confident", "optimistic", "relieved", "great" };

            if (negativeWords.Any(w => message.Contains(w))) return "negative";
            if (positiveWords.Any(w => message.Contains(w))) return "positive";

            return "";
            // End of detect sentiment method
        }

        // Returns true if the message is asking for more detail on the current topic
        public bool IsFollowUp(string message)
        {
            // Start of is follow up method
            string[] followUpPhrases =
            {
                "explain more", "tell me more", "can you elaborate",
                "i want to know more", "give me more details", "more info"
            };

            return followUpPhrases.Any(p => message.Contains(p));
            // End of is follow up method
        }

        // Returns a short empathetic opener based on the detected sentiment
        public string GetSentimentSupport(string sentiment)
        {
            // Start of get sentiment support method
            switch (sentiment)
            {
                case "negative":
                    return "I understand this topic can feel overwhelming — you are not alone.";
                case "positive":
                    return "Great to hear you are feeling confident about this!";
                default:
                    return "";
            }
            // End of get sentiment support method
        }


        // Saves the user's stated favourite topic to a local text file for later recall
        public void SaveToFile(string message)
        {
            // Start of save to file method
            string topic = message.Replace("i am interested in", "").Trim();

            if (string.IsNullOrWhiteSpace(topic))
            {
                AddBotMessage("Please tell me what you are interested in after \"I am interested in\".");
                return;
            }

            File.WriteAllText(memoryFile, topic);
            AddBotMessage("Got it! I will remember that your favourite topic is \"" + topic + "\".");
            // End of save to file method
        }

        // Adds a bot message with a blue sender label to the RichTextBox
        private void AddBotMessage(string text)
        {
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 2, 0, 2);

            // Blue bold "ChatBot: " label
            Run label = new Run("ChatBot: ");
            label.Foreground = botColor;
            label.FontWeight = FontWeights.Bold;
            para.Inlines.Add(label);

            // White message body
            Run body = new Run(text);
            body.Foreground = Brushes.White;
            para.Inlines.Add(body);

            chat_rtb.Document.Blocks.Add(para);
            chat_rtb.ScrollToEnd();
        }

        // Adds a user message with a red sender label to the RichTextBox
        private void AddUserMessage(string text)
        {
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 2, 0, 2);

            // Red bold "Username: " label
            Run label = new Run(username + ": ");
            label.Foreground = userColor;
            label.FontWeight = FontWeights.Bold;
            para.Inlines.Add(label);

            // White message body
            Run body = new Run(text);
            body.Foreground = Brushes.White;
            para.Inlines.Add(body);

            chat_rtb.Document.Blocks.Add(para);
        }

        // Shows an inline validation error under the username text box
        private void ShowError(string message)
        {
            error_text.Text = message;
            error_text.Visibility = Visibility.Visible;
        }

        // End of class
    }
    // End of namespace
}