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

                        list.Add(new Tweet
                        {
                            IdStr = id,
                            CreatedAt = createdAt
                        });

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