
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;

namespace WarManager
{
    /// <summary>
    /// Handles production of an HTML printing system
    /// </summary>
    [Notes.Author("Handles production of an HTML printing system")]
    public class HTMLPrintCards
    {
        private readonly string htmlStarterPath = GeneralSettings.Save_Location_Server + @"\Data\War System\Print\htmlStarter.txt";
        private readonly string htmlEnderPath = GeneralSettings.Save_Location_Server + @"\Data\War System\Print\htmlEnder.txt";
        public string DocTitle { get; private set; } = "New Document";

        public List<string> Links = new List<string>();

        private List<(DataSet set, DataEntry data)> Data = new List<(DataSet set, DataEntry data)>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="docTitle">the title of the page to print</param>
        /// <param name="cards">the cards to print</param>
        public HTMLPrintCards(string docTitle, List<Card> cards)
        {
            DocTitle = docTitle;

            Data = new List<(DataSet set, DataEntry data)>();

            foreach (var card in cards)
            {
                var set = card.DataSet;

                if (set.PrintableTags.Count > 0)
                {
                    DataEntry e = set.GetEntry(card.RowID);

                    string printFormat = set.PrintFormat.Trim();
                    Data.Add((set, e));
                }
            }
        }


        /// <summary>
        /// Print out the cards
        /// </summary>
        /// <returns></returns>
        public string Print()
        {
            List<(string, string)> links = new List<(string, string)>();
            List<string> content = new List<string>();

            bool body = false;

            foreach (var card in Data)
            {
                if (card.set.PrintFormat == "header")
                {
                    if (body)
                    {
                        content.Add(GetBodyEnder());
                        body = false;
                    }

                    string linkId = Guid.NewGuid().ToString().Substring(0, 5);



                    DataEntry e = card.data;
                    links.Add((linkId, e.GetValueAt(2).Value.ToString())); //this might be an issue in the future. The title for the link can only be in column 2

                    // string job = CreateJobCard(j.GetData(2), linkId, j.GetData(1), j.GetData(13), j.GetData(6),
                    //  j.GetData(5) + " " + j.GetData(7) + " " + j.GetData(8), j.GetData(10), "n/a", j.GetData(17), j.GetData(16));

                    var tagValues = GetOrderedTagValuePairs(card.data);
                    content.Add(CreateHeaderCard(tagValues, linkId));
                }

                if (card.set.PrintFormat == "body")
                {
                    if (!body)
                    {
                        content.Add(GetBodyStarter());
                        body = true;
                    }

                    // Debug.Log(j.GetData(2));

                    // string employee = CreateEmployeeCard(j.GetData(2) + " " + j.GetData(4), j.GetData(13), j.GetData(7), j.GetData(9));
                    // content.Add(employee);

                    var values = GetOrderedTagValuePairs(card.data);
                    content.Add(CreateBodyCard(values));
                }
            }

            if (body)
            {
                content.Add(GetBodyEnder());
            }

            string file = GetStarter(htmlStarterPath);
            string header = CreateDashboardHeader(DocTitle, WarSystem.CurrentActiveAccount.UserName, WarSystem.CurrentActiveAccount.UserName, DateTime.Now);
            string buttonLinks = CreateLinks(links);
            string ender = GetEnder(htmlEnderPath);

            string finalContent = "<p>Nothing to show.</p>";

            if (content.Count > 0)
                finalContent = string.Join("\n", content);

            return file + "\n" + header + "\n" + buttonLinks + "\n" + finalContent + "\n" + ender;
        }


