
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Sharing;

namespace WarManager.Testing.Unit
{
    [Notes.Author("Handles the testing of the messaging system")]
    public class text_userNotifications : MonoBehaviour
    {

        string serverLocation = @"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\Data\Server notifications.json";

        //[Test]
        //public void test_CreateMessage_VerifyJsonExport()
        //{
        //    string fromUser = "taylor.howell@jrcousa.com";
        //    string[] toUsers = new string[2] { fromUser, fromUser + " test " };

        //    UserPersistantNotificationsHandler p = new UserPersistantNotificationsHandler();
        //    p.CreateNewNotification(Guid.NewGuid().ToString(), fromUser, toUsers, "test", "test");

        //    var notifications = p.GetNotificationsSent(fromUser);
        //    Assert.IsTrue(notifications.Count > 0);

        //    string result = "[{\"title\":\"test\",\"message\":\"test\",\"from\":\"taylor.howell@jrcousa.com\",\"to\":\"taylor.howell@jrcousa.com\",\"task\":false,\"complete\":false,\"instructions\":\"\"}]";
        //    Assert.IsTrue(p.GetAllMessagesJSON() == result);

        //    Debug.Log(result);
        //}

        //[Test]
        //public void test_DeserializeAddAndSerialize()
        //{
        //    string fromUser = "taylor.howell@jrcousa.com";


        //    UserPersistantNotificationsHandler p = new UserPersistantNotificationsHandler();

        //    string[] toUsers = new string[2] { fromUser, fromUser + " test " };

        //    p.Deserialize(serverLocation);
        //    p.CreateNewNotification(Guid.NewGuid().ToString(), fromUser, toUsers, "test", "test");
        //    Debug.Log(p.Notifications.Length);
        //    Debug.Log("Message: " + p.GetAllMessagesJSON());
        //    p.Serialize(serverLocation);
        //}

        //[Test]
        //public void test_GetUsers_RetreivalSuccessful()
        //{
        //    WarSystem.LoadPermissions();
        //   var accounts = Account.GetAccounts();
        //   Assert.IsTrue(accounts.Count > 0);
        //}
    }
}