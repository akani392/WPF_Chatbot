using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace WPF_Chatbot
{
    // Start of namespace
    public partial class MainWindow : Window
    {
        // Start of class

        // ── Class-level variables 
        string username = "";
        Random random = new Random();
        string memoryFile = "memory.txt";   
        string currentTopic = "";             

        // ── Keyword lookup – maps topic names to trigger words 
        // Used by DetectTopic() to figure out what the user is asking about
        Dictionary<string, string[]> topicKeyWords = new Dictionary<string, string[]>()
        {
            { "phishing", new string[] { "phishing", "email", "scam", "fraud"        } },
            { "malware",  new string[] { "malware",  "virus", "trojan", "ransomware" } },
            { "password", new string[] { "password", "credentials", "passphrase"     } },
            { "firewall", new string[] { "firewall", "network barrier", "acl"        } },
            { "scam",     new string[] { "scam", "fraud", "social engineering"       } }
        };

        // ── Main response dictionary 
        // Each topic has an array of researched responses; one is picked at random
        Dictionary<string, string[]> chatbotResponses = new Dictionary<string, string[]>()
        {
            // ── Phishing 
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

            // ── Malware 
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

            // ── Password 
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

            // Firewa;;
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

            // ── Scam 
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

        // ── Sub-topic deep-dive responses 
        // Triggered when the user types e.g. "malware types" / "malware detect" / "malware prevent"
        Dictionary<string, string> subTopicResponses = new Dictionary<string, string>()
        {
            {
                "malware types",
                "Types of Malware:\n\n" +
                "Ransomware: Like a digital kidnapper, ransomware encrypts the victim's files and demands payment " +
                "(usually in cryptocurrency) for the decryption key. If the victim does not pay, files may be " +
                "deleted permanently or leaked online.\n\n" +
                "Spyware: A hidden observer that runs quietly in the background. A common variant is a keylogger, " +
                "which records every keystroke to steal bank logins, passwords, and credit card numbers without " +
                "the user realizing.\n\n" +
                "Key difference: Ransomware disrupts to force a quick payout; spyware uses stealth to gather data over time."
            },
            {
                "malware detect",
                "How to Detect Malware:\n\n" +
                "- Slow device performance: Malware secretly consumes CPU and memory in the background.\n" +
                "- Unexpected pop-ups or browser redirects: A sign of adware or spyware.\n" +
                "- Antivirus suddenly disabled: Malware often disables security tools to avoid detection.\n" +
                "- Unusual network activity: Malware may be sending your data to a remote attacker.\n" +
                "- Files missing or encrypted: A strong sign of ransomware infection."
            },
            {
                "malware prevent",
                "How to Prevent Malware:\n\n" +
                "- Keep software updated: Updates patch known vulnerabilities that malware exploits.\n" +
                "- Use reputable antivirus software: It can detect and block known threats.\n" +
                "- Avoid suspicious links and attachments: Most malware arrives via phishing emails.\n" +
                "- Download only from trusted sources: Fake software installers are a common delivery method.\n" +
                "- Back up your data regularly: If ransomware strikes, backups let you recover without paying."
            }
        };

        
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

        // Submit username – validate input then move to the chat screen
        private void submit_names(object sender, RoutedEventArgs e)
        {
            // Start of submit names method
            username = user_name.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.");
                return;
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("Please enter a valid name (letters only).");
                return;
            }

            // Hide username grid and show chat grid
            username_grid.Visibility = Visibility.Hidden;
            chats_grid.Visibility = Visibility.Visible;

            // Display welcome message in the chat list
            chats_list.Items.Add($"ChatBot: Welcome, {username}! Ask me about phishing, malware, passwords, firewalls, or scams.");
            // End of submit names method
        }

        // Send button – read input, display it, then get and display a bot reply
        private void send_question(object sender, RoutedEventArgs e)
        {
            // Start of send question method
            string message = questions_textbox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(message))
                return;

            // Show what the user typed
            chats_list.Items.Add($"{username}: {message}");

            // ── Memory: save favourite topic  
            if (message.Contains("interested in"))
            {
                SaveToFile(message);
                questions_textbox.Clear();
                return;
            }

            // ── Memory: recall favourite topic 
            if (message.Contains("favorite topic") || message.Contains("favourite topic"))
            {
                if (File.Exists(memoryFile))
                {
                    string savedTopic = File.ReadAllText(memoryFile);
                    chats_list.Items.Add($"ChatBot: Your favourite topic is: {savedTopic}");
                }
                else
                {
                    chats_list.Items.Add("ChatBot: I don't know your favourite topic yet.");
                }

                questions_textbox.Clear();
                ScrollToBottom();
                return;
            }

            // Get bot reply and add it to the chat list
            string botReply = GetChatbotResponse(message);
            chats_list.Items.Add($"ChatBot: {botReply}");

            questions_textbox.Clear();
            ScrollToBottom();
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

            // Check for deep-dive sub-topic questions (e.g. "malware types")
            string subReply = GetSubTopicResponse(message);
            if (!subReply.StartsWith("Sorry"))
                return subReply;

            // Sentiment-only reply when no topic is detected
            if (!string.IsNullOrEmpty(sentiment))
                return $"{GetSentimentSupport(sentiment)} Feel free to ask me about phishing, malware, passwords, firewalls, or scams.";

            return "I am built to answer cybersecurity questions. Try asking about: phishing, malware, passwords, firewalls, or scams.";
            // End of chatbot response method
        }

        // Builds a response combining a random topic fact, a sentiment opener, and repeat-ask deep-dive hints
        public string BuildResponse(string topic, string sentiment, bool moreInfo)
        {
            // Start of build response method

            // Track how many times this topic has been asked
            if (!topicAskCount.ContainsKey(topic))
                topicAskCount[topic] = 0;

            topicAskCount[topic]++;

            // After asking about the same topic twice, offer deep-dive options
            if (topicAskCount[topic] == 2)
            {
                return $"You've asked about {topic} a few times! Would you like to go deeper? Type:\n" +
                       $"  '{topic} types'    - Types of {topic}\n" +
                       $"  '{topic} detect'   - How to detect {topic}\n" +
                       $"  '{topic} prevent'  - How to prevent {topic}";
            }

            // Pick a random response for this topic
            string[] responses = chatbotResponses[topic];
            int index = random.Next(responses.Length);
            string response = responses[index];

            // Prepend an empathetic opener if a sentiment was detected
            string support = GetSentimentSupport(sentiment);
            return string.IsNullOrEmpty(support) ? response : $"{support}\n\n{response}";

            // End of build response method
        }

        // Returns a deep-dive answer for sub-topic queries like "malware types"
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
            return sentiment switch
            {
                "negative" => "I understand this topic can feel overwhelming — you are not alone.",
                "positive" => "Great to hear you are feeling confident about this!",
                _ => ""
            };
            // End of get sentiment support method
        }


        // Saves the user's stated favourite topic to a local text file for later recall
        public void SaveToFile(string message)
        {
            // Start of save to file method
            string topic = message.Replace("i am interested in", "").Trim();

            if (string.IsNullOrWhiteSpace(topic))
            {
                chats_list.Items.Add("ChatBot: Please tell me what you are interested in after \"I am interested in\".");
                return;
            }

            File.WriteAllText(memoryFile, topic);
            chats_list.Items.Add($"ChatBot: Got it! I will remember that your favourite topic is \"{topic}\".");
            // End of save to file method
        }

      

        // Scrolls the chat list down to the most recently added message
        private void ScrollToBottom()
        {
            if (chats_list.Items.Count > 0)
                chats_list.ScrollIntoView(chats_list.Items[chats_list.Items.Count - 1]);
        }

        // End of class
    }
    // End of namespace
}