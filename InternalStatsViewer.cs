// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities.Editor;


// namespace WarManager.Unity3D
// {
//     [Notes.Author("Handles showing specific stats about war manager")]
    public class InternalStatsViewer// : OdinMenuEditorWindow
    {

//         [MenuItem("War Manager/Stats")]
//         private static void OpenWindow()
//         {
//             GetWindow<InternalStatsViewer>().Show();
//         }

//         protected override OdinMenuTree BuildMenuTree()
//         {
//             var tree = new OdinMenuTree();

//             tree.Add("General", new GeneralStats());
//             tree.Add("Tools Manager", new HandleToolsManager());
//             tree.AddAllAssetsAtPath("Scripts", "Assets/Custom/Scripts", false);

//             return tree;
//         }
    }

//     public class GeneralStats
//     {
//         [TabGroup("General")]
//         [SerializeField]
//         public string ServerName;

//         [TabGroup("General")]
//         public string AccountName;

//         [TabGroup("Data Sets")]
//         public string DataSetErrorStatus;

//         [TabGroup("Data Sets")]
//         [ShowIf("DataSetErrorStatus", "None")]
//         public List<string> LoadedDataSets = new List<string>();


//         [TabGroup("Sheets")]
//         public string CurrentSheetID;

//         [TabGroup("Sheets")]
//         public int SheetCount;

//         [TabGroup("Sheets")]
//         public List<string> Sheets = new List<string>();

//         public GeneralStats()
//         {

//         }

//         [Button("Refresh Data")]
//         private void Refresh()
//         {
//             ServerName = WarSystem.ConnectedServerName;

//             if (WarSystem.CurrentActiveAccount != null)
//                 AccountName = WarSystem.CurrentActiveAccount.UserName;

//             LoadedDataSets.Clear();

//             if (WarSystem.DataSetManager != null)
//             {
//                 foreach (var set in WarSystem.DataSetManager.Datasets)
//                 {
//                     LoadedDataSets.Add(set.DatasetName);
//                 }

//                 DataSetErrorStatus = "No Errors";
//             }
//             else
//             {
//                 DataSetErrorStatus = "Null";
//             }

//             CurrentSheetID = WarManager.Backend.SheetsManager.CurrentSheetID;

//             var allSheets = WarManager.Backend.SheetsManager.GetActiveCardSheets();

//             SheetCount = allSheets.Length;

//             Sheets.Clear();

//             foreach (var sheet in allSheets)
//             {
//                 string str = sheet.Name + " (" + sheet.ID + ")";
//                 Sheets.Add(str);
//             }

//             // Debug.Log("Refreshing");
//         }

//         [TabGroup("Sheets")]
//         [Button("Refresh War Manager")]
//         public void ReloadWarManager()
//         {
//             WarManager.Backend.SheetsManager.ReloadCurrentSheet();
//         }
//     }

//     public class HandleToolsManager
//     {
//         public ToolTypes CurrentSelectedTool = ToolsManager.SelectedTool;
//         public ToolTypes PreviousTool = ToolsManager.PreviousTool;
//         public WarMode CurrentMode = ToolsManager.Mode;


//         public HandleToolsManager()
//         {
//             // ToolsManager.OnToolSelected += SelectedTool;
//         }


//         [Button("Refresh")]
//         public void Refresh()
//         {
//             CurrentSelectedTool = ToolsManager.SelectedTool;
//             PreviousTool = ToolsManager.PreviousTool;
//             CurrentMode = ToolsManager.Mode;
//         }

//         public void SelectedTool(ToolTypes type)
//         {
//             Refresh();
//         }
//     }
// }
