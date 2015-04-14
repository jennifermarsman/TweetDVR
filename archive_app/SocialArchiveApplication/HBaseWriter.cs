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
            CellSet cs = new CellSet();
            foreach (var tweet in tweets)
            {
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
                    column = Encoding.UTF8.GetBytes("d:screen_name"),
                    data = Encoding.UTF8.GetBytes(tweet.Creator.ScreenName)
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
                    column = Encoding.UTF8.GetBytes("d:hashtags"),
                    data = Encoding.UTF8.GetBytes(string.Join(",",tweet.Hashtags.Select(x=> x.Text).ToArray()))
                });
                cs.rows.Add(row);           
            }
            client.StoreCells(TABLE_NAME, cs);
            Console.WriteLine("You have written {0} rows", cs.rows.Count);
        }


        private static char[] _punctuationChars = new[] { 
            ' ', '!', '\"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',   //ascii 23--47
            ':', ';', '<', '=', '>', '?', '@', '[', ']', '^', '_', '`', '{', '|', '}', '~' };   //ascii 58--64 + misc.


 

    }

}

