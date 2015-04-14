using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HBase.Client;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System.IO;
using System.Globalization;
using System.Threading;
using Tweetinvi.Core.Interfaces;

namespace Microsoft.Azure.HDInsight.Sample.SocialArchiveApplication
{
    public class HBaseWriter
    {
        HBaseClient client;
        //string tableByIdName = "tweets_by_id";
        
        const string TABLE_NAME = "tweet_dvr";


        long rowCount = 0;
        Dictionary<string, DictionaryItem> dictionary;


        public HBaseWriter()
        {
          
            var credentials = new HBase.Client.ClusterCredentials(
                    new Uri(ConfigurationManager.AppSettings["cluster_uri"]), 
                    ConfigurationManager.AppSettings["cluster_username"], 
                    ConfigurationManager.AppSettings["cluster_password"]
                );

            
            client = new HBaseClient(credentials);

            if (!client.ListTables().name.Contains(TABLE_NAME))
            {
                // Create the table
                var tableSchema = new TableSchema();
                tableSchema.name = TABLE_NAME;
                tableSchema.columns.Add(new ColumnSchema { name = "d" });
                client.CreateTable(tableSchema);
                Console.WriteLine("Table \"{0}\" created.", TABLE_NAME);
            }
        }

        public void WriteTweets(string topic, IEnumerable<Tweetinvi.Core.Interfaces.ITweet> tweets)
        {
            LoadDictionary();

            CellSet cs = new CellSet();
            foreach (var tweet in tweets)
            {
                ++rowCount;
                var words = tweet.Text.ToLower().Split(_punctuationChars);
                int sentimentScore = CalcSentimentScore(words);
                Console.WriteLine("Score: {0}, Tweet: {1}", sentimentScore, tweet.Text);
                var time_index = ((ulong)tweet.CreatedAt.ToBinary()).ToString().PadLeft(20, '0');
                var key = topic + "_" + time_index;
                var row = new CellSet.Row { key = Encoding.UTF8.GetBytes(key) };

                
                row.values.Add( new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:id_str"),
                    data = Encoding.UTF8.GetBytes(tweet.IdStr)
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:created_at"),
                    data = Encoding.UTF8.GetBytes(tweet.CreatedAt.ToString())
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:screen_name"),
                    data = Encoding.UTF8.GetBytes(tweet.Creator.ScreenName)
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:profile_image_url"),
                    data = Encoding.UTF8.GetBytes(tweet.Creator.ProfileImageUrl)
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:name"),
                    data = Encoding.UTF8.GetBytes(tweet.Creator.Name)
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:text"),
                    data = Encoding.UTF8.GetBytes(tweet.Text)
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:retweet_count"),
                    data = Encoding.UTF8.GetBytes(tweet.RetweetCount.ToString())
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:favourite_count"),
                    data = Encoding.UTF8.GetBytes(tweet.FavouriteCount.ToString())
                });
                row.values.Add(new Cell
                { 
                    column = Encoding.UTF8.GetBytes("d:hashtags"),
                    data = Encoding.UTF8.GetBytes(string.Join(",",tweet.Hashtags.Select(x=> x.Text).ToArray()))
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:sentiment_score"),
                    data = Encoding.UTF8.GetBytes(sentimentScore.ToString())
                });
                row.values.Add(new Cell
                {
                    column = Encoding.UTF8.GetBytes("d:order_index"),
                    data = Encoding.UTF8.GetBytes(rowCount.ToString())
                });

                cs.rows.Add(row);           
            }
            //client.StoreCells(TABLE_NAME, cs);
            Console.WriteLine("You have written {0} rows", cs.rows.Count);
        }

        private int CalcSentimentScore(string[] words)
        {
            var total = 0;
            foreach (var word in words)
            {
                if (dictionary.Keys.Contains(word))
                {
                    switch (dictionary[word].Polarity)
                    {
                        case "negative": total -= 1; break;
                        case "positive": total += 1; break;
                    }
                }
            }
            if (total > 0)
            {
                return 1;
            }
            else if (total < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private void LoadDictionary()
        {
            List<string> lines = File.ReadAllLines(@"..\..\data\dictionary.tsv").ToList();
            var items = lines.Select(line =>
            {
                var fields = line.Split('\t');
                var pos = 0;
                return new DictionaryItem
                {
                    Type = fields[pos++],
                    Length = Convert.ToInt32(fields[pos++]),
                    Word = fields[pos++],
                    Pos = fields[pos++],
                    Stemmed = fields[pos++],
                    Polarity = fields[pos++]
                };
            });

            dictionary = new Dictionary<string, DictionaryItem>();
            foreach (var item in items)
            {
                if (!dictionary.Keys.Contains(item.Word))
                {
                    dictionary.Add(item.Word, item);
                }
            }
        }

        private static char[] _punctuationChars = new[] { 
            ' ', '!', '\"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',   //ascii 23--47
            ':', ';', '<', '=', '>', '?', '@', '[', ']', '^', '_', '`', '{', '|', '}', '~' };   //ascii 58--64 + misc.


 

    }
    public class DictionaryItem
    {
        public string Type { get; set; }
        public int Length { get; set; }
        public string Word { get; set; }
        public string Pos { get; set; }
        public string Stemmed { get; set; }
        public string Polarity { get; set; }
    }

}

