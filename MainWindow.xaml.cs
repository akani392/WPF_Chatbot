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

        // Voice recording
        public void PlayVoiceGreeting()
        {
            // Start of voice recording
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
            // End of voice recording
        }

        //Class-level variables

        string username = "";
        Random random = new Random();
        string memoryFile = "memory.txt";
        string currentTopic = "";

        // currentMode controls which feature the user is currently in.
        // "menu"  - main menu is showing, waiting for 1 / 2 / 3
        // "cyber" - cybersecurity Q&A mode (Part 1 & 2 features)
        // "task"  - Task Assistant mode
        // "quiz"  - Quiz Game mode
        string currentMode = "menu";

        // Tracks how many times each topic has been asked so we can offer deep-dives
        Dictionary<string, int> topicAskCount = new Dictionary<string, int>();

        // Feature class instances
        TaskAssistant taskAssistant = new TaskAssistant();
        QuizGame quizGame = new QuizGame();
        NLPProcessor nlp = new NLPProcessor();   // Task 3: intent detection
        ActivityLog activityLog = new ActivityLog();    // Task 3: action history

        // Colours for chat messages
        readonly Brush botColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));  // blue - ChatBot
        readonly Brush userColor = new SolidColorBrush(Color.FromRgb(244, 67, 54));   // red  - user

        // Numbered deep-dive menu for cybersecurity topics (existing Part 1/2 feature)
        Dictionary<string, string> activeMenu = null;

        // ── Topic keyword lookup ─────────────────────────────────────────────────

        Dictionary<string, string[]> topicKeyWords = new Dictionary<string, string[]>()
        {
            { "phishing", new string[] { "phishing", "email", "lure" } },
            { "malware",  new string[] { "malware", "virus", "trojan", "ransomware" } },
            { "password", new string[] { "password", "credentials", "passphrase" } },
            { "firewall", new string[] { "firewall", "network barrier", "acl" } },
            { "scam",     new string[] { "scam", "fraud", "social engineering" } }
        };

        // ── Main response dictionary ─────────────────────────────────────────────

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
                    "A password is a secret word or string of characters used to verify a user's identity.",
                    "In computer security, a password is a form of knowledge-based authentication.",
                    "Passwords are stored as cryptographic hashes - the system compares hashes, not plain text.",
                    "A password is a fundamental access control mechanism linking actions to a unique account.",
                    "Historically, a password was a spoken phrase used by guards to verify identity."
                }
            },
            {
                "firewall",
                new string[]
                {
                    "A firewall is a digital security barrier that protects a private network from unauthorized access.",
                    "A firewall sits between a trusted internal network and an untrusted external one.",
                    "Firewalls inspect network traffic at specific OSI layers and filter against an Access Control List.",
                    "A firewall can be hardware, cloud-native, or a software agent enforcing security policies.",
                    "The original 'firewall' was a physical wall built to stop the spread of fire in buildings."
                }
            },
            {
                "scam",
                new string[]
                {
                    "In cybersecurity, a scam exploits trust, fear, urgency, or greed to trick people.",
                    "Scams are delivered via email, SMS, voice calls, or social media to steal credentials.",
                    "Scam websites mirror real brands to harvest usernames, passwords, and 2FA codes.",
                    "Business Email Compromise (BEC) deceives employees into routing payments to attackers.",
                    "Scams wrap malicious payloads in deceptive narratives to bypass security warnings."
                }
            }
        };

        // ── Sub-topic deep-dive responses ────────────────────────────────────────

        Dictionary<string, string> subTopicResponses = new Dictionary<string, string>()
        {
            { "malware types",   "Types of Malware:\n\nRansomware: Encrypts files and demands payment.\nSpyware: Records keystrokes silently to steal credentials." },
            { "malware detect",  "How to Detect Malware:\n\n- Slow performance\n- Pop-ups or browser redirects\n- Antivirus suddenly disabled\n- Unusual network activity\n- Encrypted or missing files" },
            { "malware prevent", "How to Prevent Malware:\n\n- Keep software updated\n- Use reputable antivirus\n- Avoid suspicious links\n- Download from trusted sources\n- Back up your data" },
            { "phishing types",  "Types of Phishing:\n\nSpear Phishing: Targeted at a specific person.\nSmishing: Via SMS.\nVishing: Via voice calls.\nWhaling: Targets senior executives." },
            { "phishing detect", "How to Detect Phishing:\n\n- Check for misspelled sender addresses\n- Suspicious urgency\n- Hover to preview links\n- Generic greetings\n- Poor grammar" },
            { "phishing prevent","How to Prevent Phishing:\n\n- Never click unsolicited links\n- Enable MFA\n- Use email spam filters\n- Report suspicious emails\n- Stay trained on red flags" },
            { "password types",  "Types of Passwords:\n\nPassphrase: Random word sequence.\nOTP: Temporary single-use code.\nBiometric: Fingerprint or face ID.\nHardware Key: Physical device like YubiKey." },
            { "password detect", "How to Detect a Compromised Password:\n\n- Unexpected login alerts\n- Breach notifications\n- Check haveibeenpwned.com\n- Sudden logouts" },
            { "password prevent","How to Keep Passwords Secure:\n\n- Use unique 12+ character passwords\n- Use a password manager\n- Enable MFA\n- Never share passwords\n- Change immediately if breached" },
            { "firewall types",  "Types of Firewalls:\n\nPacket-Filtering: Inspects individual packets.\nStateful: Tracks active connections.\nApplication Layer: Deep content filtering.\nNGFW: Combines IPS, SSL inspection, and app awareness." },
            { "firewall detect", "How to Know Your Firewall Is Working:\n\n- Check firewall logs\n- Run a port scanner\n- Monitor management console alerts\n- Run penetration tests" },
            { "firewall prevent","Firewall Best Practices:\n\n- Keep firmware updated\n- Block everything by default\n- Segment your network\n- Audit rules regularly\n- Combine with IDS/IPS" },
            { "scam types",      "Types of Scams:\n\nAdvance-Fee: Promises rewards for upfront payment.\nTech Support: Fake alerts for unnecessary fixes.\nRomance: Fake relationships before requesting money.\nBEC: Impersonates executives to redirect payments." },
            { "scam detect",     "How to Detect a Scam:\n\n- Unsolicited contact asking for money\n- Verify requests via official channels\n- Too-good-to-be-true deals\n- Pressure tactics\n- Requests for gift cards or wire transfers" },
            { "scam prevent",    "How to Avoid Scams:\n\n- Never send money to unverified people\n- Enable spam filters\n- Educate yourself and others\n- Report to consumer protection authorities\n- Slow down — scammers rely on urgency" }
        };

        // ── Constructor ──────────────────────────────────────────────────────────

        public MainWindow()
        {
            // Start of constructor
            InitializeComponent();
            // End of constructor
        }

        // ── Navigation methods ───────────────────────────────────────────────────

        // Start chatbot - hide logo grid, show username entry grid
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

        // Submit username - validate input then move to the chat screen
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

            error_text.Visibility = Visibility.Collapsed;
            username_grid.Visibility = Visibility.Hidden;
            chats_grid.Visibility = Visibility.Visible;

            PlayVoiceGreeting();

            // Welcome the user then show the main menu straight away
            AddBotMessage("Welcome, " + username + "! I am your Cybersecurity Awareness Assistant.");
            ShowMainMenu();
            // End of submit names method
        }

        // Allow pressing Enter in the chat box to send
        private void questions_textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                send_question(sender, e);
        }

        // ── Main send handler ────────────────────────────────────────────────────

        private void send_question(object sender, RoutedEventArgs e)
        {
            // Start of send question method
            string message = questions_textbox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(message))
                return;

            AddUserMessage(message);

            // NLP: summary can be requested from any mode
            NLPProcessor.NLPResult globalNlp = nlp.Analyse(message);
            if (globalNlp.DetectedIntent == NLPProcessor.Intent.ViewSummary)
            {
                AddBotMessage(activityLog.GetSummary());
                questions_textbox.Clear();
                return;
            }

            // "00" always returns to the main menu from any mode
            if (message == "00")
            {
                currentMode = "menu";
                activeMenu = null;
                ShowMainMenu();
                questions_textbox.Clear();
                return;
            }

            // Route based on current mode
            switch (currentMode)
            {
                case "menu":
                    HandleMenuInput(message);
                    break;

                case "cyber":
                    HandleCyberMode(message);
                    break;

                case "task":
                    HandleTaskMode(message);
                    break;

                case "quiz":
                    HandleQuizMode(message);
                    break;
            }

            questions_textbox.Clear();
            // End of send question method
        }

        // ── Mode: Menu ───────────────────────────────────────────────────────────

        // Shows the main menu in amber so it stands out from normal chat
        private void ShowMainMenu()
        {
            // Start of show main menu method
            AddMenuMessage(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "         MAIN MENU\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                "  1  -  Cybersecurity Q&A\n" +
                "  2  -  Task Assistant\n" +
                "  3  -  Cybersecurity Quiz\n\n" +
                "Type a number to get started.\n" +
                "Type 00 at any time to return here."
            );
            // End of show main menu method
        }

        // Handles number input while the main menu is showing.
        // NLP is checked first so natural language typed from the menu
        // routes directly to the right mode without needing a number.
        private void HandleMenuInput(string message)
        {
            // Start of handle menu input method

            // NLP Task 3: detect intent from natural language BEFORE checking numbers.
            // e.g. "set a reminder to change password" -> switches to task mode directly
            NLPProcessor.NLPResult nlpResult = nlp.Analyse(message);

            if (nlpResult.DetectedIntent == NLPProcessor.Intent.AddTask ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.ViewTasks ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.CompleteTask ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.DeleteTask)
            {
                currentMode = "task";
                AddBotMessage("Switching to Task Assistant...");
                HandleTaskMode(nlpResult.NormalisedCommand);
                return;
            }

            if (nlpResult.DetectedIntent == NLPProcessor.Intent.StartQuiz)
            {
                currentMode = "quiz";
                string quizReply;
                quizGame.TryHandle("quiz", out quizReply);
                AddBotMessage(quizReply);
                return;
            }

            if (nlpResult.DetectedIntent == NLPProcessor.Intent.AskCyber)
            {
                currentMode = "cyber";
                AddBotMessage("Switching to Cybersecurity Q&A...");
                HandleCyberMode(message);
                return;
            }

            if (nlpResult.DetectedIntent == NLPProcessor.Intent.ViewSummary)
            {
                AddBotMessage(activityLog.GetSummary());
                return;
            }

            // No NLP intent matched - fall back to numbered menu selection
            switch (message.Trim())
            {
                case "1":
                    currentMode = "cyber";
                    AddBotMessage("Cybersecurity Q&A mode. Ask me about phishing, malware, passwords, firewalls, or scams.\nType 00 to return to the main menu.");
                    break;

                case "2":
                    currentMode = "task";
                    AddBotMessage("Task Assistant mode. Type \"tasks\" to see your options.\nType 00 to return to the main menu.");
                    break;

                case "3":
                    currentMode = "quiz";
                    string quizReply;
                    quizGame.TryHandle("quiz", out quizReply);
                    AddBotMessage(quizReply);
                    break;

                default:
                    AddBotMessage("Please type 1, 2, or 3 to choose an option, or just describe what you need.");
                    ShowMainMenu();
                    break;
            }
            // End of handle menu input method
        }
        // ── Mode: Cybersecurity Q&A ──────────────────────────────────────────────

        private void HandleCyberMode(string message)
        {
            // Start of handle cyber mode method

            // Numbered deep-dive menu (existing Part 1/2 feature)
            if (activeMenu != null)
            {
                string subKey;
                if (activeMenu.TryGetValue(message, out subKey))
                {
                    activeMenu = null;
                    AddBotMessage(subTopicResponses[subKey]);
                    return;
                }
                else
                {
                    activeMenu = null;
                    AddBotMessage("That wasn't a valid option. Continuing with your question...");
                }
            }

            // NLP Task 3: run intent detection so users can phrase requests naturally
            // e.g. "remind me to check my firewall" is recognised as an add-task intent
            NLPProcessor.NLPResult nlpResult = nlp.Analyse(message);

            // If the user expressed a task intent while in cyber mode, switch modes
            if (nlpResult.DetectedIntent == NLPProcessor.Intent.AddTask)
            {
                currentMode = "task";
                HandleTaskMode(nlpResult.NormalisedCommand);
                return;
            }

            // If the user asked for the quiz while in cyber mode, switch modes
            if (nlpResult.DetectedIntent == NLPProcessor.Intent.StartQuiz)
            {
                currentMode = "quiz";
                HandleQuizMode("quiz");
                return;
            }

            // If the user asked what actions have been taken, show the activity log
            if (nlpResult.DetectedIntent == NLPProcessor.Intent.ViewSummary)
            {
                AddBotMessage(activityLog.GetSummary());
                return;
            }

            // Memory: save favourite topic
            if (message.Contains("interested in"))
            {
                SaveToFile(message);
                return;
            }

            // Memory: recall favourite topic
            if (message.Contains("favorite topic") || message.Contains("favourite topic"))
            {
                if (File.Exists(memoryFile))
                    AddBotMessage("Your favourite topic is: " + File.ReadAllText(memoryFile));
                else
                    AddBotMessage("I don't know your favourite topic yet. Try \"I am interested in phishing\".");
                return;
            }

            // Normal topic/sentiment response - log what was discussed
            string cyberReply = GetChatbotResponse(message);
            AddBotMessage(cyberReply);

            if (!string.IsNullOrEmpty(currentTopic))
                activityLog.AddEntry("Discussed cybersecurity topic: " + currentTopic);
            // End of handle cyber mode method
        }
        // ── Mode: Task Assistant ─────────────────────────────────────────────────

        private void HandleTaskMode(string message)
        {
            // Start of handle task mode method

            // NLP Task 3: detect natural-language task requests before direct commands
            // e.g. "Add a task to enable 2FA" or "Remind me to update my password tomorrow"
            NLPProcessor.NLPResult nlpResult = nlp.Analyse(message);

            // Use the normalised command if NLP found a clearer interpretation
            string commandToProcess = message;
            if (nlpResult.DetectedIntent == NLPProcessor.Intent.AddTask ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.ViewTasks ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.CompleteTask ||
                nlpResult.DetectedIntent == NLPProcessor.Intent.DeleteTask)
            {
                commandToProcess = nlpResult.NormalisedCommand;
            }

            // If user asked for the activity summary while in task mode
            if (nlpResult.DetectedIntent == NLPProcessor.Intent.ViewSummary)
            {
                AddBotMessage(activityLog.GetSummary());
                return;
            }

            string taskReply;

            // TryHandle covers the menu, title input, and reminder follow-up
            if (taskAssistant.TryHandle(commandToProcess, username, out taskReply))
            {
                AddBotMessage(taskReply);

                // Log meaningful task actions to the activity log
                if (commandToProcess.StartsWith("add task"))
                    activityLog.AddEntry("Task added: '" + nlpResult.ExtractedTitle + "'" +
                        (nlpResult.ExtractedDays.HasValue
                            ? " with reminder in " + nlpResult.ExtractedDays + " day(s)"
                            : ""));

                return;
            }

            // Complete and delete shortcuts shown after menu options 3 and 4
            string actionReply;
            if (taskAssistant.TryCompleteTask(commandToProcess, username, out actionReply))
            {
                AddBotMessage(actionReply);
                activityLog.AddEntry("Completed a task");
                return;
            }

            if (taskAssistant.TryDeleteTask(commandToProcess, username, out actionReply))
            {
                AddBotMessage(actionReply);
                activityLog.AddEntry("Deleted a task");
                return;
            }

            AddBotMessage("Type \"tasks\" to see the Task Assistant menu, or 00 to return to the main menu.");
            // End of handle task mode method
        }
        // ── Mode: Quiz ───────────────────────────────────────────────────────────

        private void HandleQuizMode(string message)
        {
            // Start of handle quiz mode method
            string quizReply;

            if (quizGame.TryHandle(message, out quizReply))
            {
                AddBotMessage(quizReply);

                // Quiz ended naturally - log with score info, return to the menu
                if (quizReply.Contains("Quiz Complete"))
                {
                    // Extract the score line from the reply for the log entry
                    string scoreLine = "";
                    foreach (string line in quizReply.Split('\n'))
                    {
                        if (line.Contains("Final Score:"))
                        {
                            scoreLine = line.Trim();
                            break;
                        }
                    }
                    activityLog.AddEntry("Quiz completed — " + scoreLine);
                    currentMode = "menu";
                    ShowMainMenu();
                }
                else if (quizReply.Contains("Quiz stopped"))
                {
                    activityLog.AddEntry("Quiz stopped early by user");
                    currentMode = "menu";
                    ShowMainMenu();
                }
            }
            // End of handle quiz mode method
        }
        // ── Chatbot logic (used in cyber mode) ───────────────────────────────────

        // Main entry point - ties together topic detection, sentiment, and follow-up
        public string GetChatbotResponse(string message)
        {
            // Start of chatbot response method
            string sentiment = DetectSentiment(message);
            bool moreInfo = IsFollowUp(message);
            string topic = DetectTopic(message);

            if (string.IsNullOrEmpty(topic) && moreInfo && !string.IsNullOrEmpty(currentTopic))
                topic = currentTopic;

            if (!string.IsNullOrEmpty(topic))
            {
                currentTopic = topic;
                return BuildResponse(topic, sentiment, moreInfo);
            }

            string subReply = GetSubTopicResponse(message);
            if (!subReply.StartsWith("Sorry"))
                return subReply;

            if (!string.IsNullOrEmpty(sentiment))
                return GetSentimentSupport(sentiment) + " Feel free to ask about phishing, malware, passwords, firewalls, or scams.";

            return "I am built to answer cybersecurity questions. Try asking about: phishing, malware, passwords, firewalls, or scams.";
            // End of chatbot response method
        }

        // Builds a response and shows a numbered menu after 3 asks on the same topic
        public string BuildResponse(string topic, string sentiment, bool moreInfo)
        {
            // Start of build response method
            if (!topicAskCount.ContainsKey(topic))
                topicAskCount[topic] = 0;

            topicAskCount[topic]++;

            if (topicAskCount[topic] == 3)
            {
                activeMenu = new Dictionary<string, string>
                {
                    { "1", topic + " types"   },
                    { "2", topic + " detect"  },
                    { "3", topic + " prevent" }
                };

                return "You have asked a lot about " + topic + "! Would you like to go deeper?\n\n" +
                       "  Reply 1  -  Types of " + topic + "\n" +
                       "  Reply 2  -  How to detect " + topic + "\n" +
                       "  Reply 3  -  How to prevent " + topic;
            }

            string[] responses = chatbotResponses[topic];
            string response = responses[random.Next(responses.Length)];
            string support = GetSentimentSupport(sentiment);

            return string.IsNullOrEmpty(support) ? response : support + "\n\n" + response;
            // End of build response method
        }

        public string GetSubTopicResponse(string message)
        {
            // Start of sub-topic response method
            foreach (var entry in subTopicResponses)
            {
                if (message.Contains(entry.Key))
                    return entry.Value;
            }
            return "Sorry, I don't understand that.";
            // End of sub-topic response method
        }

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

        public bool IsFollowUp(string message)
        {
            // Start of is follow up method
            string[] followUpPhrases = { "explain more", "tell me more", "can you elaborate", "i want to know more", "give me more details", "more info" };
            return followUpPhrases.Any(p => message.Contains(p));
            // End of is follow up method
        }

        public string GetSentimentSupport(string sentiment)
        {
            // Start of get sentiment support method
            switch (sentiment)
            {
                case "negative": return "I understand this topic can feel overwhelming — you are not alone.";
                case "positive": return "Great to hear you are feeling confident about this!";
                default: return "";
            }
            // End of get sentiment support method
        }

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

        // ── Message display helpers ──────────────────────────────────────────────

        // Adds a bot message with a blue label
        private void AddBotMessage(string text)
        {
            // Start of add bot message method
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 2, 0, 2);

            Run label = new Run("ChatBot: ");
            label.Foreground = botColor;
            label.FontWeight = FontWeights.Bold;
            para.Inlines.Add(label);

            Run body = new Run(text);
            body.Foreground = Brushes.White;
            para.Inlines.Add(body);

            chat_rtb.Document.Blocks.Add(para);
            chat_rtb.ScrollToEnd();
            // End of add bot message method
        }

        // Adds a menu message with an amber label so it stands out visually
        private void AddMenuMessage(string text)
        {
            // Start of add menu message method
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 4, 0, 4);

            // White bold "MENU" label
            Run label = new Run("MENU\n");
            label.Foreground = Brushes.White;
            label.FontWeight = FontWeights.Bold;
            para.Inlines.Add(label);

            // White menu body text
            Run body = new Run(text);
            body.Foreground = Brushes.White;
            para.Inlines.Add(body);

            chat_rtb.Document.Blocks.Add(para);
            chat_rtb.ScrollToEnd();
            // End of add menu message method
        }

        // Adds a user message with a red label
        private void AddUserMessage(string text)
        {
            // Start of add user message method
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 2, 0, 2);

            Run label = new Run(username + ": ");
            label.Foreground = userColor;
            label.FontWeight = FontWeights.Bold;
            para.Inlines.Add(label);

            Run body = new Run(text);
            body.Foreground = Brushes.White;
            para.Inlines.Add(body);

            chat_rtb.Document.Blocks.Add(para);
            // End of add user message method
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