using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager.Testing.Unit
{

    public class test_cardSheet : MonoBehaviour
    {
        //[Test]
        //[Timeout(1)]
        //public void test_openHomeSheetWithNoLogIn_doesNotOpen()
        //{
        //    SheetsManager.SetHomeCardSheetCurrent();

        //    Assert.IsNull(GetCurentSheetName());

        //    WarSystem.Terminate();
        //}



        #region portable actions

        private string GetCurentSheetName()
        {

            if (SheetsManager.TryGetActiveSheet(SheetsManager.CurrentSheetID, out var sheet))
            {
                return sheet.Name;
            }

            return null;
        }

        private bool IsSheetCurrent(string name)
        {
            var sheet = GetCardSheet(name);

            if (SheetsManager.CurrentSheetID == sheet.ID)
                return true;

            return false;
        }

        private WarManager.Backend.Sheet<Cards.Card> GetCardSheet(string name)
        {
            var sheets = SheetsManager.GetActiveCardSheets();

            foreach (var sheet in sheets)
            {

                UnityEngine.Debug.Log(sheet.Name);

                if (sheet.Name == name)
                {
                    return sheet;
                }

                if (sheet.ID == name)
                {
                    return sheet;
                }
            }

            throw new System.NullReferenceException("The sheet does not exist");
        }

        #endregion
    }
}
