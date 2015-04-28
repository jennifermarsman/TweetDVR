// inspired by Maxim's awesome sentiment sample
// https://github.com/maxluk/tweet-sentiment/blob/master/tweet-sentiment/SimpleStreamingService/Program.cs 



using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.Models.Parameters;


namespace Microsoft.Azure.HDInsight.Sample.SocialArchiveApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitterCredentials.SetCredentials(
                ConfigurationManager.AppSettings["token_AccessToken"],
                ConfigurationManager.AppSettings["token_AccessTokenSecret"],
                ConfigurationManager.AppSettings["token_ConsumerKey"],
                ConfigurationManager.AppSettings["token_ConsumerSecret"]);
            
            int rateLimit = 180;
            int counter = 101344; //TODO set me
            long max_id = 587397633007837184; //TODO set me
            while (rateLimit > 10 )
            {
                var rateLimits = RateLimit.GetCurrentCredentialsRateLimits();
                rateLimit = rateLimits.SearchTweetsLimit.Remaining;
                Console.WriteLine("You can access your timeline {0} times.", rateLimit);
                var param = CreateSearchParam("#got OR #gameofthrones", max_id);
                Console.WriteLine("Searching ...");
                var search = Search.SearchTweets(param);
                max_id = search.Last().Id;
                Console.WriteLine("Count: {0} First Time: {1} Last Time: {2} Min ID {3}", search.Count().ToString(), search.First().CreatedAt.ToString(), search.Last().CreatedAt.ToString(), max_id);
                Console.WriteLine("Adding to HBase ...");
                counter+= search.Count();
                ProcessTweets("GameOfThrones", search, counter);
                Console.WriteLine("Current Counter: {0}", counter);
            }

                /*
                var rateLimits = RateLimit.GetCurrentCredentialsRateLimits();
                Console.WriteLine("You can access your timeline {0} times.", rateLimits.SearchTweetsLimit.Remaining);

                var param = CreateSearchParam("#got OR #gameofthrones", 588126231171858433);
                var search = Search.SearchTweets(param);

                param = CreateSearchParam("#got OR #gameofthrones", search.Last().Id);
                var search2 = Search.SearchTweets(param);

                ProcessTweets("GameOfThrones", search);
                */
                //param = CreateSearchParam("#GameOfThrones");
                //search = Search.SearchTweets(param);

                //ProcessTweets("GameOfThrones", search);
            
            Console.WriteLine("Success");
            Console.ReadLine();



        }

        private static ITweetSearchParameters CreateSearchParam(string topic, long max_id)
        {
            var param = Search.CreateTweetSearchParameter(topic);

            param.TweetSearchType = TweetSearchType.OriginalTweetsOnly;
            param.Lang = Language.English;
            param.MaxId = max_id;
            param.SearchType = SearchResultType.Mixed;
            param.TweetSearchType = TweetSearchType.OriginalTweetsOnly;
            param.MaximumNumberOfResults = 1000;
            return param;
        }

        private static void ProcessTweets(string topic, IEnumerable<Tweetinvi.Core.Interfaces.ITweet> search, int counter)
        {
            var hbw = new HBaseWriter(counter);
            hbw.WriteTweets(topic, search);
        }
    }
}
