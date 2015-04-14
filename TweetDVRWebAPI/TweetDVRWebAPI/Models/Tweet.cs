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
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public string FavouriteCount { get; set; }
        public string Hashtags { get; set; }
        public string RetweetCount { get; set; }
        public string ScreenName { get; set; }
    }
}
