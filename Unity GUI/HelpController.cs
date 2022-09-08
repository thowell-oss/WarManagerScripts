

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.RegularExpressions;



using UnityEngine;

using WarManager.Unity3D.Windows;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class HelpController : MonoBehaviour
    {
        public string newBugURL = "";
        public string newFeatureURL = "";

        public Sprite BugSprite;
        public Sprite newFeatureSprite;

        public float refreshTimeSeconds = 3600;

        public List<(string title, string content, int height, string[] butnTitles, string[] butnLinks)> HelpPageList = new List<(string title, string content, int height, string[] butnTitles, string[] butnLinks)>();

        public Sprite BackSprite, RefreshSprite, ContentPageSprite, ButtonLinkSprite;

        public bool isOpen { get; private set; } = false;

        // void Awake() => StartCoroutine(UpdateHelpWindow());

        // IEnumerator UpdateHelpWindow()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForSeconds(refreshTimeSeconds * .5f);
        //         // Debug.Log("updating help window");
        //         if (!isOpen)
        //             UpdateListOfHelp();
        //         yield return new WaitForSeconds(refreshTimeSeconds * 59.5f);
        //     }
        // }

        /// <summary>
        /// Update the help list
        /// </summary>
        void UpdateListOfHelp()
        {

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo("Help", null));
            content.Add(new SlideWindow_Element_ContentInfo("Refresh", -1, ButtonHandler, RefreshSprite));
            content.Add(new SlideWindow_Element_ContentInfo("Report Bug", -2, (x) =>
            {
                Application.OpenURL(newBugURL);
            }, BugSprite));

            content.Add(new SlideWindow_Element_ContentInfo("Request Feature", -2, (x) =>
            {
                Application.OpenURL(newFeatureURL);
            }, newFeatureSprite));

            content.Add(new SlideWindow_Element_ContentInfo(25));

            if (Directory.Exists(GeneralSettings.Save_Location_Server_Help))
            {
                var files = Directory.GetFiles(GeneralSettings.Save_Location_Server_Help);

                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        string str = File.ReadAllText(file);
                        jsonParse(content, str);
                    }
                }

                if (content.Count < 4)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("", "Nothing to show."));
                }
            }
            
            SlideWindowsManager.main.AddHelpContent(content, false);
            isOpen = false;
        }

        /// <summary>
        /// Manually update the help window
        /// </summary>
        /// <param name="x"></param>
        void ButtonHandler(int x)
        {
            if (x == -1 || x == -2)
            {
                UpdateListOfHelp();
            }

            if (x < HelpPageList.Count && x >= 0)
            {
                ShowPage(x);
                isOpen = true;
            }
        }

        public void jsonParse(List<SlideWindow_Element_ContentInfo> contentList, string json)
        {
            using (var document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;

                JsonElement Jtitle = root.GetProperty("title");
                string title = Jtitle.GetString();

                string content = "";

                if (root.TryGetProperty("content", out var Jcontent))
                {
                    content = Jcontent.GetString();
                }

                List<(string title, string link)> linkButtons = new List<(string title, string link)>();

                if (root.TryGetProperty("buttons", out var butns))
                {
                    foreach (var button in butns.EnumerateArray())
                    {
                        string buttonTitle = button.GetProperty("title").GetString();
                        string link = button.GetProperty("link").GetString();

                        var match = Regex.Match(link, @"((\w+:\/\/)[-a-zA-Z0-9:@;?&=\/%\+\.\*!'\(\),\$_\{\}\^~\[\]`#|]+)");

                        if (match.Success)
                        {
                            linkButtons.Add((buttonTitle, link));
                        }
                    }
                }

                JsonElement Jlength = root.GetProperty("height");
                int height = Jlength.GetInt32();

                List<string> titles = new List<string>();
                List<string> links = new List<string>();

                foreach (var buttonInfo in linkButtons)
                {
                    titles.Add(buttonInfo.title);
                    links.Add(buttonInfo.link);
                }


                HelpPageList.Add((title, content, height, titles.ToArray(), links.ToArray()));
                contentList.Add(new SlideWindow_Element_ContentInfo(title, HelpPageList.Count - 1, ShowPage, ContentPageSprite));
            }
        }


        /// <summary>
        /// Show Help Page
        /// </summary>
        /// <param name="x"></param>
        public void ShowPage(int x)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            (string title, string content, int height, string[] butnTitles, string[] butnLinks) info = HelpPageList[x];

            content.Add(new SlideWindow_Element_ContentInfo(" ", -2, ButtonHandler, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(info.title, info.content, info.height));
            content.Add(new SlideWindow_Element_ContentInfo(50));


            if (info.height >= 5000)
            {
                content.Add(new SlideWindow_Element_ContentInfo("", -2, ButtonHandler, BackSprite));
            }

            for (int i = 0; i < info.butnTitles.Length; i++)
            {
                if (info.butnLinks.Length > i)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(info.butnTitles[i], i, (x) =>
                    {
                        Application.OpenURL(info.butnLinks[x]);
                    }, ButtonLinkSprite));
                }
            }

            SlideWindowsManager.main.AddHelpContent(content);
        }
    }
}
