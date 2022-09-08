
using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Threading;

using System.IO;

using System.Text;

using UnityEngine;

using WarManager;
using WarManager.Cards;
using WarManager.Sharing.Security;
using WarManager.Sharing;
using WarManager.Backend;

namespace WarManager.Testing
{
    public class sheet_partition_tester : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

            var userPassword = "60e7a2da-a450-4a1e-a2d8-a296a7e894d1";

            Account.LogIn("Default User", userPassword, out var account);

            Debug.LogError("USER NAME CHANGED!");

            string sheetID = "";

            string password = account.HashKey;
            Debug.Log(password);
            string MD5Hash = Encryption.GetMD5Hash(password);
            Debug.Log(MD5Hash);

            sheetID = SheetsManager.NewCardSheet("My New Card Sheet", new string[1] {"test"}, GeneralSettings.DefaultGridScale, new string[0]);

            Debug.Log("Sheet " + sheetID + " was opened");

            Sheet<Card> sheet = SheetsManager.GetActiveSheet(sheetID);


            // SheetsManager.OnCloseCardSheet += SheetCloseMessenger;
            // try
            // {
            sheet.AddNewLayer(new Layer("My first layer", 1, ""));
            sheet.AddNewLayer(new Layer("My second layer", 2, ""));

            AddCards(sheet);


            // string text = "this is some text to encrypt";

            // byte[] password = Guid.NewGuid().ToByteArray();
            // byte[] IV = Guid.NewGuid().ToByteArray();

            // var encr = Encryption.EncryptStringToBytes_Aes(text, password, IV);

            // var roundTrip = Encryption.DecryptStringFromBytes_Aes(encr, password, IV);

            // Debug.Log(roundTrip);

            byte[] unencryptedKey = Guid.NewGuid().ToByteArray();

            // byte[] unencryptedKey = new byte[password.Length];

            // UTF8Encoding encoding = new UTF8Encoding();
            //encoding.GetBytes(password, 0, password.Length, unencryptedKey, 0);


            //SheetsManager.SaveSheet(sheetID, unencryptedKey);
            SheetsManager.CloseSheet(sheetID);

            string newSheetID = "";

            bool success = false; //SheetsManager.OpenCardSheet(GeneralSettings.Save_Location_Offline + "\\" + "My New Card Sheet.wmsht", unencryptedKey, out newSheetID);


            if (success)
            {
                Debug.Log(newSheetID);

                var newSheet = SheetsManager.GetActiveSheet(newSheetID);

                Debug.Log(newSheet.CurrentLayer.Name);
            }
            else
            {
                Debug.LogError("Sheet failed to open");
            }

            // var card = newSheet.GetObj(new Point(0, 0), newSheet.CurrentLayer);
            // for(int i = 0; i < 20; i++)
            // {
            //     for(int j = 0; j < 20; j++)
            //     {
            //         card = newSheet.GetObj(new Point(i, -j), newSheet.CurrentLayer);
            //         Debug.Log(card + " " + i.ToString() + " -" + j.ToString());
            //     }
            // }

            //SheetsManager.CloseSheet(newSheetID);

            //var newSheet = SheetsManager.GetActiveSheet(newSheetID);
            //Debug.Log(newSheet.GetObj(new Point(0,0), sheet.CurrentLayer));

            // 		}
            // 		catch (Exception e)
            // 		{
            // 			Debug.LogError(e.Message);
            // 		}
            // 		finally
            // 		{
            // 			SheetsManager.CloseSheet(sheetID);

            // #if UNITY_EDITOR
            // 			UnityEditor.EditorApplication.isPlaying = false;
            // #endif
            // 		}

        }

        private void AddCards(Sheet<Card> sheet)
        {
            List<Card> l = new List<Card>();

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    string id = i.ToString() + "" + j.ToString();
                    l.Add(new Card(new Point(i, -j), id, sheet.ID, sheet.CurrentLayer.ID));
                }
            }

            sheet.AddObjRange(l);

            //Debug.Log(sheet.GetObj(new Point(5, -5), sheet.CurrentLayer));
        }

        private void ReplaceCards(Sheet<Card> sheet)
        {
            Card c = new Card();

            for (int i = 0; i < 10; i++)
            {
                c = sheet.Replace(c, sheet.CurrentLayer);
                c.point = new Point(i, 0);
            }

            Debug.Log(sheet.GetObj(new Point(4, 0), sheet.CurrentLayer));

            Debug.Log(c.point);

            for (int j = 0; j < 24; j++)
            {
                if (c != null)
                {
                    c.point = new Point(0, -j);
                    c = sheet.Replace(c, sheet.CurrentLayer);
                }

            }

            Debug.Log(sheet.GetObj(new Point(0, -21), sheet.CurrentLayer).ToString());

        }

        private void SheetCloseMessenger(string id)
        {
            SheetsManager.OnCloseCardSheet -= SheetCloseMessenger;
            Debug.Log("Sheet " + id + " was closed");
        }
    }
}
