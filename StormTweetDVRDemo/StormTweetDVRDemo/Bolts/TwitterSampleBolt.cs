using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SCP;


namespace StormTweetDVRDemo.Bolts
{
    /// <summary>
    /// This twitter sample bolt inherits from SqlAzureSampleBolt
    /// </summary>
    public class TwitterSampleBolt : ISCPBolt
    {
        Context context;
        long count = 0;
        Dictionary<string, DictionaryItem> dictionary;


        public TwitterSampleBolt(Context context, Dictionary<string, Object> parms)
        {
            this.context = context;

            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(SerializableTweet) });

            // Our output bolt is sending the tweet's text, the created time, a count, and hashtags
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            //outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(long), typeof(string) });
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(string), typeof(string), typeof(DateTime), typeof(bool),
            typeof(string), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(string), typeof(long), typeof(string)});

            this.context.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));
        }

        public static TwitterSampleBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new TwitterSampleBolt(ctx, parms);
        }

        public void Execute(SCPTuple tuple)
        {
            var tweet = tuple.GetValue(0) as SerializableTweet;
            ExecuteTweet(tweet);
        }

        /// <summary>
        /// This is where you business logic around a tweet will go
        /// Here we are emitting the count and the tweet text to next bolt
        /// And also inserting the tweet into SQL Azure
        /// </summary>
        /// <param name="tweet"></param>
        public void ExecuteTweet(SerializableTweet tweet)
        {
            //LoadDictionary();

            count++;
            Context.Logger.Info("ExecuteTweet: Count = {0}, Tweet = {1}, Time={2}", count, tweet.Text, tweet.CreatedAt);

            //TODO: You can do something on other tweet fields
            //Like aggregations on tweet.Language etc

            // Calculating sentiment score
            //var words = tweet.Text.ToLower().Split(_punctuationChars);
            //int sentimentScore = CalcSentimentScore(words);

            //Emit the value to next bolt - SignalR & SQL Azure
            //Ensure that subsequent bolts align with the data fields and types you send
            //this.context.Emit(new Values(count, tweet.Text));
//            this.context.Emit(new Values(count, tweet.Text, tweet.CreatedAt));
//            this.context.Emit(new Values(tweet.IdStr, tweet.Creator.ScreenName, tweet.Creator.ProfileImageUrl, tweet.Creator.Name, tweet.Text, tweet.RetweetCount, tweet.FavouriteCount, tweet.Hashtags));
            this.context.Emit(new Values(tweet.CreatorName, tweet.CreatorScreenName, tweet.CreatorProfileImageUrl, tweet.CreatedAt, tweet.IsRetweet,
                tweet.Language.ToString(), tweet.Retweeted, tweet.Text, tweet.IdStr, tweet.Topic, tweet.RetweetCount, tweet.FavouriteCount, tweet.Hashtags, count, tweet.Sentiment));
        }

        private int CalcSentimentScore(string[] words)
        {
            var total = 0;
            foreach (var word in words)
            {
                if (dictionary.Keys.Contains(word))
                {
                    switch (dictionary[word].Polarity)
                    {
                        case "negative": total -= 1; break;
                        case "positive": total += 1; break;
                    }
                }
            }
            if (total > 0)
            {
                return 1;
            }
            else if (total < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private void LoadDictionary()
        {
            List<string> lines = File.ReadAllLines(@"..\..\data\dictionary.tsv").ToList();
            var items = lines.Select(line =>
            {
                var fields = line.Split('\t');
                var pos = 0;
                return new DictionaryItem
                {
                    Type = fields[pos++],
                    Length = Convert.ToInt32(fields[pos++]),
                    Word = fields[pos++],
                    Pos = fields[pos++],
                    Stemmed = fields[pos++],
                    Polarity = fields[pos++]
                };
            });

            dictionary = new Dictionary<string, DictionaryItem>();
            foreach (var item in items)
            {
                if (!dictionary.Keys.Contains(item.Word))
                {
                    dictionary.Add(item.Word, item);
                }
            }
        }

        private static char[] _punctuationChars = new[] { 
            ' ', '!', '\"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',   //ascii 23--47
            ':', ';', '<', '=', '>', '?', '@', '[', ']', '^', '_', '`', '{', '|', '}', '~' };   //ascii 58--64 + misc.

        public class DictionaryItem
        {
            public string Type { get; set; }
            public int Length { get; set; }
            public string Word { get; set; }
            public string Pos { get; set; }
            public string Stemmed { get; set; }
            public string Polarity { get; set; }
        }


    }
}