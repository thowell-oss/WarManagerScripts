/* GeneralSettings.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;


namespace WarManager
{
    /// <summary>
    /// Handles settings to be referenced throughout the software
    /// </summary>
    [Notes.Author("Handles persistant settings for War Manager (cross account)")]
    public static class GeneralSettings
    {
        public delegate void ChangeLanguage(Language l);
        /// <summary>
        /// event is called when the langauge is changed
        /// </summary>
        public static event ChangeLanguage OnChangeLangauge;

        public static readonly string WebSiteSupportURL = "https://warmanagerstorage.blob.core.windows.net/wmcontainerstorage/Installer%20Website/support.html";
        public static readonly string BugReportURL = "https://forms.gle/JhQUwfo27uC2jQZm6";
        public static readonly string FeedbackURL = "";

        public delegate void resetAutoSave_delegate();
        public static event resetAutoSave_delegate OnResetAutoSave;

        /// <summary>
        /// After a shift is complete, the first location from where the card was located now gets filled by a set of cards shifting from the bottom
        /// </summary>
        public static bool FillGapAfterShiftFromBottom { get; set; } = true;

        /// <summary>
        /// After a shift is complete, the first location from where the card was located now gets filled by a set of cards shifting from the right side
        /// </summary>
        public static bool FillGapAfterShiftFromSide { get; set; } = false;


        /// <summary>
        /// The default scale of the grid incase one cannot be found from the sheet meta data
        /// </summary>
        /// <value></value>
        public static double[] DefaultGridScale = new double[2] { 6, 3 };

        private static bool _allowSideShifting = true;

        /// <summary>
        /// Allow cards to shift from side to side
        /// </summary>
        public static bool AllowSideShifting
        {
            get
            {
                return _allowSideShifting;
            }
            set
            {
                _allowSideShifting = value;
                //ContextMenuHandler.Refresh();
            }
        }

        private static bool _allowUpDownShifting = true;

        /// <summary>
        /// Allow cards to shift up and down
        /// </summary>
        public static bool AllowUpDownShifting
        {
            get
            {
                return _allowUpDownShifting;
            }

            set
            {
                _allowUpDownShifting = value;
                //ContextMenuHandler.Refresh();
            }
        }

        /// <summary>
        /// Can lock delegate for event - used to force cards out of locked state if locking is not allowed
        /// </summary>
        /// <param name="canLock">dictates if the card can be locked or not</param>
        public delegate void CanLockCard(bool canLock);
        public static event CanLockCard OnCanLockCard;

        private static bool _allowCardLocking = true;

        /// <summary>
        /// Can the cards be locked?
        /// </summary>
        public static bool AllowCardLocking
        {
            get
            {
                return _allowCardLocking;
            }
            set
            {
                _allowCardLocking = value;
                if (OnCanLockCard != null)
                {
                    OnCanLockCard(value);
                }
            }
        }

        /// <summary>
        /// can hide delegate for event - used to force cards out of hiding if hiding is not allowed
        /// </summary>
        /// <param name="hide">can the card be hidden?</param>
        public delegate void allowCardHiding(bool hide);
        public static event allowCardHiding OnAllowCardHiding;

        private static bool _allowCardHiding = true;
        /// <summary>
        /// Can the cards be hidden?
        /// </summary>
        public static bool AllowCardHiding
        {
            get
            {
                return _allowCardHiding;
            }
            set
            {
                _allowCardHiding = value;
                if (OnAllowCardHiding != null)
                {
                    OnAllowCardHiding(value);
                }
            }
        }

        /// <summary>
        /// Can the cards be removed?
        /// </summary>
        public static bool AllowCardRemoving { get; set; } = true;

        /// <summary>
        /// Can the user view details of the card?
        /// </summary>
        public static bool CanViewDetails { get; set; } = true;


        private static Language _currentLanguage;
        /// <summary>
        /// The current langugage being used in the War Manager
        /// </summary>
        public static Language Account_Current_Language
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                _currentLanguage = value;
                if (OnChangeLangauge != null)
                    OnChangeLangauge(value);
            }
        }

        /// <summary>
        /// The current department  (v1)
        /// </summary>
        public static Department Account_Department { get; set; } = Department.CommercialRoofing;

        #region colors
        public static (float r, float g, float b) Select_Card_Color { get; set; } = (255, 255, 0);

        #endregion

        #region touch settings
        /// <summary>
        /// If set to true, some menus/buttons will float to the bottom of the screen for ease of use
        /// </summary>
        public static bool PreferMenusOnBotton { get; set; } = false;
        #endregion


        #region Slide Windows
        /// <summary>
        /// The window will force open when it gets updates reguardless of its current open or close state
        /// </summary>
        public static bool ForceWindowPaneOpen { get; set; } = false;

        /// <summary>
        /// The window pane will close when the mouse or touch tap drags a card
        /// </summary>
        public static bool AutoCloseWhenPointerDragCard { get; set; } = true;
        #endregion

        #region file systems

        /// <summary>
        /// Dictates if the sheet editor should auto save
        /// </summary>
        public static bool Save_AutoSave { get; set; } = true;

        /// <summary>
        /// How many seconds does it take between auto saves?
        /// </summary>
        /// <value></value>
        public static int Save_AutoSave_Time_Seconds { get; private set; } = 300;


        /// <summary>
        /// Sends an event to reset the auto save timer when the seconds are set to a different interval
        /// </summary>
        /// <param name="seconds"></param>
        public static int SetAutoSaveTime(int seconds)
        {
            if (seconds > 2 && seconds < 1000)
            {
                Save_AutoSave_Time_Seconds = seconds;

                if (OnResetAutoSave != null)
                {
                    OnResetAutoSave();
                }
            }

            return GeneralSettings.Save_AutoSave_Time_Seconds;
        }

        /// <summary>
        /// Dictates the location where to save the file (Root:)
        /// </summary>
        public static string Save_Location_Server { get; set; } = "";

        /// <summary>
        /// Root:\Data\Data Sets
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_Datasets
        {
            get
            {
                return Save_Location_Server + @"\Data\Data Sets";
            }
        }

        /// <summary>
        /// Root:\Data\Data Sets\Action
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_ActionDatasets
        {
            get
            {
                return Save_Location_Server + @"\Data\Data Sets\Action";
            }
        }


        /// <summary>
        /// Root:\CSV Data\_Action
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_ActionData
        {
            get
            {
                return Save_Location_Server + @"\CSV Data\_Action";
            }
        }

        /// <summary>
        /// Root:\Data\War System\Help (Folder)
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_Help
        {
            get
            {
                return Save_Location_Server + @"\Data\War System\Help";
            }
        }

        /// <summary>
        /// Root:\Data\User Permissions
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_Permissions
        {
            get
            {
                return Save_Location_Server + @"\Data\User Permissions";
            }
        }

        /// <summary>
        /// Root:\Sheets
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_Sheets
        {
            get
            {
                return Save_Location_Server + @"\Sheets";
            }
        }

        /// <summary>
        /// Get the server info: Root:\Data\War System\server_info.txt
        /// </summary>
        /// <value></value>
        public static string Save_Location_Server_ServerInfo
        {
            get
            {
                return Save_Location_Server + GetServerInfoPathOnly;
            }
        }

        /// <summary>
        /// Get the server info path only without the root path
        /// </summary>
        /// <value></value>
        public static string GetServerInfoPathOnly
        {
            get
            {
                return @"\Data\War System\server_info.txt";
            }
        }

        /// <summary>
        /// Get access to the server Icons
        /// </summary>
        /// <value></value>
        public static string Server_Icons
        {
            get
            {
                return Save_Location_Server + @"\Data\Data Set Usable Icons";
            }
        }


        /// <summary>
        /// Get the War System Directory
        /// </summary>
        /// <value></value>
        public static string GetWarSystemPath
        {
            get
            {
                return Save_Location_Server + @"\Data\War System";
            }
        }

        public static string HomeSheetName
        {
            get
            {
                return "Home";
            }
        }

        /// <summary>
        /// Dictates the location where the file is saved offline
        /// </summary>
        public static string Save_Location_Offline { get; set; } = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\War Manager\Offline";

        /// <summary>
        /// The version of the files
        /// </summary>
        public static readonly double Save_Sheet_Version = 1.1;

        /// <summary>
        /// Should War Manager Use the profanity filter?
        /// </summary>
        public static bool UseProfanityFilter = false;

        /// <summary>
        /// API key used to ping rest server in order to see if an email is valid or not
        /// </summary>
        /// <value></value>
        public static string API_Key_EmailVerify { get; set; } = "92928b756e623357b3bd80e8dc90deae8dcfe4517eab01695f6c6d08956b870c64b87b422dd7105547b5fa96e1830492";

        #endregion

        /// <summary>
        /// Controls if the on screen mouse and keyboard input system should be enabled or not
        /// </summary>
        /// <value></value>
        public static bool UseOnScreenInputGui { get; set; } = false;

        /// <summary>
        /// Handles the amount of power behind the zoom amount;
        /// </summary>
        /// <value></value>
        public static float zoomPower { get; set; } = 7;

        /// <summary>
        /// Enable or Disable the actor cards
        /// </summary>
        /// <value></value>
        public static bool EnableActors { get; set; } = true;

    }

    /// <summary>
    /// department for v2 TODO: be able to change this
    /// </summary>
    public enum Department
    {
        CommercialRoofing,
        SheetMetal,
        Florida,
        Solar,
        Colorado,
        TenantFinish,
        Storm,
        Repair,
    }

    /// <summary>
    /// The system language
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// All text will be in English
        /// </summary>
        English,

        /// <summary>
        /// All text will be in Spanish
        /// </summary>
        Spanish,

        /// <summary>
        /// All text will be in French
        /// </summary>
        French,
    }
}
