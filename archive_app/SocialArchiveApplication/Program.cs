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

        }
    }
}
