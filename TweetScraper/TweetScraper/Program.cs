using System;
using System.Collections.Generic;
using System.Linq;

namespace TweetScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool loop = true;
            string takenInput;
            // Create log files to keep errors taken from driver and tweets that are scrapped
            LogHolder.CreateLogFile(1);
            LogHolder.CreateLogFile(2);

            Console.WriteLine("Hello! Welcome to TScraper. TScraper keeps all the tweets that sent for given username into the text file.");

            while (loop)
            {
                Console.WriteLine("\n\nSelect what you want:" +
                                  "\n1->  Just search for username, it won't work for protected accounts" +
                                  "\n2->  Enter twitter using username and password, then search for username you can access protected user's tweets if " +
                                  "you are following him/her" +
                                  "\n0->  Exit from the program");
                takenInput = Console.ReadLine();
                switch (takenInput)
                {
                    case "1":
                        twitterWithoutLoggingIn();
                        break;
                    case "2":
                        twitterWithLoggingIn();
                        break;
                    case "0":
                        Console.WriteLine("Good Bye!\n");
                        loop = false;
                        break;
                    default:
                        Console.WriteLine("Please enter a valid input\n\n");
                        break;
                }
            }
            LogHolder.CloseLogFile(1);
            LogHolder.CloseLogFile(2);
            Console.ReadLine();
        }

        // Open browser with twitter url and close it when it's done
        private static void twitterWithoutLoggingIn()
        {
            Driver driver = new Driver("https://www.twitter.com/");
            scrapTweets(driver);
            driver.DriverQuit();
        }

        // Open browser with twitter url, take username and password and scrap tweets, close it when it's done
        private static void twitterWithLoggingIn()
        {
            Driver driver = new Driver("https://twitter.com/login");
            pageLoading(driver);
            while (true)
            {
                Console.WriteLine("Please enter your phone number, e-mail, or username\t");
                string username = Console.ReadLine();
                driver.FillElement("Username", username, "Twitter");

                Console.WriteLine("Please enter password\t");
                string password = Console.ReadLine();
                driver.FillElement("Password", password, "Twitter");

                driver.ClickElement("LoginButton", "Twitter");
                System.Threading.Thread.Sleep(1000);

                if (driver.CheckElementAvailable("LoginError", 0, "Twitter"))
                {
                    Console.WriteLine("Wrong e-mail or password. If you want to re-enter again press 1, or press anything to quit");
                    string input = Console.ReadLine();
                    if (input != "1")
                        return;
                    Console.WriteLine("\n\n");
                    driver.SetURL("https://twitter.com/login/");
                    pageLoading(driver);
                }
                else
                    break;

            }
            Console.WriteLine("Successfully logged in. Wait just a second\n");
            System.Threading.Thread.Sleep(1000);

            scrapTweets(driver);
            driver.DriverQuit();
        }

        // Check username is available or not
        private static bool usernameNotAvailable(Driver driver)
        {
            if (driver.CheckElementAvailable("NotFoundUsername", -1, "Twitter"))
                return true;
            return false;
        }

        // Wait until page is loading with checking loading circle
        private static void pageLoading(Driver driver)
        {
            while (driver.CheckElementAvailable("PageLoading", 0, "Twitter"))
                System.Threading.Thread.Sleep(1000);
        }

        // Check if given username protectedAccount or not
        private static bool protectedAccount(Driver driver)
        {
            if (driver.CheckElementAvailable("ProtectedAccount", 0, "Twitter"))
            {
                Console.WriteLine("This account is protected. Please log in and follow, then try again");
                return false;
            }
            return true;
        }

        // Sometimes page could not be loaded, give error when that happens
        private static bool notLoaded(Driver driver)
        {
            if (driver.CheckElementAvailable("Reload", 0, "Twitter"))
            {
                Console.WriteLine("Page could not be loaded. Please try again");
                return false;
            }
            return true;
        }

        // When tweets are not be loaded try to click load button
        private static bool tryToReload(Driver driver)
        {
            for (int i = 0; i < 8; i++)
            {
                driver.ClickElement("Reload", "Twitter");
                if (driver.CheckElementAvailable("Reload", 0, "Twitter"))
                    System.Threading.Thread.Sleep(1000);
                else
                    return true;
            }
            return false;
        }

        private static void scrapTweets(Driver driver)
        {
            // First make a selection to specify what you want
            int selection = selectFunction();

            if (selection == 1)
            {
                Console.WriteLine("Please enter a username:\t");
                string username = Console.ReadLine();
                driver.SetURL("https://www.twitter.com/" + username);
            }
            else if (selection == 2)
            {
                Console.WriteLine("Please enter a start date(YYYY-MM-DD format):\t");
                string startDate = Console.ReadLine();
                Console.WriteLine("Please enter an end date(YYYY-MM-DD format):\t");
                string endDate = Console.ReadLine();
                Console.WriteLine("Please enter a username:\t");
                string username = Console.ReadLine();
                driver.SetURL("https://twitter.com/search?q=(from%3A" + username +
                                           ")%20since%3A" + startDate + "%20until%3A" + endDate + "%20-filter%3Areplies&src=typed_query");
            }
            else
                return;
            /*
             * Creating variables we need
             * Random to wait randomly while tweets are loading
             * k to realize end of page
             */
            Console.WriteLine("Write time in seconds for waiting. If you wait more, you will scrap more\nNumber must start with 1 (nearly 100 tweets)");
            int takenNum = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Operation is started please wait now");
            System.Threading.Thread.Sleep(2000);
            Random random = new Random();
            int k = 0;
            List<string> allTweets = new List<string>();

            pageLoading(driver);

            if (usernameNotAvailable(driver))
            {
                Console.WriteLine("Username cannot found check again");
                driver.DriverQuit();
                return;
            }

            if (protectedAccount(driver) && notLoaded(driver))
            {
                while (true)
                {
                    /*
                     * Collect tweet data, when page is scrolled wait for loading bar
                     * When tweets end, loop is going to break, you can decrease this wait time decreasing k value in if condition
                     * 
                     */
                    var tweetList = driver.FindElements("TweetBoxList", "Twitter");
                    var dateList = driver.FindElements("TweetDateList", "Twitter");
                    for (int i = 0; i < tweetList.Count; i++)
                    {
                        try
                        {
                            allTweets.Add(dateList.ElementAt(i).GetAttribute("title") + " --> " + tweetList.ElementAt(i).GetProperty("innerText"));
                        }
                        catch (Exception)
                        {
                            System.Threading.Thread.Sleep(Convert.ToInt32(random.NextDouble() * takenNum * 1000));
                        }
                    }
                    k = driver.TwitterScrollDown(k);
                    if (k == 6)
                        break;
                    if (driver.CheckElementAvailable("Reload", -1, "Twitter"))
                        if (!tryToReload(driver))
                            break;
                }

                // When page is scrolled usually old tweets are going to be added in the list
                // Thanks to distinct we can avoid this
                List<string> distinctTweet = new List<string>();
                distinctTweet = allTweets.Distinct().ToList();
                foreach (var tweet in distinctTweet)
                    LogHolder.AddLogFile(tweet, 2);
            }
        }

        private static int selectFunction()
        {
            while (true)
            {
                Console.WriteLine("\n\nSelect what you want:" +
                                  "\n1->  Write username and scrap latest tweets" +
                                  "\n2->  Enter username, starting date and ending date for specified date timeline tweets\n" +
                                  "Importance: Please use short date ranges" +
                                  "\n0->  Exit");
                string takenInput = Console.ReadLine();
                switch (takenInput)
                {
                    case "1":
                        return 1;
                    case "2":
                        return 2;
                    case "0":
                        return 0;
                    default:
                        Console.WriteLine("Please enter a valid input\n\n");
                        break;
                }
            }
        }
    }
}