        /// <summary>
        /// Get the tag value pairs to print into the dashboard
        /// </summary>
        /// <param name="tag">the list of tags to get the values</param>
        /// <param name="e">the data peice to search through</param>
        /// <returns>returns a list of tag value tuple pairs in order starting from 0 to count - 1</returns>
        private List<(string tag, string value)> GetOrderedTagValuePairs(DataEntry e)
        {
            List<(string tag, string value)> final = new List<(string tag, string value)>();

            foreach (var tagArray in e.DataSet.PrintableTagsHandler.Tags)
            {
                string tag = "";
                string data = "";

                for (int i = 0; i < tagArray.Length; i++)
                {
                    tag += tagArray[i];

                    if (e.TryGetValueWithHeader(tagArray[i], out var eData))
                    {
                        // if (tagArray[i].ToLower().Contains("email"))
                        // {
                        //     data += (string)eData.Value; //make it a email link
                        // }
                        // else if (tagArray[i].ToLower().Contains("phone"))
                        // {
                        //     data += (string)eData.Value;// make it a phone link
                        // }
                        // else
                        // {
                        data = data + " " + eData.Value.ToString();

                        // }
                    }
                }

                final.Add((tag, data));
            }

            return final;
        }

        /// <summary>
        /// Return the starter div for employee cards
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetStarter(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Get the ender div
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetEnder(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Create the header
        /// </summary>
        /// <param name="title"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="dateTimeStamp"></param>
        /// <returns></returns>
        private string CreateDashboardHeader(string title, string userName, string email, DateTime dateTimeStamp)
        {
            TimeZoneInfo currentZone = TimeZoneInfo.Local;
            string time = dateTimeStamp.ToLocalTime().ToShortTimeString() + " " + currentZone.DisplayName.ToString();
            string date = dateTimeStamp.ToLocalTime().ToLongDateString();

            string str =
            $"<div class=\"header\">\n " +
            $"<h1>{title}</h1>\n" +
            $"<h3>User:<a href = \"mailto:{email}\"> {userName} </a></h3>\n" +
            $"<p>Created {date} {time}</p>\n" +
            $"</div>";

            return str;
        }

        private string CreateLinks(List<(string id, string name)> links)
        {
            string start = $"<h3 class=\"links_header\">Links</h3>\n" +
           $"<div class=\"links\">\n";
            string end = $"</div>";

            List<string> buttons = new List<string>();

            foreach (var link in links)
            {
                string button = $"<button class=\"button\"><a href=\"#{link.id}\">{link.name}</a></button>\n";
                buttons.Add(button);
            }

            string final = string.Join("\n", buttons);

            return start + final + end;
        }

        private string CreateJobCard(string jobName, string linkId, string jobCode, string phone, string address,
         string cityAndZip, string projectManager, string complete, string prevalingWage, string gc)
        {
            string str = $"<div class=\"row\">\n" +
                        $"<div class=\"jobTitle\">\n" +
                        $"<h1 id = \"{linkId}\" > {jobName}</h1>\n" +
                        $"<h3>{jobCode}</h3>\n" +
                        $"<p>{phone}</br>\n" +
                        $"{address}</br>\n" +
                        $"{cityAndZip}</p>\n" +
                        $"</div>\n" +
                        $"<div class=\"jobDesc\">\n" +
                        $"<p>Project Manager(s): {projectManager}</p>\n" +
                        $"<p>Complete: {complete}</p>\n" +
                        $"<p>Prevailing Wage: {prevalingWage}</p>\n" +
                        $"<p>GC: {gc}</p>\n" +
                        $"</div>\n" +
                        $"</div>\n";

            return str;
        }

        /// <summary>
        /// Creates the header card
        /// </summary>
        /// <param name="data">the tag and value of the header</param>
        /// <param name="linkId">the link id (for the 'links' at the top of the page)</param>
        /// <returns>returns the string</returns>
        private string CreateHeaderCard(List<(string tag, string value)> data, string linkId)
        {
            string finalString = "";

            int loc = 0;
            int i = 0;

            finalString += $"<div class=\"row\">\n";

            while (loc < data.Count)
            {
                if (loc < 1)
                {
                    finalString += $"<div class=\"jobTitle\">\n";
                }
                else
                {
                    finalString += $"<div class=\"jobDesc\">\n";
                }

                while (i < 4 && loc < data.Count)
                {
                    string t = data[loc].tag;
                    string r = data[loc].value;

                    r = FormatString(t, r);

                    if (data.Count > 1 && loc < 2)
                    {
                        if (loc == 0)
                        {
                            finalString += $"<h1 id = \"{linkId}\" > {r}</h1>\n";
                        }

                        if (loc == 1)
                        {
                            finalString += $"<h3>{r}</h3>\n";
                        }
                    }
                    else
                    {
                        finalString += $"<p>{t}: {r}</p>\n";
                    }

                    i++;
                    loc++;
                }

                finalString += $"</div>\n";
                i = 0;
            }

            finalString += $"</div>\n";

            return finalString;
        }

        /// <summary>
        /// Check if a string is a link and add necessary syntax to make it a link (phone, email, url, etc)
        /// </summary>
        /// <param name="str">the string to check</param>
        /// <returns>Format the string</returns>
        private string FormatString(string tag, string str)
        {
            str = str.Trim();

            // tag = StringUtility.StringUtility_v1.ReplaceProfanity(tag);
            // str = StringUtility.StringUtility_v1.ReplaceProfanity(str);

            var phoneMatch = Regex.Match(str, @"(((\(\d{3}\) ?)|(\d{3}-)|(\d{3}\.))?\d{3}(-|\.)\d{4})");

            if (phoneMatch.Success)
            {
                string phoneLink = $"<a href=\"tel:{str}\">{str}</a>";
                return phoneLink;
            }

            var match = Regex.Match(str, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.ECMAScript | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string mailLink = $"<a href=\"mailto:{str}\">{str}</a>";
                return mailLink;
            }

            match = Regex.Match(str, @"((\w+:\/\/)[-a-zA-Z0-9:@;?&=\/%\+\.\*!'\(\),\$_\{\}\^~\[\]`#|]+)");
            if (match.Success)
            {
                string uriLink = $"<a href=\"{str}\">{str}</a>";
                return uriLink;
            }

            match = Regex.Match(str, @"((?!000)(?!666)(?:[0-6]\d{2}|7[0-2][0-9]|73[0-3]|7[5-6][0-9]|77[0-2]))-((?!00)\d{2})-((?!0000)\d{4})$", RegexOptions.Singleline | RegexOptions.ExplicitCapture);

            if (match.Success)
            {
                string ssidProtect = $"(Redacted)";
                return ssidProtect;
            }

            if (str.Contains("\n"))
            {
                str = str.Replace("\n", "</br>");
            }

            return str;
        }

        private string GetBodyStarter()
        {
            return "<div class=\"row\">\n";
        }

        private string GetBodyEnder()
        {
            return "</div>\n";
        }

        private string CreateEmployeeCard(string name, string position, string phone, string email)
        {
            string str = "";

            if (position.ToLower() == "foreman")
            {
                str += "<div class=\"miniColumn\" style=\"background-color: lightgreen; \">\n";
            }
            else
            {
                str += "<div class=\"miniColumn\">\n";
            }

            str += $"<h3>{name}</h3>\n" +
               $"<h4>{position}</h4>\n" +
               $"<p><a href=\"tel:{phone}\">{phone}</a></p>\n" +
               $"<p><a href=\"mailto:{email}\">{email}</a></p>\n" +
               $"</div>\n";

            return str;
        }

        private string CreateBodyCard(List<(string tag, string value)> tagValuePairs)
        {
            bool isForeman = false;

            for (int i = 0; i < tagValuePairs.Count; i++)
            {
                if (tagValuePairs[i].value.ToLower().Trim() == "foreman")
                {
                    isForeman = true;
                }
            }

            string final = "";

            if (isForeman)
            {
                final = "<div class=\"miniColumn\" style=\"background-color: lightgreen; \">\n";
            }
            else
            {
                final = "<div class=\"miniColumn\">\n";
            }


            for (int i = 0; i < tagValuePairs.Count; i++)
            {
                string tag = tagValuePairs[i].tag;
                string value = tagValuePairs[i].value;

                value = FormatString(tag, value);

                if (i == 0)
                {
                    final += $"<h3>{value}</h3>\n";
                }
                else if (i == 1)
                {
                    final += $"<h4>{value}</h4>\n";
                }
                else
                {
                    final += $"<p>{value}</p>\n";
                }
            }

            final += $"</div>\n";

            return final;
        }
    }
}
