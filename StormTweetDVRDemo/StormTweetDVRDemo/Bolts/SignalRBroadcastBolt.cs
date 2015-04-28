using System;
using System.Collections.Generic;
using Microsoft.SCP;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Configuration;

namespace StormTweetDVRDemo.Bolts
{
    public class SignalRBroadcastBolt : ISCPBolt
    {
        Context context;

        //SingnalR Connection
        HubConnection hubConnection;
        IHubProxy twitterHubProxy;
        Stopwatch timer = Stopwatch.StartNew();

        //Constructor
        public SignalRBroadcastBolt(Context context)
        {
            Context.Logger.Info("SignalRBroadcastBolt constructor called");
            //Set context
            this.context = context;

            //Define the schema for the incoming tuples from spout
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            //Input schema counter updates
            //inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(long), typeof(string) });
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(string), typeof(string), typeof(DateTime), typeof(bool),
            typeof(string), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(string), typeof(long), typeof(string)});


            //Declare both incoming and outbound schemas
            this.context.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));

            // Initialize SignalR connection
            StartSignalRHubConnection();
        }

        //Get a new instance
        public static SignalRBroadcastBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new SignalRBroadcastBolt(ctx);
        }

        //Process a tuple from the stream
        public void Execute(SCPTuple tuple)
        {
            Context.Logger.Info("Execute enter");

            var creatorName = tuple.GetValue(0) as string;
            var creatorScreenName = tuple.GetValue(1) as string;
            var createProfileImageUrl = tuple.GetValue(2) as string;
            var createdAt = (DateTime) tuple.GetValue(3);
            var isRetweet = (bool) tuple.GetValue(4);
            var language = tuple.GetValue(5) as string;
            var retweeted = (bool)tuple.GetValue(6);
            var text = tuple.GetValue(7) as string;
            var idStr = tuple.GetValue(8) as string;
            var topic = tuple.GetValue(9) as string;
            var retweetCount = (int) tuple.GetValue(10);
            var favouriteCount = (int) tuple.GetValue(11);
            var hashtags = tuple.GetValue(12) as string;
            var orderIndex = (long)tuple.GetValue(13);
            var sentiment = tuple.GetValue(14) as string;

            try
            {
                //Only send updates every 500 milliseconds
                //Ignore the messages in between so that you don't overload the SignalR website with updates at each tuple
                //If you have only aggreagates to send that can be spaced, you don't need this timer
                if (timer.ElapsedMilliseconds >= 100)
                {
                    SendSignalRUpdate(orderIndex, "Jen! " + text + " " + createdAt.ToString());
                    timer.Restart();
                }
            }
            catch (Exception ex)
            {
                Context.Logger.Error("SignalRBroadcastBolt Exception: " + ex.Message + "\nStackTrace: \n" + ex.StackTrace);
            }

            Context.Logger.Info("Execute exit");
        }

        private void StartSignalRHubConnection()
        {
            //TODO: Specify your SignalR website settings in SCPHost.exe.config
            this.hubConnection = new HubConnection(ConfigurationManager.AppSettings["SignalRWebsiteUrl"]);
            this.twitterHubProxy = hubConnection.CreateHubProxy(ConfigurationManager.AppSettings["SignalRHub"]);
            hubConnection.Start().Wait();
        }

        private void SendSignalRUpdate(long rowCount, string tweet)
        {
            if (hubConnection.State != ConnectionState.Connected)
            {
                hubConnection.Stop();
                StartSignalRHubConnection();
            }
            twitterHubProxy.Invoke("UpdateCounter", rowCount, tweet);       /// TODO Jen: change this
        }
    }
}
