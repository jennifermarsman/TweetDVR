using System;
using Tweetinvi.Core.Interfaces;

namespace StormTweetDVRDemo
{
    //Workaround to make the Tweet from TweetInvi Serializable
    //Mark this class Serializable
    [Serializable]
    public class SerializableTweet
    {

        public string CreatorScreenName { get; set; }      // i.e. mwinkle
        public string CreatorProfileImageUrl { get; set; }
        public string CreatorName { get; set; }            // i.e. Matt Winkler

        public DateTime CreatedAt { get; set; }
        public bool IsRetweet { get; set; }
        public string Language { get; set; }
        public bool Retweeted { get; set; }
        public string Text { get; set; }
        public string IdStr { get; set; }
        public string Topic { get; set; }
        public int RetweetCount { get; set; }
        public int FavouriteCount { get; set; }
        public string Hashtags { get; set; }
        public long OrderIndex { get; set; }
        public string Sentiment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tweet"></param>
        /// <param name="topic"></param>
        /// <param name="hashtags">Comma-separated list of hashtags</param>
        public SerializableTweet(ITweet tweet, string topic, string hashtags)
        {
            this.CreatorName = tweet.Creator.Name;
            this.CreatorScreenName = tweet.Creator.ScreenName;
            this.CreatorProfileImageUrl = tweet.Creator.ProfileImageUrl;
            this.CreatedAt = tweet.CreatedAt;
            this.IsRetweet = tweet.IsRetweet;
            this.Language = tweet.Language.ToString();
            this.Retweeted = tweet.Retweeted;
            this.Text = tweet.Text;
            this.IdStr = tweet.IdStr;
            this.Topic = topic;
            this.RetweetCount = tweet.RetweetCount;
            this.FavouriteCount = tweet.FavouriteCount;
            this.Hashtags = hashtags;
            this.OrderIndex = 0; // TODO: logic Jen come back and fix this
            this.Sentiment = this.Sentiment;
        }
    }

}
