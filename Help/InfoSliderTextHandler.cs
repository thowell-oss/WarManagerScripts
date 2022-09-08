using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Help
{
    [Notes.Author("Converts json data into helpful information")]
    public class InfoSliderTextHandler : MonoBehaviour
    {
        private static string filePath = @"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\Data\War System\Help" + @"\Tips.json";
        private static string tipsFilePath = "https://warmanagerstorage.blob.core.windows.net/wmcontainerstorage/Installer%20Website/Tips.json";

        public List<InfoSliderData> tips = new List<InfoSliderData>();

        private List<int> lastTips = new List<int>();

        public LoadingInfoSlider TipsSlider;

        public void GetTips()
        {
            string text = new System.Net.WebClient().DownloadString(tipsFilePath);

            JsonDocument doc = JsonDocument.Parse(text);

            var root = doc.RootElement;

            foreach (var x in root.EnumerateArray())
            {
                var title = x.GetProperty("title").GetString();
                var description = x.GetProperty("description").GetString();

                tips.Add(new InfoSliderData() { Title = title, Data = description });
            }
        }

        void Start()
        {
            try
            {
                if (tips == null || tips.Count < 1)
                    GetTips();

                StartCoroutine(CycleTips());
            }
            catch (System.Exception ex)
            {

            }
        }

        IEnumerator CycleTips()
        {
            while (true)
            {
                var nextTip = GetNextTip();

                TipsSlider.TitleText.text = nextTip.Title;
                TipsSlider.DescriptionText.text = nextTip.Data;

                TipsSlider.Open();
                yield return new WaitForSeconds(10);
                TipsSlider.Close();
                yield return new WaitForSeconds(.5f);
            }
        }

        private InfoSliderData GetNextTip()
        {

            if (tips.Count < 1)
                throw new System.ArgumentOutOfRangeException("cannot find file");

            if (lastTips.Count == tips.Count)
                lastTips.Clear();

            var rand = 0;

            while (lastTips.Contains(rand))
            {
                rand = UnityEngine.Random.Range(0, tips.Count);
            }

            lastTips.Add(rand);
            return tips[rand];
        }
    }
}
