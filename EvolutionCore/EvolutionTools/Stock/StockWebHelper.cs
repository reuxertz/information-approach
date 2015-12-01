using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using EvolutionTools;

namespace EvoStockTools
{
    public enum HistInterval { Day, Week, Month };
    public enum MonthAbrev { Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec };

    public static class StockWebHelper
    {
        //Functions

        //Historical Stock Quote - (Month - 1)
        public static string[][] YahooHistoricalStockQuote(string name, string fromDateDay, string fromDateMonth, string fromDateYear, 
            string toDateDay, string toDateMonth, string toDateYear, HistInterval interval)
        {
            var table = new List<string[]>();            

            using (WebClient web = new WebClient())
            {
                var yahooIntervals = new string[] { "d", "w", "m" };
                var yInterval = yahooIntervals[((int)interval)];
                var tempPath = Environment.CurrentDirectory + @"\_tempCSV.csv";
                var URL = string.Format(@"http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g={7}&ignore=.csv",
                    name, fromDateMonth, fromDateDay, fromDateYear, toDateMonth, toDateDay, toDateYear, yInterval);

                //Delete old Temporary File
                if (File.Exists(tempPath))                
                    File.Delete(tempPath);

                //Download Stock Info
                web.DownloadFile(URL, tempPath);

                var rawTextArray = Management.GetTextFromFile(tempPath);
                
                for (int i = 0; i < rawTextArray.Length; i++)
                    table.Add(rawTextArray[i].Split(','));
            }

            return table.ToArray<string[]>();
        }
        public static string[][] YahooHistoricalStockQuote(string name, string fromDateYear, string toDateYear, HistInterval interval)
        {
            return YahooHistoricalStockQuote(name, "1", "0", fromDateYear, "1", "0", toDateYear, interval);
        }

        public static string[][] GoogleHistoricalStockQuote(string name, int startYear, bool AddIndexColumn, out string[] headers, out Type[] dataTypes)
        {
            string[][] table = null;
            headers = null;
            dataTypes = null;
   
            using (WebClient web = new WebClient())
            {
                var tempPath = Environment.CurrentDirectory + @"\_tempCSV.csv";
                var URL = string.Format(@"http://www.google.com/finance/historical?output=csv&q=" + name + 
                    @"&startdate=Jan+1%2C+" + startYear + 
                    @"&num=3000");
                
                //Delete old Temporary File
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                //Download Stock Info
                web.DownloadFile(URL, tempPath);

                var rawTextArray = Management.GetTextFromFile(tempPath);
                dataTypes = new Type[] { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };
                table = new string[rawTextArray.Length - 1][];

                for (int i = 0; i < rawTextArray.Length; i++)
                {
                    var s = rawTextArray[i].Split(',');

                    if (AddIndexColumn)
                    {
                        var ss = s.ToList<string>();

                        if (i == 0)
                        {
                            ss.Insert(0, "Index");
                            var dt = dataTypes.ToList<Type>();
                            dt.Add(typeof(int));
                            dataTypes = dt.ToArray<Type>();
                        }
                        else
                            ss.Insert(0, rawTextArray.Length - i - 1 + "");

                        s = ss.ToArray<string>();
                    }

                    if (i == 0)
                        headers = s;
                    else
                    {
                        var ii = table.Length - (i - 1) - 1;
                        table[ii] = s;
                    }
                }

                web.Dispose();
            }

            //Convert Dates to numerical
            /*
            for (int i = 1; i < table.Count; i++)
            {
                var d = table[i][0].Split('-');
                
                var month = d[1];
                var day = d[0];
                var year = d[2];

                for (int j = 0; j < 12; j++)
                {
                    var t = ((MonthAbrev)j).ToString();
                    if (t == month)
                        month = "" + j;
                }

                table[i][0] = year + @"/" + month + @"/" + day;
            }*/

            return table.ToArray<string[]>();
        }











    }
}
