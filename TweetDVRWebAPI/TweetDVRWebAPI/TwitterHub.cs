using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace TweetDVRWebAPI
{
    public class TwitterHub : Hub
    {
        public void ShowTweet(string topic, string tweetId)
        {
            Clients.All.showTweet(topic, tweetId);
        }
    }
}