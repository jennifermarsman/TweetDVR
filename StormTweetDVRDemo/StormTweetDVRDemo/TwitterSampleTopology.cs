using System;
using System.Collections.Generic;
using Microsoft.SCP;
using Microsoft.SCP.Topology;
using StormTweetDVRDemo.Spouts;
using StormTweetDVRDemo.Bolts;

namespace StormTweetDVRDemo
{
    [Active(true)]
    class TwitterSampleTopology : TopologyDescriptor
    {
        public ITopologyBuilder GetTopologyBuilder()
        {
            TopologyBuilder topologyBuilder = new TopologyBuilder(typeof(TwitterSampleTopology).Name + DateTime.Now.ToString("-yyyyMMddHHmmss"));

            topologyBuilder.SetSpout(
                typeof(TwitterSampleSpout).Name,
                TwitterSampleSpout.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){"tweet"}}
                },
                1);

            topologyBuilder.SetBolt(
                typeof(TwitterSampleBolt).Name,
                TwitterSampleBolt.Get,
                new Dictionary<string, List<string>>() 
                {
//                    {Constants.DEFAULT_STREAM_ID, new List<string>(){"count", "tweet"}}
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){"creatorName", "creatorScreenName", "createProfileImageUrl", "createdAt", "isRetweet", "language",
                    "retweeted", "text", "idStr", "topic", "retweetCount", "favouriteCount", "hashtags", "orderIndex", "sentiment"}}
                },
                1).shuffleGrouping(typeof(TwitterSampleSpout).Name);

            topologyBuilder.SetBolt(
                typeof(HBaseBolt).Name,
                HBaseBolt.Get,
                new Dictionary<string, List<string>>() { 
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){"count", "tweet", "time"}}
                },
                1).shuffleGrouping(typeof(TwitterSampleBolt).Name);

            topologyBuilder.SetBolt(
                typeof(SignalRBroadcastBolt).Name,
                SignalRBroadcastBolt.Get,
                new Dictionary<string, List<string>>(),
                1).shuffleGrouping(typeof(TwitterSampleBolt).Name);
            return topologyBuilder;
        }
    }
}

