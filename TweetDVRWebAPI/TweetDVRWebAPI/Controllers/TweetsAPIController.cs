using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TweetDVRWebAPI.Models;

namespace TweetDVRWebAPI.Controllers
{
    public class TweetsAPIController : ApiController
    {
        HBaseReader hbase = new HBaseReader();

        public async Task<IEnumerable<Tweet>> GetTweets(string topic, DateTime time, string keyword = null, int maxCount = 100)
        {
            return await hbase.QueryTweetsAsync(topic, time, keyword, maxCount);
        }
    }
}
