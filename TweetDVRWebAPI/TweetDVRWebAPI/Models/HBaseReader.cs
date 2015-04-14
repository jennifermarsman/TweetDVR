using Microsoft.HBase.Client;
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

        public async Task<IEnumerable<Tweet>> QueryTweetsAsync(string topic, DateTime time, string keyword, int maxCount)
        {
            if (maxCount == 0)
            {
                maxCount = 100;
            }

            var list = new List<Tweet>();

            var time_index = ((ulong)time.ToBinary()).ToString().PadLeft(20, '0');
            var startRow = topic + "_";// + time_index;
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

                    var sentimentField =
                        row.values.Find(c => Encoding.UTF8.GetString(c.column) == "d:sentiment");
                    var sentiment = 0;
                    if (sentimentField != null)
                    {
                        sentiment = Convert.ToInt32(Encoding.UTF8.GetString(sentimentField.data));
                    }

                    list.Add(new Tweet
                    {
                        Text = text,
                        Sentiment = sentiment
                    });
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