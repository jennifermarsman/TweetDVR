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


            var param = CreateSearchParam("#GOT");
            var search = Search.SearchTweets(param);

            ProcessTweets("GameOfThrones", search);

            param = CreateSearchParam("#GameOfThrones");
            search = Search.SearchTweets(param);

            ProcessTweets("GameOfThrones", search);

            param = CreateSearchParam("#MSBuild");
            search = Search.SearchTweets(param);

            ProcessTweets("Build", search);


            Console.WriteLine("Success");
            Console.ReadLine();



        }

        private static ITweetSearchParameters CreateSearchParam(string topic)
        {
            var param = Search.CreateTweetSearchParameter(topic);

            param.TweetSearchType = TweetSearchType.OriginalTweetsOnly;
            param.Lang = Language.English;
            param.MaximumNumberOfResults = 1000;
            return param;
        }

        private static void ProcessTweets(string topic, IEnumerable<Tweetinvi.Core.Interfaces.ITweet> search)
        {
            var hbw = new HBaseWriter();
            hbw.WriteTweets(topic, search);
        }
    }
}
