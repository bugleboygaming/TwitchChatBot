using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.UI;
using System.Web.Hosting;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace TwitchChatBot
{

    public class TwitchChatBotException : Exception
    { 
        public TwitchChatBotException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public TwitchChatBotException(string message): base(message) 
        {
            ErrorInfo = message;
            // Add any type-specific logic.
        }
        public TwitchChatBotException(string message, Exception innerException): base (message, innerException)
        {
            ErrorInfo = message;
            // Add any type-specific logic for inner exceptions.
        }
        protected TwitchChatBotException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
        public override string ToString()
        {
            return ErrorInfo;
        }

        string ErrorInfo = "";
    }

    public static class ExLogger
    {
        public static void ExLog(string inString)
        {
            string proxy = "";

            GoogleFormLogger gfl = new GoogleFormLogger(proxy);
            gfl.PostToGoogleForm(inString);
        }
    }

    public class GoogleFormLogger
    {
        public GoogleFormLogger(string inProxy)
        {

            if (!(String.IsNullOrEmpty(inProxy)))
            {
                HttpClientHandler handler = new HttpClientHandler();
                //CookieContainer cc = new CookieContainer();
                WebProxy proxy = new WebProxy();
                proxy.Address = new Uri(inProxy);
                handler.Proxy = proxy;
                formLoggerClient = new HttpClient(handler);
            }
            else {
                formLoggerClient = new HttpClient();
            }
        }

        public async void PostToGoogleForm(string inData)
        { 
            HttpContent content = new FormUrlEncodedContent(new List<KeyValuePair<string,string>>{new KeyValuePair<string,string>("entry_1083813362",inData)});
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentType.CharSet = "UTF-8";
            formLoggerClient.PostAsync(new Uri("https://docs.google.com/forms/d/14Tpdr7_JJbWBIpVqyBUXvo9scDORk84AYOgczc12sFo/formResponse"), content).Wait();
            //await formLoggerClient.PostAsync(new Uri("https://docs.google.com/forms/d/14Tpdr7_JJbWBIpVqyBUXvo9scDORk84AYOgczc12sFo/formResponse"), content);
            //var response = await formLoggerClient.PostAsync(new Uri("https://docs.google.com/forms/d/14Tpdr7_JJbWBIpVqyBUXvo9scDORk84AYOgczc12sFo/formResponse"), content);
            
            //string responseBody = await response.Content.ReadAsStringAsync();
            
            //Console.WriteLine(responseBody);
        }

        HttpClient formLoggerClient;
    }


    class CustomHost : MarshalByRefObject
    {
        public void parse(string page, string query, ref StreamWriter sw)
        {
            SimpleWorkerRequest swr = new SimpleWorkerRequest(page, query, sw);
            HttpRuntime.ProcessRequest(swr);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        ~CustomHost()
        {
            HttpRuntime.Close();
        }
    }



    public class TwitchBot : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ProxyPassNotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public class TwitchAuthorizationPart
        {
            public string[] scopes { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }

        public class TwitchToken
        {
            public bool valid { get; set; }
            public TwitchAuthorizationPart authorization { get; set; }
            public string user_name { get; set; }
        }

        public class TwitchUserInfo
        {
            public Dictionary<string,string> _links {get;set;}
            public TwitchToken token { get; set; }
        }

        public void PayPalSupport()
        {
            Process.Start("http://www.donation-tracker.com/u/sovietmade");
        }

        public void GitHubForkMe()
        {
            Process.Start("https://github.com/Sovietmade/TwitchChatBot");
        }

        public void RateIt()
        {
            Process.Start("https://sourceforge.net/projects/twitchquizbot/reviews/new");
        }

        public class TwitchAuthorization
        {

            public TwitchAuthorization()
            {
                AuthKey = Properties.Settings.Default.authKey;
                AuthName = Properties.Settings.Default.authName;

            }

            public void TwitchAuthorize()
            {
                Process.Start("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=amoyxo9a7agc0e1gjpcawa1rqb2ciy4&redirect_uri=http://localhost:6555/Auth.aspx&scope=chat_login+channel_editor+user_read");
            }



            public void TwitchLogOut()
            {
                Process.Start("http://www.twitch.tv/logout");
                AuthName = "";
                AuthKey = "";
            }

            public string AuthKey
            {
                set
                {
                    authKey = value;
                    Properties.Settings.Default.authKey = value;
                    Properties.Settings.Default.Save();
                }
                get
                {
                    return authKey;
                }
            }

            public string AuthName
            {
                set
                {
                    authName = value;
                    Properties.Settings.Default.authName = value;
                    Properties.Settings.Default.Save();
                }
                get
                {
                    return authName;
                }
            }

            //public TwitchUserInfo UserInfo { get; set; }

            String authKey;
            String authName;

        }

		public TwitchBot ()
		{     
			mTcpConnection = new TcpConnection();
			mTcpConnection.DataReceived += ProccessMessageData;
            mTcpConnection.EmergencyDisc = EmergencyReset;
			mIrcCommandAnalyzer = new SimpleTwitchBotIrcCommandAnalyzer();

            host = (CustomHost)ApplicationHost.CreateApplicationHost(typeof(CustomHost), "/", Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
            
            StartHttpListener();
            mQE = new QuizEngine();
            mQE.SendMessage = SendMessageToCurrentChannel;
            mQE.PropertyChanged += ProxyPassNotifyPropertyChanged;
            mQE.InternalInitiationQuizStop = StopQuiz;

            TwitchChannel = "NONE";
		}

        public void StartHttpListener()
        {
            mListener = new HttpListener();
            mListener.Prefixes.Add("http://localhost:6555/");
            mListener.Start();
            mListener.BeginGetContext(ListenerCallback, mListener);
        }

        void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext context = mListener.EndGetContext(result);
            HttpListenerResponse response = context.Response;
 
            StreamReader reader = new StreamReader(context.Request.InputStream);
            string postRequest = reader.ReadToEnd();
            if (postRequest.Contains("{\"x\":\"#access_token="))
            {
                string cutRequest = postRequest.Replace("{\"x\":\"#access_token=", "");
                cutRequest = cutRequest.Substring(0, cutRequest.IndexOf('&'));
                Console.WriteLine("Data received:" + cutRequest);
                TA.AuthKey = cutRequest;
                WebRequest request = WebRequest.Create ("https://api.twitch.tv/kraken?oauth_token=" + cutRequest);
                HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse ();
                Stream dataStream = httpWebResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader dataReader = new StreamReader(dataStream);
                // Read the content. 
                string responseFromServer = dataReader.ReadToEnd();
                TwitchUserInfo twitchinfo = new JavaScriptSerializer().Deserialize<TwitchUserInfo>(responseFromServer);
                AuthorizedName = twitchinfo.token.user_name;
            }
            

            StreamWriter sw = new StreamWriter(response.OutputStream);

            string lp = context.Request.Url.LocalPath.Substring(1);
            string queryUrl = context.Request.Url.Query;
            if (context.Request.Url.Query.Length > 0 && context.Request.Url.Query[0] == '?')
            {
                queryUrl = queryUrl.Substring(1);
            }
            host.parse(lp, queryUrl, ref sw);
            sw.Flush();
            response.Close();
            
            mListener.BeginGetContext(ListenerCallback, mListener);
        }

		public Endpoint Proxy {
			get {
				return mTcpConnection.Proxy;
			}
			set{
				mTcpConnection.Proxy = value;
			}
		}

		public Endpoint Destination {
			get {
				return mTcpConnection.Destination;
			}
			set{
				mTcpConnection.Destination = value;
			}
		}

        public void Disconnect()
        {
            if (ctsPing != null)
            {
                ctsPing.Cancel();
            }

            if (IsQuizRunning)
            {
                StopQuiz();
            }
            mQE.EndAcceptMessages();
            mTcpConnection.Disconnect();
            
            Connected = Connected;
        }

		public void Connect ()
		{
			mTcpConnection.Connect();
            Connected = Connected; //  trigger NotifyPropertyChangeds
            mQE.BeginAcceptMessages();
            if (Connected)
            {
                StartPinging();
            }
		}

        async void StartPinging()
        {
            if (Connected)
            {
                if (ctsPing != null)
                {
                    ctsPing.Cancel();
                }
                ctsPing = new CancellationTokenSource();
                await PingServer(ctsPing.Token);
            }
        }

        async Task PingServer(CancellationToken ct)
        {
            while (!(ct.IsCancellationRequested))
            {
                await Task.Delay(30000);
                SendMessage(new IrcCommand(null, "PING", new IrcCommandParameter("TwitchQuizBot")).ToString() + "\r\n");
                //mQE.ChuckNorris("");
            }
        }

        class RandomQuizObject {
            public string text { get; set; }
            public int number { get; set; }
            public bool found { get; set; }
            public string type { get; set; }
        }

        void FillInQuiz()
        {
            
            for (int i = 0; i < 20; i++)
            {
                WebRequest request = WebRequest.Create("https://numbersapi.p.mashape.com/random/trivia?fragment=true&json=true&max=99999&min=1");
                request.Headers.Add("X-Mashape-Key", "NvahP1eNmSmsh4QkjEgVRNWyEQLyp1bwLL5jsnMaCxKFBDpzK7");
                HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
                Stream dataStream = httpWebResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader dataReader = new StreamReader(dataStream);
                // Read the content. 
                string responseFromServer = dataReader.ReadToEnd();
                RandomQuizObject RQO = new JavaScriptSerializer().Deserialize<RandomQuizObject>(responseFromServer);
                //Console.WriteLine(RQO.text);
                mQE.AddNewQuizObject(RQO.text, RQO.number.ToString());
                //Console.WriteLine(responseFromServer);
            }
        }

        async public Task GetRandomQuizQuestion()
        {
            await Task.Factory.StartNew((System.Action)FillInQuiz, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            //await FillInQuiz();
        }

		public void SendMessage (string inMessage)
		{
			mTcpConnection.SendMessage(inMessage);
		}

		void ProccessMessageData(object sender, ReceivedDataArgs ea)
		{
			//Collecting all data from mMessageBuffer and ReceivedDataArgs in the single buffer - tempTotalData.
			//Collected data will be parsed for distinct commands.
			//Collecting data...
			int MessagesBufferLength = (int)mMessagesBuffer.Length;
			byte[] tempTotalData = new byte[MessagesBufferLength + ea.Data.Length];
			mMessagesBuffer.ToArray().CopyTo(tempTotalData,0);
			Array.Copy(ea.Data,0,tempTotalData,MessagesBufferLength,ea.Data.Length);

			mMessagesBuffer.SetLength(0);
			//Collecting finishes here.
			//Parsing data...
            
		    int msgStart = 0;
		    for (int i = 0; i < tempTotalData.Length - 1; i++)
		    {
		        if (tempTotalData[i] == 13 && tempTotalData[i + 1] == 10) // byte 10 = LF and byte 13 = CR
		        {
		            byte[] message = new byte[i - msgStart];
		            Array.Copy(tempTotalData, msgStart, message, 0, i - msgStart); // Copy data[msgStart:i] to message
					//Console.WriteLine("Command Received: {0}",Encoding.UTF8.GetString(message));
					string inMessage = Encoding.UTF8.GetString(message);
					if(inMessage.Length > 0){
                        //OnNotice("test");
						mMessageQ.Enqueue(inMessage);
						Console.WriteLine("COMMAND NAME: {0}",IrcCommand.Parse(inMessage).Name);
                        Console.WriteLine("PARAMETER: ");
                        foreach (var p in IrcCommand.Parse(inMessage).Parameters) {
                            Console.WriteLine(p.ToString());
                        }
                        IrcCommand incCommand = IrcCommand.Parse(inMessage);
                        IrcCommand outCommand = mIrcCommandAnalyzer.GetResponse(IrcCommand.Parse(inMessage));
                        if (outCommand != null)
                        {
                            SendMessage(outCommand.ToString() + "\r\n");
                        }
                        if (incCommand.Name == "PRIVMSG")
                        {
                            int indexOfExclamationSign = incCommand.Prefix.IndexOf('!');
                            string name = incCommand.Prefix.Substring(0, indexOfExclamationSign);

                            if (name != "jtv")
                            {
                                privMessages.Add(String.Format("{0}:{1}\n", name, incCommand.Parameters[incCommand.Parameters.Length - 1].Value));
                                PrivMessages = PrivMessages;
                                mQE.Process(incCommand);
                            }
                            else {
                                if (incCommand.Parameters != null && incCommand.Parameters.Length > 0)
                                {
                                    if (incCommand.Parameters[incCommand.Parameters.Length - 1].Value == "Your message was not sent because you are sending messages too quickly.")
                                    {
                                        OnNotice("Your message was not sent because you are sending messages too quickly. Possible solution: grant mod priveleges to bot");
                                    }
                                    else if (incCommand.Parameters[incCommand.Parameters.Length - 1].Value == "Your message was not sent because it is identical to the previous one you sent, less than 30 seconds ago.")
                                    {
                                        OnNotice("Your message was not sent because it is identical to the previous one you sent, less than 30 seconds ago.");
                                    }
                                }
                            }
                            
                            
                            //Text addition to the chat window
    
                        }
                        if (incCommand.Name == "NOTICE")
                        {
                            if (incCommand.Parameters[incCommand.Parameters.Length - 1].Value == "Login unsuccessful")
                            {
                                Disconnect();
                                AuthorizedName = "";
                                Auth.AuthKey = "";
                                OnNotice("Seems like you should re-authorize and re-connect");
                                //throw new TwitchChatBotException("Seems like you should re-authorize and re-connect");
                                //throw new InvalidOperationException("Seems like you should re-authorize and re-connect");
                            }
                            else {
                                Disconnect();
                                AuthorizedName = "";
                                Auth.AuthKey = "";
                                OnNotice(incCommand.Parameters[incCommand.Parameters.Length - 1].Value);
                            }

                            Console.WriteLine(incCommand.Parameters[incCommand.Parameters.Length - 1].Value);
                        }
					}
					msgStart = i = i + 2;
		        }
		    }
 
		    // What is left from msgStart til the end of data is only a partial message.
		    // We want to save that for when the rest of the message arrives.
		    mMessagesBuffer.Write(tempTotalData, msgStart, tempTotalData.Length - msgStart);

		}

        public void TwitchAuthorize()
        {
            TA.TwitchAuthorize();
        }

        public void TwitchLogOut()
        {
            TA.TwitchLogOut();
            AuthorizedName = AuthorizedName;
        }

        public void EmergencyReset()
        {
            StopQuiz();
            Disconnect();
            OnNotice("Woops!Seems like connection was lost,try to reconnect please");
        }

        async public void StartQuiz()
        {
            if (mQE.QuizList.Length == 0)
            {
                await ProcessQuizFile();
            }
            IsQuizRunning = true;
            mQE.StartQuiz();
            
        }



        public void StopQuiz()
        {
            IsQuizRunning = false;
            mQE.StopQuiz();
            
        }

		public void DumpMessageQ ()
		{
			foreach (var i in mMessageQ) {
				Console.WriteLine(i);
			}
		}



        public void JoinTwitchChannel(String inTwitchChannel)
        {
            //throw new TwitchChatBotException("Test");
            SendMessage("JOIN #" + inTwitchChannel + "\r\n");
            TwitchChannel = inTwitchChannel;
        }

        public void SendMessageToCurrentChannel(String inMessage)
        {
            if (inMessage != null)
            {
                string name = null;
                //if (AuthorizedName != null)
                //{
                //    name = AuthorizedName + "!" + AuthorizedName + "@" + AuthorizedName + ".tmi.twitch.tv";
                //}
                SendMessage(new IrcCommand(name, "PRIVMSG", new IrcCommandParameter("#" + TwitchChannel, false), new IrcCommandParameter(inMessage, true)).ToString() + "\r\n");
            }
        }

        public String QuizFile
        { 
            get{
                return mQE.QuizFile;
            }
            set {
                mQE.QuizFile = value;
            }
        }

        public void AppendQuizObjectToTheQuizFile(QuizObject qo)
        {
            mQE.AppendQuizObjectToTheQuizFile(qo);
        }

        async public Task ProcessQuizFile()
        {
            await mQE.AddQuiz();
        }

        public int DelayBetweenQuestions
        {
            get
            {
                return mQE.DelayBetweenQuestions;
            }
            set
            {
                mQE.DelayBetweenQuestions = value;
            }
        }

        public int TimeBetweenQuestions
        {
            get
            {
                return mQE.TimeBetweenQuestions;
            }
            set
            {
                mQE.TimeBetweenQuestions = value;
            }
        }

        public int TimeBetweenHints
        {
            get
            {
                return mQE.TimeBetweenHints;
            }
            set
            {
                mQE.TimeBetweenHints = value;
            }
        }

        public string LoyalityCommand
        {
            get
            {
                return mQE.LoyalityCommand;
            }
            set
            {
                mQE.LoyalityCommand = value;
            }
        }

        public bool ConnectedAndAuthorized
        {
            get {
                if (Connected && Authorized)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            set {
                NotifyPropertyChanged();
            }
        }



        public TwitchAuthorization Auth
        {
            get
            {
                return TA;
            }
            private set
            {
                TA = value;
            }
        }

        public String AuthorizedName
        {
            get
            {
                return TA.AuthName;
            }
            private set
            {
                TA.AuthName = value;
                Authorized = Authorized; // trigger NotifyPropertyChanged
                NotifyPropertyChanged();
                if (value == null) {
                    Disconnect();
                    TA.AuthKey = "";
                    OnNotice("Authorization went wrong.");
                }
            }
        }

        public bool Authorized
        {
            get {
                return !(String.IsNullOrEmpty(AuthorizedName));
            }
            set {
                ConnectedAndAuthorized = ConnectedAndAuthorized; // trigger NotifyPropertyChanged
                IsConnectedAuthorizedAndQuizRunning = IsConnectedAuthorizedAndQuizRunning;
                IsConnectedAuthorizedAndQuizNotRunning = IsConnectedAuthorizedAndQuizNotRunning;
                NotifyPropertyChanged();
            }
        }

        //omg i hate doing this , but here we go
        public Visibility ConnectedVisibility
        {
            get {
                if (Connected)
                {
                    return Visibility.Hidden;
                }
                else {
                    return Visibility.Visible;
                }
            }
            set {
                NotifyPropertyChanged();
            }
        }

        public Visibility DisconnectedVisibility
        {
            get
            {
                if (Connected)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public bool Connected 
        {
            get { 
                return mTcpConnection.Connected;
            }
            private set {
                ConnectedVisibility = ConnectedVisibility;
                DisconnectedVisibility = DisconnectedVisibility;
                ConnectedAndAuthorized = ConnectedAndAuthorized; // trigger NotifyPropertyChanged
                IsConnectedAuthorizedAndQuizRunning = IsConnectedAuthorizedAndQuizRunning;
                IsConnectedAuthorizedAndQuizNotRunning = IsConnectedAuthorizedAndQuizNotRunning;
                NotifyPropertyChanged();
            }
        }

        public String TwitchChannel
        {
            get {
                return twitchChannel;
            }
            set {
                
                twitchChannel = value;
                NotifyPropertyChanged();
            } 
        }

        public void AddNewQuizObject(string inQuestion, string inAnswer)
        {
            mQE.AddNewQuizObject(inQuestion, inAnswer);    
        }

        public void DropTheItemFromList(QuizObject inObjectToDrop)
        {
            mQE.DropTheItemFromList(inObjectToDrop);
        }

        public void DropTheQuizList()
        {
            mQE.DropTheQuizList();
        }

        public void AskScpecifiedQuestion(QuizObject inQuizObject)
        {
            mQE.AskScpecifiedQuestion(inQuizObject);
        }

        public QuizObject[] QuizList
        {
            get
            {
                return mQE.QuizList;
            }
        }

        public QuizObjectsList QuizListOrig
        {
            get
            {
                return mQE.QuizListOrig;
            }
        }

        public ScoreObject[] Score
        {
            get
            {
                return mQE.Score;
            }
        }

        public QuizObject CurrentQuizObject
        {
            get
            {
                return mQE.CurrentQuizObject;
            }
        }


        public bool IsRandom
        {
            get {
                return mQE.IsRandom;
            }
            set {
                mQE.IsRandom = value;
            }
        }


        public bool ForgiveSmallMisspelling
        {
            get
            {
                return mQE.ForgiveSmallMisspelling;
            }
            set
            {
                mQE.ForgiveSmallMisspelling = value;
            }
        }

        public void PreviousQuestion()
        {
            mQE.PreviousQuestion();
        }

        public void NextQuestion()
        {
            mQE.NextQuestion();
        }

        public String PrivMessages 
        {
            get { 
                return String.Join("",privMessages);
            }
            set {
                
                NotifyPropertyChanged();
            }
        }

        public void AddMessageToPrivMessages(string inString)
        {
            if (privMessages.Count >= MaxMessagesInChat)
            {
                privMessages.RemoveRange(0, 5);
            }
            privMessages.Add(inString);
        }

        public int TimeTillNextQuestion
        {
            get
            {
                return mQE.TimeTillNextQuestion;
            }
        }

        public bool IsConnectedAuthorizedAndQuizRunning
        {
            get {
                return (isQuizRunning && ConnectedAndAuthorized) ;
            }
            set {
                NotifyPropertyChanged();
            }
        }

        public bool IsConnectedAuthorizedAndQuizNotRunning
        {
            get
            {
                return (Connected && Authorized && !IsQuizRunning);

            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public bool IsQuizRunning
        {
            get {
                return isQuizRunning;
            }
            set
            {
                isQuizRunning = value;
                IsConnectedAuthorizedAndQuizRunning = IsConnectedAuthorizedAndQuizRunning;
                IsConnectedAuthorizedAndQuizNotRunning = IsConnectedAuthorizedAndQuizNotRunning;
            }
        }

        public Action<Bitmap> ShowStaff
        {
            get
            {
                return mQE.ShowStaff;
            }
            set
            {
                mQE.ShowStaff = value;
            }
        }

        bool isQuizRunning = false;

        string twitchChannel;

        public int MaxMessagesInChat
        {
            get {
                return maxMessagesInChat;
            }
            set {
                maxMessagesInChat = value;
                NotifyPropertyChanged();
            }
        }

        int maxMessagesInChat = 50;

        void OnNotice(string inString)
        {
            if (NotifyAboutNotices != null)
            {
                NotifyAboutNotices(inString);
            }
        }

        CancellationTokenSource ctsPing;
        public event Action<String> NotifyAboutNotices; 
        List<String> privMessages = new List<string>();
		TcpConnection mTcpConnection;
		Queue<string> mMessageQ = new Queue<string>();
		MemoryStream mMessagesBuffer = new MemoryStream();
		IrcCommandAnalyzer mIrcCommandAnalyzer;
		QuizEngine mQE;
        HttpListener mListener;
        CustomHost host;
        TwitchAuthorization TA = new TwitchAuthorization();
	}
}

