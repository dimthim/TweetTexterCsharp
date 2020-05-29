using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Net;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Security.Policy;

namespace TwitterSharp
{


    public partial class OpeningMenu : Form
    {
        bool MessageBoxOpen = false;
        static bool StartLoop = false;
        static string OldTweetTime = "";

        public OpeningMenu()
        {
            InitializeComponent();


            double IntervalMinutes = 10;
            var TimeSinceCheck = new System.Timers.Timer(1000 * 60 * IntervalMinutes);
            TimeSinceCheck.Elapsed += OnTimedEvent;
            TimeSinceCheck.AutoReset = true;
            TimeSinceCheck.Enabled = true;
        }

        static Uri TwitterURI;
        
        private async static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (StartLoop)
            {
                //Check Twitter

                string TwitStr = "";
                using(var TwitterClient = new HttpClient())
                {
                    TwitterClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", "AAAAAAAAAAAAAAAAAAAAANIFEgEAAAAAC1%2F4cuMGUpNgGfdcfCteF%2F4JHSA%3DhSNOROobfSNHj3rrVJ3AnTi3i2X8NhZ6m7gBJut2NIK0NgB0XC");
                    TwitStr = await TwitterClient.GetStringAsync(TwitterURI);
                }
                

                bool InQuotes = false;
                string TitleString = "";
                string NewTweetTime = "";
                string TweetText = "";

                for (int i = 0; i < TwitStr.Length; ++i)
                {

                    if(TweetText.Length > 0 && NewTweetTime.Length > 0)
                    {
                        break;
                    }
                    else if (InQuotes)
                    {
                        if (TitleString.Length > 0 && TwitStr[i] == '"' && TwitStr[i - 1] != '\\')
                        {
                            switch (TitleString)
                            {
                                case "created_at":
                                    {
                                        //Write Next quote body to TimeString
                                        for (int ITime = i + 3; ITime < TwitStr.Length; ++ITime)
                                        {
                                            if (TwitStr[ITime] == '"' && TwitStr[ITime - 1] != '\\')
                                            {
                                                i = ITime;
                                                break;
                                            }
                                            else
                                                NewTweetTime += TwitStr[ITime];

                                        }

                                    }
                                    break;
                                case "text":
                                    {
                                        //Write next quote body to TweetContent
                                        for (int ITime = i + 3; ITime < TwitStr.Length; ++ITime)
                                        {
                                            if (TwitStr[ITime] == '"' && TwitStr[ITime - 1] != '\\')
                                            {
                                                i = ITime;
                                                break;
                                            }
                                            else
                                                TweetText += TwitStr[ITime];
                                        }

                                    }
                                    break;
                                default:
                                    {
                                    }
                                    break;
                            }
                            TitleString = "";
                            InQuotes = false;
                        }
                        else
                        {
                            TitleString += TwitStr[i];
                        }
                        
                    }
                    else if (TwitStr[i] == '"' && !InQuotes)
                    {
                        InQuotes = true;
                    }
                }

                //Send Text by email User if new tweet is posted.
                if(NewTweetTime != OldTweetTime)
                {
                    OldTweetTime = NewTweetTime;
                    string[] TweetChunk = NewTweetTime.Split(' ');

                    //send text message via email.
                    MimeMessage TextAlert = new MimeMessage();
                    TextAlert.From.Add(new MailboxAddress (GlobalVars.TwitterHandle, "1s22p63d104f14@gmail.com"));
                    TextAlert.To.Add(new MailboxAddress(GlobalVars.PhoneNumber + GetProviderAddress()));
                    TextAlert.Subject = GlobalVars.TwitterHandle;

                    string TrimmedTweetText = "";
                    if(TweetText.Length < 116)
                    {
                        TrimmedTweetText = TweetText;
                    }
                    else
                    {
                        TrimmedTweetText = TweetText.Substring(0, 115) + "...";
                    }

                    TextAlert.Body = new TextPart("plain")
                    {
                        Text = "<" + TweetChunk[3] + ">" + "\n" + TrimmedTweetText
                    };

                    SendMessage(TextAlert);


                }


            }
        }

        enum providers 
        {
            VERIZON,
            ATT,
            TMOBILE,
            SPRINT
        }
        string[] Providers = { "Verizon", "AT&T", "T-Mobile", "Sprint" };
        static string GetProviderAddress()
        {
            string Address = "";
            switch((providers)GlobalVars.Provider)
            {
                case providers.VERIZON: Address = "@vtext.com";break;
                case providers.ATT: Address = "@txt.att.net"; break;
                case providers.TMOBILE: Address = "@tmomail.net"; break;
                case providers.SPRINT: Address = "@messaging.sprintpcs.com"; break;
            }

            return (Address);
        }

        public static void SendMessage(MimeMessage TextAlert)
        {
            using (var client = new SmtpClient())
            {
                //client.Connect(URI,Port,SecurityOptions)
                client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);

                client.Authenticate("1s22p63d104f14@gmail.com", GlobalVars.RandoDingen);

                client.Send(TextAlert);
      
                client.Disconnect(true);
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (this.textNumber.Text.Length == 10 && this.textTwitter.Text.Length > 0)
            {
                GlobalVars.PhoneNumber = this.textNumber.Text;
                GlobalVars.TwitterHandle = this.textTwitter.Text;
                GlobalVars.Provider = this.comboBox1.SelectedIndex;
                this.Visible = false;
                StartLoop = true;
          
                TwitterURI = new Uri("https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=" + GlobalVars.TwitterHandle + "&count=1&trim_user=1");
            }

        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!MessageBoxOpen)
            {
                MessageBoxOpen = true;
                var Response = MessageBox.Show("Do you want to close TwitterSharp?", "TwitterSharp", MessageBoxButtons.YesNo);
                MessageBoxOpen = false;
                if (Response == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }

     
        private void OpeningMenu_Load(object sender, EventArgs e)
        {
            this.textNumber.Text = GlobalVars.PhoneNumber;
            this.textTwitter.Text = GlobalVars.TwitterHandle;
            this.comboBox1.Items.AddRange(Providers);

            if(GlobalVars.Provider < Providers.Length)
                this.comboBox1.Text = (string)this.comboBox1.Items[GlobalVars.Provider];
            else
                this.comboBox1.Text = (string)this.comboBox1.Items[0];
        }

    }
}
