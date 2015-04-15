﻿using Microsoft.HBase.Client;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TweetDVRWebAPI.Models
{
    public class HBaseReader
    {
        HBaseClient client;
        string tweetDvrTableName = "tweet_dvr";

        public HBaseReader()
        {
            var creds = new ClusterCredentials(
                            new Uri(ConfigurationManager.AppSettings["Cluster"]),
                            ConfigurationManager.AppSettings["User"],
                            ConfigurationManager.AppSettings["Pwd"]);
            client = new HBaseClient(creds);
        }

        public async Task<IEnumerable<Tweet>> QueryTweetsAsync(string topic, DateTime time, string keyword, int maxCount, int sentimentFilter)
        {
            if (maxCount == 0)
            {
                maxCount = 100;
            }

            var list = new List<Tweet>();

            var time_index = (time.ToString("yyyyMMddHHmmss-") + time.Millisecond.ToString());
            var startRow = topic + "_" + time_index;
            var endRow = topic + "|";
            var scanSettings = new Scanner
            {
                batch = maxCount,
                startRow = Encoding.UTF8.GetBytes(startRow),
                endRow = Encoding.UTF8.GetBytes(endRow)
            };
            ScannerInformation scannerInfo =
                await client.CreateScannerAsync(tweetDvrTableName, scanSettings);

            CellSet next;
            while ((next = await client.ScannerGetNextAsync(scannerInfo)) != null)
            {
                foreach (CellSet.Row row in next.rows)
                {
                    var idField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:id_str");
                    var id = "";
                    if (idField != null)
                    {
                        id = Convert.ToString(Encoding.UTF8.GetString(idField.data));
                    }

                    var createdAtField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:created_at");
                    DateTime createdAt = DateTime.MinValue;
                    if (createdAtField != null)
                    {
                        createdAt = Convert.ToDateTime(Encoding.UTF8.GetString(createdAtField.data));
                    }

                    var textField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:text");
                    var text = "";
                    if (textField != null)
                    {
                        text = Convert.ToString(Encoding.UTF8.GetString(textField.data));
                    }

                    var screenNameField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:screen_name");
                    var screenName = "";
                    if (screenNameField != null)
                    {
                        screenName = Convert.ToString(Encoding.UTF8.GetString(screenNameField.data));
                    }

                    var profileImageUrlField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:profile_image_url");
                    var profileImageUrl = "";
                    if (profileImageUrlField != null)
                    {
                        profileImageUrl = Convert.ToString(Encoding.UTF8.GetString(profileImageUrlField.data));
                    }

                    var nameField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:name");
                    var name = "";
                    if (nameField != null)
                    {
                        name = Convert.ToString(Encoding.UTF8.GetString(nameField.data));
                    }

                    var retweetCountField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:retweet_count");
                    var retweetCount = "";
                    if (retweetCountField != null)
                    {
                        retweetCount = Convert.ToString(Encoding.UTF8.GetString(retweetCountField.data));
                    }

                    var hashtagsField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:hashtags");
                    var hashtags = "";
                    if (hashtagsField != null)
                    {
                        hashtags = Convert.ToString(Encoding.UTF8.GetString(hashtagsField.data));
                    }

                    var favouriteCountField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:favourite_count");
                    var favouriteCount = "";
                    if (favouriteCountField != null)
                    {
                        favouriteCount = Convert.ToString(Encoding.UTF8.GetString(favouriteCountField.data));
                    }

                    var sentimentField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:sentiment_score");
                    var sentiment = 0;
                    if (sentimentField != null)
                    {
                        sentiment = Convert.ToInt32(Encoding.UTF8.GetString(sentimentField.data));
                    }

                    if (!string.IsNullOrEmpty(text) && (sentimentFilter == 3 || sentimentFilter == sentiment))
                    {
                        list.Add(new Tweet
                        {
                            IdStr = id,
                            CreatedAt = createdAt,
                            Text = text,
                            Name = name,
                            ProfileImageUrl = profileImageUrl,
                            Sentiment = sentiment,
                            FavouriteCount = favouriteCount,
                            Hashtags = hashtags,
                            RetweetCount = retweetCount,
                            ScreenName = screenName
                        });
                    }

                    if (list.Count >= maxCount)
                    {
                        break;
                    }
                }

                if (list.Count >= maxCount)
                {
                    break;
                }
            }

            return list;
        }

        internal async Task<bool> CheckTable()
        {
            return (await client.ListTablesAsync()).name.Contains(tweetDvrTableName);
        }
    }
}