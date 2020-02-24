using Newtonsoft.Json.Linq;
using System.IO;

namespace TweetScraper.JsonFiles
{
    static class ParseJson
    {
        static public string FindXpath(string siteName, string XpathLocation)
        {
            JObject obj = JObject.Parse(File.ReadAllText(@"Write your XPath.json file's path"));
            JToken value = obj[siteName];
            return (string)value[XpathLocation];
        }

    }
}
