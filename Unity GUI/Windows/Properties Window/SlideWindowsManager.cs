/* SlideWindowsManager.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using WarManager;
using WarManager.Cards;
using WarManager.Unity3D.Windows;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles the general behavior of all the windows
    /// </summary>
    public class SlideWindowsManager : MonoBehaviour
    {
        #region static
        public static SlideWindowsManager main;
        public void Awake()
        {
            setStatic();
        }

        private void setStatic()
        {
            main = this;
        }

        [SerializeField] FormsHider _formsHider;

        #endregion

        public SlideWindowController Properties;
        public SlideWindowController Reference;
        public SlideWindowController Layers;
        public SlideWindowController Help;
        public SlideWindowController TasksAndNotifications;

        /// <summary>
        /// Add content to the tasks and messages window
        /// </summary>
        /// <param name="content">the content to add the window</param>
        /// <param name="forceOpen">should the window be forced open (even when the settings dictate not to).</param>
        /// <param name="filter">automatic filtering</param>
        /// <param name="thinkingTime">how much psudo-loading time it will take to load in the objects (helpful for long lists)</param>
        public void AddTasksAndMessagesContent(List<SlideWindow_Element_ContentInfo> content, bool forceOpen = false, string filter = "-", float thinkingTime = -1)
        {
            if (_formsHider.FormsActive)
            {
                return;
            }

            WindowContentQueue contentQueue = new WindowContentQueue();
            TasksAndNotifications.ClearContent();

            for (int i = 0; i < content.Count; i++)
            {
                contentQueue.EnqeueContent(content[i]);
            }

            TasksAndNotifications.AddContent(contentQueue);

            if (filter != "-" && filter != null)
            {
                TasksAndNotifications.SetFilter(filter);
                // TasksAndNotifications.RunFilter();
            }

            if (forceOpen && TasksAndNotifications.Closed)
            {
                TasksAndNotifications.ToggleTab();
                Properties.Close();
                Help.Close();
                Reference.Close();
            }

            if (thinkingTime > 0)
            {
                TasksAndNotifications.SetLoadingBackgroundActive_Timer(thinkingTime);
            }
        }

        /// <summary>
        /// Add content to the reference window
        /// </summary>
        /// <param name="content"> the list of content</param>
        /// <param name="forceOpen">should the window open after being setup?</param>
        /// <param name="filter">a string of information to filter out any unwanted info</param>
        /// <param name="thinkingTime">if the float is greater than 0, a loading symbol will appear for the amout of the float</param>
        public void AddReferenceContent(List<SlideWindow_Element_ContentInfo> content, bool forceOpen = false, string filter = "-", float thinkingTime = -1)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            WindowContentQueue contentQueue = new WindowContentQueue();
            Reference.ClearContent();

            for (int i = 0; i < content.Count; i++)
            {
                contentQueue.EnqeueContent(content[i]);
            }

            Reference.AddContent(contentQueue);

            if (filter != "-" && filter != null)
            {
                Reference.SetFilter(filter);
                // Reference.RunFilter();
            }

            if (forceOpen && Reference.Closed)
            {
                Reference.ToggleTab();
                Properties.Close();
                TasksAndNotifications.Close();
                Help.Close();
            }

            if (thinkingTime > 0)
            {
                Reference.SetLoadingBackgroundActive_Timer(thinkingTime);
            }
        }

        /// <summary>
        /// Add content to the layers window
        /// </summary>
        /// <param name="contentInfos">the list of layers content</param>
        /// <param name="forceOpen">should the window open after being setup?</param>
        /// <param name="filter">a string of information to filter out any unwanted info</param>
        /// <param name="thinkingTime">if the float is greater than 0, a loading symbol will appear for the amout of the float</param>
        public void AddLayersContent(List<SlideWindow_Element_ContentInfo> contentInfos, bool forceOpen = false, string filter = "-", float thinkingTime = -1)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            WindowContentQueue contentQueue = new WindowContentQueue();
            Layers.ClearContent();

            for (int i = 0; i < contentInfos.Count; i++)
            {
                contentQueue.EnqeueContent(contentInfos[i]);
            }

            Layers.AddContent(contentQueue);

            if (filter != "-" && filter != null)
            {
                Layers.SetFilter(filter);
                // Layers.RunFilter();
            }

            if (forceOpen && Layers.Closed)
            {
                Layers.ToggleTab();
            }

            if (thinkingTime > 0)
            {
                Layers.SetLoadingBackgroundActive_Timer(thinkingTime);
            }
        }

        /// <summary>
        /// Add content to the properties window
        /// </summary>
        /// <param name="contentInfos"> the list of properties content</param>
        /// <param name="forceOpen">should the window open after being setup?</param>
        /// <param name="filter">a string of information to filter out any unwanted info</param>
        /// <param name="thinkingTime">if the float is greater than 0, a loading symbol will appear for the amout of the float</param>
        public void AddPropertiesContent(List<SlideWindow_Element_ContentInfo> contentInfos, bool forceOpen = false, string filter = "-", float thinkingTime = -1)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            var contentQueue = new WindowContentQueue();
            Properties.ClearContent();

            for (int i = 0; i < contentInfos.Count; i++)
            {
                contentQueue.EnqeueContent(contentInfos[i]);
            }

            Properties.AddContent(contentQueue);

            if (filter != "-" && filter != null)
            {
                Properties.SetFilter(filter);
                // Properties.RunFilter();
            }

            if (forceOpen && Properties.Closed)
            {
                Properties.ToggleTab();
                Reference.Close();
                Help.Close();
                TasksAndNotifications.Close();
            }

            if (thinkingTime > 0)
            {
                Properties.SetLoadingBackgroundActive_Timer(thinkingTime);
            }
        }

        public void UpdateTasksAndMessagesElements()
        {
            TasksAndNotifications.UpdateElements();
        }

        public void UpdatePropertiesElements()
        {
            Properties.UpdateElements();
        }

        public void UpdateReferenceElements()
        {
            Reference.UpdateElements();
        }

        /// <summary>
        /// Add content to the help window
        /// </summary>
        /// <param name="content">the content to add to the help window</param>
        /// <param name="forceOpen">should the window open after being setup?</param>
        /// <param name="filter">a string of information to filter out any unwanted info</param>
        /// <param name="thinkingTime">if the float is greater than 0, a loading symbol will appear for the amout of the float</param>
        public void AddHelpContent(List<SlideWindow_Element_ContentInfo> content, bool forceOpen = false, string filter = "-", float thinkingTime = -1)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            var contentQueue = new WindowContentQueue();
            Help.ClearContent();

            for (int i = 0; i < content.Count; i++)
            {
                contentQueue.EnqeueContent(content[i]);
            }

            Help.AddContent(contentQueue);

            if (filter != "-" && filter != null)
            {
                Help.SetFilter(filter);
                // Help.RunFilter();
            }

            if (forceOpen && Help.Closed)
            {
                Help.ToggleTab();

                Reference.Close();
                Help.Close();
                TasksAndNotifications.Close();
                Properties.Close();
            }

            if (thinkingTime > 0)
            {
                Help.SetLoadingBackgroundActive_Timer(thinkingTime);
            }
        }

        public void ClearTasksAndMessagesContent()
        {
            TasksAndNotifications.ClearContent();
        }

        /// <summary>
        /// Clear content from the properties window
        /// </summary>
        public void ClearProperties()
        {
            Properties.ClearContent();
        }

        /// <summary>
        /// Clear content form the reference window
        /// </summary>
        public void ClearReference()
        {
            Reference.ClearContent();
        }

        /// <summary>
        /// Clear content from the layers window 
        /// </summary>
        public void ClearLayers()
        {
            Layers.ClearContent();
        }


        /// <summary>
        /// Clear content from the help window
        /// </summary>
        public void ClearHelp()
        {
            Help.ClearContent();
        }


        /// <summary>
        /// Set the card properties according to the card
        /// </summary>
        /// <param name="display">the card to show</param>
        public void SetCardsProperties(UnityCardDisplay[] cards)
        {
            if (_formsHider.FormsActive)
            {
                return;
            }

            if (cards == null || cards.Length == 0)
            {
                Properties.ClearContent();
                return;
            }

            WindowContentQueue content = new WindowContentQueue();

            if (cards.Length == 1)
            {

                UnityCardDisplay display = cards[0];

                if (display == null)
                {
                    Properties.ClearContent();
                    return;
                }

                if (display != null && display.ID != null && display.Card != null)
                {
                    SlideWindow_Element_ContentInfo header = new SlideWindow_Element_ContentInfo("Card Debug Info", null);
                    SlideWindow_Element_ContentInfo id = new SlideWindow_Element_ContentInfo("Card ID:", display.ID.Substring(0, 5));
                    SlideWindow_Element_ContentInfo spacer = new SlideWindow_Element_ContentInfo(25);
                    SlideWindow_Element_ContentInfo location = new SlideWindow_Element_ContentInfo("Grid Location: ", display.Card.point.ToString());
                    SlideWindow_Element_ContentInfo offset = new SlideWindow_Element_ContentInfo("Offset: ", display.Card.Layout.Offset.ToString());
                    SlideWindow_Element_ContentInfo globalLocation = new SlideWindow_Element_ContentInfo("Global Location: ", display.transform.position.ToString());
                    SlideWindow_Element_ContentInfo isActive = new SlideWindow_Element_ContentInfo("Is Active?", display.ActiveCard.ToString());
                    SlideWindow_Element_ContentInfo isHidden = new SlideWindow_Element_ContentInfo("Is Hidden?", display.Card.CardHidden.ToString());
                    SlideWindow_Element_ContentInfo spacer2 = new SlideWindow_Element_ContentInfo(40);

                    SlideWindow_Element_ContentInfo SheetInfo = new SlideWindow_Element_ContentInfo("Sheet Debug Info", null);
                    SlideWindow_Element_ContentInfo sheetMult = new SlideWindow_Element_ContentInfo("Spacing", WarManagerDriver.Main.Scale.ToString());
                    SlideWindow_Element_ContentInfo currentTool = new SlideWindow_Element_ContentInfo("Current tool", ToolsManager.SelectedTool.ToString());
                    SlideWindow_Element_ContentInfo TotalSelected = new SlideWindow_Element_ContentInfo("Total Cards Selected", WarManagerDriver.Main.SelectHandler.SelectedCardAmt.ToString());

                    content.EnqeueContent(header);
                    content.EnqeueContent(id);
                    content.EnqeueContent(spacer);
                    content.EnqeueContent(location);
                    content.EnqeueContent(globalLocation);
                    content.EnqeueContent(offset);
                    content.EnqeueContent(isActive);
                    content.EnqeueContent(isHidden);
                    //content.EnqeueContent(state);
                    content.EnqeueContent(spacer2);
                    content.EnqeueContent(SheetInfo);
                    content.EnqeueContent(sheetMult);
                    content.EnqeueContent(currentTool);
                    content.EnqeueContent(TotalSelected);

                    Properties.ClearContent();
                    Properties.AddContent(content);

                }


            }
            else if (cards.Length > 1)
            {
                SlideWindow_Element_ContentInfo header = new SlideWindow_Element_ContentInfo("Card Debug Info", null);
                SlideWindow_Element_ContentInfo id = new SlideWindow_Element_ContentInfo("Card ID:", "Multiple Cards...");
                SlideWindow_Element_ContentInfo spacer = new SlideWindow_Element_ContentInfo(25);
                SlideWindow_Element_ContentInfo location = new SlideWindow_Element_ContentInfo("Grid Location: ", "-");
                SlideWindow_Element_ContentInfo offset = new SlideWindow_Element_ContentInfo("Offset: ", "-");
                SlideWindow_Element_ContentInfo globalLocation = new SlideWindow_Element_ContentInfo("Global Location: ", "-");
                SlideWindow_Element_ContentInfo isActive = new SlideWindow_Element_ContentInfo("Is Active?", "-");
                SlideWindow_Element_ContentInfo isHidden = new SlideWindow_Element_ContentInfo("Is Hidden?", "-");
                SlideWindow_Element_ContentInfo state = new SlideWindow_Element_ContentInfo("Card State: ", "-");
                SlideWindow_Element_ContentInfo spacer2 = new SlideWindow_Element_ContentInfo(40);

                SlideWindow_Element_ContentInfo SheetInfo = new SlideWindow_Element_ContentInfo("Sheet Debug Info", null);
                SlideWindow_Element_ContentInfo sheetMult = new SlideWindow_Element_ContentInfo("Spacing", WarManagerDriver.Main.Scale.ToString());
                SlideWindow_Element_ContentInfo currentTool = new SlideWindow_Element_ContentInfo("Current tool", ToolsManager.SelectedTool.ToString());
                SlideWindow_Element_ContentInfo TotalSelected = new SlideWindow_Element_ContentInfo("Total Cards Selected", WarManagerDriver.Main.SelectHandler.SelectedCardAmt.ToString());

                content.EnqeueContent(header);
                content.EnqeueContent(id);
                content.EnqeueContent(spacer);
                content.EnqeueContent(location);
                content.EnqeueContent(globalLocation);
                content.EnqeueContent(offset);
                content.EnqeueContent(isActive);
                content.EnqeueContent(isHidden);
                content.EnqeueContent(state);
                content.EnqeueContent(spacer2);
                content.EnqeueContent(SheetInfo);
                content.EnqeueContent(sheetMult);
                content.EnqeueContent(currentTool);
                content.EnqeueContent(TotalSelected);

                Properties.ClearContent();
                Properties.AddContent(content);
            }

        }

        /// <summary>
        /// Close properties and reference windows
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void CloseWindows(bool overrideGeneralSettings = false)
        {
            if (overrideGeneralSettings)
            {
                Properties.Closed = true;
                Reference.Closed = true;
                Help.Closed = true;
                TasksAndNotifications.Closed = true;
            }
            else if (GeneralSettings.AutoCloseWhenPointerDragCard)
            {
                Properties.Closed = true;
                Reference.Closed = true;
                Help.Closed = true;
                TasksAndNotifications.Closed = true;
            }
        }

        /// <summary>
        /// Close reference window
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void CloseReference(bool overrideGeneralSettings)
        {
            if (overrideGeneralSettings)
            {
                Reference.Closed = true;
            }
            else if (GeneralSettings.AutoCloseWhenPointerDragCard)
            {
                Reference.Closed = true;
            }
        }

        /// <summary>
        /// clsoe properties window
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void CloseProperties(bool overrideGeneralSettings)
        {
            if (overrideGeneralSettings)
            {
                Properties.Closed = true;

            }
            else if (GeneralSettings.AutoCloseWhenPointerDragCard)
            {
                Properties.Closed = true;

            }
        }

        public void CloseTasksAndManagement(bool overrideGeneralSettings)
        {
            if (overrideGeneralSettings)
            {
                TasksAndNotifications.Closed = true;

            }
            else if (GeneralSettings.AutoCloseWhenPointerDragCard)
            {
                TasksAndNotifications.Closed = true;

            }
        }

        public void OpenTasksAndManagement(bool overrideGeneralSettings)
        {
            if (_formsHider.FormsActive)
            {
                return;
            }

            if (TasksAndNotifications.Closed)
            {
                TasksAndNotifications.ToggleTab();
            }

            Properties.Closed = true;
            Reference.Closed = true;
            Help.Closed = true;
            Layers.Closed = true;
        }

        /// <summary>
        /// Open the Properties Window
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void OpenProperties(bool overrideGeneralSettings)
        {
            if (_formsHider.FormsActive)
            {
                return;
            }

            if (Properties.Closed)
            {
                Properties.ToggleTab();
            }

            Reference.Closed = true;
            Help.Closed = true;
            Layers.Closed = true;
            TasksAndNotifications.Closed = true;

        }


        /// <summary>
        /// Open the Reference Window
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void OpenReference(bool overrideGeneralSettings)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            if (Reference.Closed)
                Reference.ToggleTab();

            Properties.Closed = true;
            Help.Closed = true;
            Layers.Closed = true;
            TasksAndNotifications.Closed = true;
        }


        /// <summary>
        /// Open the Help Window
        /// </summary>
        /// <param name="overrideGeneralSettings"></param>
        public void OpenHelp(bool overrideGeneralSettings)
        {

            if (_formsHider.FormsActive)
            {
                return;
            }

            if (Help.Closed)
                Help.ToggleTab();

            Properties.Closed = true;
            Reference.Closed = true;
            Layers.Closed = true;
            TasksAndNotifications.Closed = true;
        }
    }
}
