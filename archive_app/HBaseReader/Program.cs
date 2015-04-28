using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HBase;
using Microsoft.HBase.Client;
using org.apache.hadoop.hbase.rest.protobuf.generated;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var credentials = new Microsoft.HBase.Client.ClusterCredentials(
                    new Uri(ConfigurationManager.AppSettings["cluster_uri"]),
                    ConfigurationManager.AppSettings["cluster_username"],
                    ConfigurationManager.AppSettings["cluster_password"]
                );
            var table = ConfigurationManager.AppSettings["table_name"];
            var client = new HBaseClient(credentials);
            var scanSettings = new Scanner()
            {
                batch = 10,
                startRow = BitConverter.GetBytes(0),
                endRow = BitConverter.GetBytes(100)
            };
            var scannerInfo = client.CreateScanner(table, scanSettings);
            CellSet next = null;
            while ((next = client.ScannerGetNext(scannerInfo)) != null)
            {
                foreach (var row in next.rows)
                {
                    Encoding.UTF8.GetString(row.key);
                }
            }
            Console.WriteLine("All done");
            Console.ReadLine();

        }
    }
}
