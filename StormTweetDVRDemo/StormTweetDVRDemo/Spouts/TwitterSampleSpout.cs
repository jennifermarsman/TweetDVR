using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.SCP;
using Tweetinvi;
using System.Configuration;
using Tweetinvi.Core.Interfaces;
using System.Text;

namespace StormTweetDVRDemo.Spouts
{
    public class TwitterSampleSpout : ISCPSpout
    {
        Context context;
        Queue<SerializableTweet> queue = new Queue<SerializableTweet>();
        string topic = "GameOfThrones";
        List<string> hashtags = new List<string>(); 

        //TODO: Use a cache if you want to re-emit tuples on fail
        //Dictionary<long, SerializableTweet> cache = new Dictionary<long, SerializableTweet>();

        public TwitterSampleSpout(Context context)
        {
            this.context = context;
            hashtags.Add("GameOfThrones");
            hashtags.Add("#GoT");
            
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(SerializableTweet) });
            this.context.DeclareComponentSchema(new ComponentStreamSchema(null, outputSchema));

            //TODO: Specify your twitter credentials in SCPHost.exe.config
            TwitterCredentials.SetCredentials(
                ConfigurationManager.AppSettings["TwitterAccessToken"],
                ConfigurationManager.AppSettings["TwitterAccessTokenSecret"],
                ConfigurationManager.AppSettings["TwitterConsumerKey"],
                ConfigurationManager.AppSettings["TwitterConsumerSecret"]);

            //TODO: Setup a Twitter Stream
            //CreateSampleStream();
            CreateFilteredStream();
        }

        public void CreateSampleStream()
        {
            var stream = Tweetinvi.Stream.CreateSampleStream();
            stream.TweetReceived += (sender, args) => { NextTweet(args.Tweet); };
            stream.StartStreamAsync();
        }

        public void CreateFilteredStream()
        {
            var stream = Tweetinvi.Stream.CreateFilteredStream();
            stream.MatchingTweetReceived += (sender, args) => { NextTweet(args.Tweet); };

            //TODO: Setup your filter criteria
            //TODO: filter on language, add time, etc.
            hashtags.ForEach(delegate(String name)
            {
                stream.AddTrack(name);
            });
            //stream.AddTrack("GameofThrones");
            //stream.AddTrack("#GoT");

//            stream.StartStreamMatchingAllConditionsAsync();
            stream.StartStreamMatchingAnyConditionAsync();
        }

        public static TwitterSampleSpout Get(Context context, Dictionary<string, Object> parms)
        {
            return new TwitterSampleSpout(context);
        }

        /// <summary>
        /// The twitter async stream methods call this method to queue the tweets
        /// </summary>
        /// <param name="tweet"></param>
        public void NextTweet(ITweet tweet)
        {
            // Create comma-separated list of hashtags
            StringBuilder sb = new StringBuilder();
            hashtags.ForEach(delegate(String name)
            {
                if (sb.Length != 0) sb.Append(", ");        // won't do first time through
                sb.Append(name);
            });

            queue.Enqueue(new SerializableTweet(tweet, topic, sb.ToString()));
        }

        public void NextTuple(Dictionary<string, Object> parms)
        {
            if (queue.Count > 0)
            {
                var tweet = queue.Dequeue();
                context.Emit(new Values(tweet));
                Context.Logger.Info("NextTuple: Emitted Tweet = {0}", tweet.Text);
            }
            else
            {
                //Free up some CPU cycles if no tweets are being received
                Thread.Sleep(50);
            }
        }

        public void Ack(long seqId, Dictionary<string, Object> parms)
        {
            //do nothing - optionally you can cache the tuple and remove them in this method
            //cache.Remove(seqId);
        }

        public void Fail(long seqId, Dictionary<string, Object> parms)
        {
            //do nothing - optionally you can cache the tuples and re-emit them in this method
            //this.context.Emit(cache[seqId]);
        }
    }
}