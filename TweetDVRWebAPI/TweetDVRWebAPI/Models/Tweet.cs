using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetDVRWebAPI.Models
{
    public class Tweet
    {
        public string IdStr { get; set; }
        public string Text { get; set; }
        public string Lang { get; set; }
        public int Sentiment { get; set; }
    }
}
