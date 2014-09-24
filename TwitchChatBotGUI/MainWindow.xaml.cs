﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

using TwitchChatBot;

namespace TwitchChatBotGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bot = new TwitchBot();
            bot.Destination = new Endpoint();
            bot.Destination.EndpointAddress = "irc.twitch.tv";
            bot.Destination.EndpointPort = 6667;
        }

        private void ProxyAddressChanged(object sender, TextChangedEventArgs e)
        {

        }

        

        private void ProxyPortChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        TwitchBot bot;

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            if(ProxyAddress.Text != "" && ProxyPort.Text != ""){
                bot.Proxy = new Endpoint();
                bot.Proxy.EndpointAddress = ProxyAddress.Text;
                bot.Proxy.EndpointPort = Int32.Parse(ProxyPort.Text);
            }
            bot.Connect();

            SendButton.IsEnabled = true;
            StartQuizButton.IsEnabled = true;
            //https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=amoyxo9a7agc0e1gjpcawa1rqb2ciy4&redirect_uri=http://localhost:6555
            bot.SendMessage("PASS oauth:lxubjjlsavkv1o3ih44d3csztfpw7vu\r\n");
            bot.SendMessage("NICK sovietmade\r\n");
            bot.SendMessage("JOIN #sovietmade\r\n");
            //bot.SendMessage("PRIVMSG #sovietmade :test\r\n");
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageBox.Text;
            IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter(messageToSend, true));
            bot.SendMessage(ic.ToString() + "\r\n");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer--;
        }
        async Task CountDown(CancellationToken ct)
        {
            while (timer >= 0)
            {
                TimerLabel.Content = String.Format("Quiz starting in {0}",timer);
                await Task.Delay(100);
            }
        }

        async Task ShowTimer(CancellationToken ct) 
        {
            t = new System.Timers.Timer(1000);
            t.Elapsed += timer1_Tick;
            t.Start();

            await CountDown(ct);
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StartQuizCT != null) {
                if (t != null)
                {
                    t.Stop();
                }
                StartQuizCT.Cancel();
            }
            StartQuizCT = new CancellationTokenSource();
            IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter("Starting Quiz in 60 seconds!", true));
            bot.SendMessage(ic.ToString() + "\r\n");
            bot.StartQuiz();
            TimerLabel.Content = 60;
            timer = 60;
            await ShowTimer(StartQuizCT.Token);
            TimerLabel.Content = "Quiz is running...";
        }

        CancellationTokenSource StartQuizCT;
        int timer;
        System.Timers.Timer t;
        
    }
}