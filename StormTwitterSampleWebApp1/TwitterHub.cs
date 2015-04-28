using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace StormTwitterSampleWebApp1
{
    public class TwitterHub : Hub
    {
        public void UpdateCounter(long rowCount, string tweet)
        {
            Clients.All.updateCounter(rowCount, tweet);
        }
    }
}