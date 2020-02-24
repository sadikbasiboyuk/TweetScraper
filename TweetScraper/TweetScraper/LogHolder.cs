using System;
using System.IO;

namespace TweetScraper
{
    public static class LogHolder
    {
        static TextWriter driverLog, tweetLog;

        public static void CreateLogFile(int select)
        {
            if (select == 1)
                driverLog = new StreamWriter("DriverLogs.txt", true);
            else if (select == 2)
                tweetLog = new StreamWriter("tweetLogs.txt", true);
        }

        public static void AddLogFile(string logStr, int select)
        {
            if (select == 1)
                Logtxt(logStr, driverLog);
            else if (select == 2)
                Logtxt(logStr, tweetLog);
        }

        public static void CloseLogFile(int select)
        {
            if (select == 1)
                driverLog.Close();
            else if (select == 2)
                tweetLog.Close();
        }

        private static void Logtxt(string logMessage, TextWriter file)
        {
            if (file == driverLog)
            {
                try
                {
                    file.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString() + " --> " + logMessage);
                    file.WriteLine("-------------------------------");
                }
                catch (Exception)
                {
                }
            }
            else if (file == tweetLog)
            {
                try
                {
                    file.WriteLine(logMessage);
                    file.WriteLine("-------------------------------");
                }
                catch (Exception)
                {
                }
            }

        }
    }
}