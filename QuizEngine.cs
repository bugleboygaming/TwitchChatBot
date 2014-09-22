using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace TwitchChatBot
{
	public class QuizEngine
	{
		public QuizEngine ()
		{
			mQuizQueue = new Queue<Tuple<string, string>>();
            mIncomingMessagesQueue = new Queue<IrcCommand>();
           
		}

		public QuizEngine (string inFileWithQuiz) : this()
		{
			if (File.Exists (inFileWithQuiz)) {
				using(FileStream fs = File.OpenRead(inFileWithQuiz)){
					string[] linesOfFile = null;
					File.ReadAllLines(inFileWithQuiz);
					ProcessStringsArrayAsQuiz(linesOfFile);
				}
			} else {
				throw new FileNotFoundException();
			}

		}

		public QuizEngine (string[] inQuiz) : this()
		{
			ProcessStringsArrayAsQuiz(inQuiz);

		}

		void ProcessStringsArrayAsQuiz (string[] inStringsArray)
		{
			string[] separators = new string[]{"{Question}","{Answer}"};

			for(int i = 0; i < inStringsArray.Length; i++ ){
				string[] result = inStringsArray[i].Split(separators,StringSplitOptions.RemoveEmptyEntries);
				if(result.Length == 2)
				{
					mQuizQueue.Enqueue(new Tuple<string, string>(result[0],result[1]));
				}

				Console.WriteLine("Strings in result: {0}",result.Length);
				foreach(var str in result){
					Console.WriteLine(str);
				}
			}
		}



		public void Process (IrcCommand inCommand)
		{
			if (inCommand.Name == "PRIVMSG") {
				//int intIndexOfLastParameter = inCommand.Parameters.Length - 1;
				//mQueue.Enqueue(inCommand.Parameters[intIndexOfLastParameter]);
				mIncomingMessagesQueue.Enqueue(inCommand);
			}

		}

        async Task<IrcCommand> GetMessageFromQ()
        { 
            while(mIncomingMessagesQueue.Count == 0)
            {
                await Task.Delay(100);
            }
            return mIncomingMessagesQueue.Dequeue();
        }

        async Task ReadIncomingMessages(CancellationToken ct)
        {
            
            while (true)
            {
                Task<IrcCommand> tic = Task.Run((Func<Task<IrcCommand>>)GetMessageFromQ);
                //IrcCommand ic = await GetMessageFromQ();
                IrcCommand ic = await tic;
                Console.WriteLine("after GetMessageFromQ:{0} ",ic.Name);
            }
        }

        async public void StartQuiz()
        {
            if (cts != null) {
                cts.Cancel();
            }
            cts = new CancellationTokenSource();
            await  ReadIncomingMessages(cts.Token);
        }

		//TODO: make an async read of incoming messages, read them if quiz is currently running, check for a valid answer.
		//TODO: make a timered method, which is extracting quiz pairs from Q over time, assigning to the temp pair

		

		public Action<string> SendMessage;
		Queue<IrcCommand> mIncomingMessagesQueue;
		Queue<Tuple<string,string>> mQuizQueue;
		System.Timers.Timer mTimer;
        CancellationTokenSource cts;
	}
}
