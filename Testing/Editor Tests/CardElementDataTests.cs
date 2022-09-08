
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.IO;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Sharing;
using WarManager.Backend.CardsElementData;

namespace WarManager.Testing.Unit
{
    [Notes.Author("Handles the testing of the card element data parsing")]
    public class CardElementDataTests : MonoBehaviour
    {
        // private static readonly string TestJsonFile = @"D:\War Manager Concept\War Manager Fenix\WarManager\Assets\Custom\Scripts\Testing\Editor Tests\Testing Files\cardElements.json";

        // [Test]
        // public void FactoryParseWithNoErrors()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());
        //     Assert.Greater(data.Count, 0);
        // }

        // [Test]
        // public void CheckTextElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var textElement = data[0];

        //     Assert.AreEqual(true, textElement is CardElementTextData);

        //     if (textElement is CardElementTextData text)
        //     {
        //         Assert.AreEqual(2, text.Version);
        //         Assert.Contains(1, text.Columns);
        //         Assert.Contains(2, text.Columns);
        //         Assert.AreEqual(false, text.Bold);
        //         Assert.AreEqual(false, text.StrikeThrough);
        //         Assert.AreEqual(false, text.Italics);
        //         Assert.AreEqual(false, text.Underline);
        //         Assert.AreEqual(false, text.RichText);

        //         Assert.AreEqual("left", text.TextJustification);
        //         Assert.AreEqual("none", text.OverflowType);
        //         Assert.AreEqual("#111", text.ColorHex);
        //     }
        // }

        // [Test]
        // public void CheckBackgroundElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var backgroundElement = data[5];

        //     Assert.AreEqual(true, backgroundElement is CardBackgroundElementData);

        //     if (backgroundElement is CardBackgroundElementData background)
        //     {
        //         Assert.AreEqual("#ffeced", background.ColorHex);
        //     }
        // }

        // [Test]
        // public void CheckGlanceElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var glanceElement = data[1];

        //     Assert.AreEqual(true, glanceElement is CardGlanceElementData);

        //     if (glanceElement is CardGlanceElementData glance)
        //     {
        //         Assert.AreEqual(2, glance.Version);
        //         Assert.AreEqual(3, glance.IconSize[0]);
        //         Assert.AreEqual(3, glance.IconSize[1]);
        //         Assert.AreEqual(4, glance.MaxIconCount);
        //         Assert.AreEqual(GlanceFlowDirection.left, glance.FlowDirection);
        //         Assert.AreEqual(6, glance.GlanceIcons.Length);

        //         GlanceIconData data1 = new GlanceIconData("CommercialRoofing", "JR WM Icons\\kc.png", "#111", true);
        //         Assert.Contains(data1, glance.GlanceIcons);

        //         GlanceIconData data2 = new GlanceIconData("EM385", "JR WM Icons\\em385.png", "#111", true);
        //         Assert.Contains(data2, glance.GlanceIcons);

        //         GlanceIconData data3 = new GlanceIconData("Crew", "JR WM Icons\\crew.png", "#111", true);
        //         Assert.Contains(data3, glance.GlanceIcons);

        //         GlanceIconData data4 = new GlanceIconData("Super", "JR WM Icons\\super.png", "#111", true);
        //         Assert.Contains(data4, glance.GlanceIcons);

        //         GlanceIconData data5 = new GlanceIconData("Foreman", "JR WM Icons\\foreman.png", "#55aa55", true);
        //         Assert.Contains(data5, glance.GlanceIcons);

        //         GlanceIconData data6 = new GlanceIconData("Travel", "JR WM Icons\\travel.png", "#55aa55", true);
        //         Assert.Contains(data6, glance.GlanceIcons);

        //     }
        // }

        // [Test]
        // public void CheckStickerElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var stickersElement = data[2];

        //     Assert.AreEqual(true, stickersElement is CardStickerElementData);

        //     if (stickersElement is CardStickerElementData sticker)
        //     {
        //         Assert.AreEqual(sticker.StickersData.Length, 4);

        //         var data1 = new StickerData("Crew", "Box 1.png", "#F118");
        //         var data2 = new StickerData("Foreman", "Vertical Bar 1.png", "#1a16");
        //         var data3 = new StickerData("Super", "Vertical Bar 2.png", "#11a8");
        //         var data4 = new StickerData("Project Manager", "Vertical Bar 3.png", "#6668");

        //         Assert.Contains(data1, sticker.StickersData);
        //         Assert.Contains(data2, sticker.StickersData);
        //         Assert.Contains(data3, sticker.StickersData);
        //         Assert.Contains(data4, sticker.StickersData);
        //     }
        // }

        // [Test]
        // public void CheckDialElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var dialElement = data[3];

        //     Assert.AreEqual(true, dialElement is CardDialElementData);

        //     if (dialElement is CardDialElementData dial)
        //     {
        //         Assert.AreEqual("#111", dial.TextColor);
        //         Assert.AreEqual("#eee", dial.DialFallBackColor);
        //         Assert.AreEqual("#111", dial.DialBackgroundColor);

        //         Assert.AreEqual(12, dial.TextFontSize);
        //         Assert.AreEqual(0, dial.SmallestValue);
        //         Assert.AreEqual(100, dial.LargestValue);
        //         Assert.AreEqual(1, dial.DialColors.Length);

        //         DialColorSetting setting = new DialColorSetting("#9f9", 50);
        //         Assert.Contains(setting, dial.DialColors);
        //     }
        // }

        // [Test]
        // public void CheckButtonLinkElement()
        // {
        //     List<CardElementViewData> data = CardElementDataFactory.GetElementData(GetJson());

        //     var linkElement = data[4];

        //     Assert.AreEqual(true, linkElement is CardButtonElementData);

        //     if (linkElement is CardButtonElementData link)
        //     {
        //         Assert.AreEqual("#0000", link.BackgroundColor);
        //         Assert.AreEqual(12, link.FontSize);
        //     }
        // }

        // private string GetJson()
        // {
        //     string json = "";

        //     using (Stream stream = new FileStream(TestJsonFile, FileMode.Open))
        //     {
        //         using (StreamReader reader = new StreamReader(stream))
        //         {
        //             json = reader.ReadToEnd();
        //         }
        //     }

        //     return json;
        // }
    }
}