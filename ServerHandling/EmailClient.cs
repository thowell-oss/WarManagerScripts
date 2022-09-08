
/* EmailClient.cs
 */

using System;
using System.Net.Mail;

using System.Collections.Generic;

using System.Threading.Tasks;
using System.Threading;

using WarManager.Cards;
using WarManager.Backend;

using StringUtility;

using WarManager.Sharing;

namespace WarManager
{
    /// <summary>
    /// Email client
    /// </summary>
    [Notes.Author("Simple Email Handling Client")]
    public static class EmailClient
    {
        /// <summary>
        /// Send an SMTP email through the War Manager Assistant gmail account
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static void SendQuickEmail(string to, string subject, string message)
        {
            SendSMTPEmail(new string[1] { to }, subject, message);
        }

        /// <summary>
        /// Send a notification SMTP email to the developer
        /// </summary>
        /// <param name="subject">the subject of the email to send</param>
        /// <param name="message">the message of the email</param>
        public static void SendNotificationSMTPEmailToDev(string subject, string message, bool SendAsync = true)
        {
            // UnityEngine.Debug.Log("email sent");

            bool send = true;

#if UNITY_EDITOR
            send = false;
#endif

            if (send)
            {
                SendSMTPEmail(new string[1] { "howelltaylor195@gmail.com" }, subject, message, false, SendAsync);

                try
                {
                    TwilioSMSHandler handler = new TwilioSMSHandler();
                    Task.Run(() => handler.SendMessage(subject + "\n" + message, "+19137497477", false, true));
                }
                catch (Exception ex)
                {
                    WarSystem.WriteToLog("Error using twilio handler " + ex.Message, Logging.MessageType.error);
                }
            }
        }



        /// <summary>
        /// Send an email through the War Manager Assistant gmail account
        /// </summary>
        /// <param name="to">the array of email addresses to go to</param>
        /// <param name="subject">the subject of the email</param>
        /// <param name="message">the message body to send</param>
        /// <param name="includeRobotNotifications">should there be notices on the email subject and body notifying the reciever this email is a robot email?</param>
        /// <returns>returns true if the message sent was successful, false if not</returns>
        public static bool SendSMTPEmail(string[] to, string subject, string message, bool includeRobotNotifications = true, bool SendAsync = true)
        {
            // Command-line argument must be the SMTP host.
            // SmtpClient client = new SmtpClient("smtp.gmail.com");
            // client.Port = 587;
            // client.Credentials = new System.Net.NetworkCredential("warmanagerassistant@gmail.com", "yisxdggenhnqlfjz");

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("warmanagerassistant@gmail.com");

                for (int i = 0; i < to.Length; i++)
                {
                    if (to[i] != null && to[i].Length > 0)
                    {

                        // #if UNITY_EDITOR
                        //                         if (to[i] != "howelltaylor195@gmail.com")
                        //                         {
                        //                             to[i] = "howelltaylor195@gmail.com";
                        //                             message = "to " + to[i] + " - " + message;
                        //                         }
                        // #endif

                        mail.To.Add(to[i]);
                    }
                }

                mail.Subject = subject;

                message = message.Replace("\\n", "\n");

                message = string.Join("<br />", message.Split('\n'));

                UnityEngine.Debug.Log(message);

                mail.Body = "<p>" + message + "</p>";

                mail.IsBodyHtml = true;


                if (includeRobotNotifications)
                {
                    mail.Subject += " (War Manager Robot Email)";
                    mail.Body += "\n\n\t<p><i>This message was sent by the War Manager robot assistant. Please contact Taylor Howell at taylor.howell@jrcousa.com for any comments, questions or concerns.</i></p>";
                }

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("warmanagerassistant@gmail.com", "yisxdggenhnqlfjz");
                SmtpServer.EnableSsl = true;

                if (SendAsync)
                    SmtpServer.SendAsync(mail, "message");
                else
                {
                    SmtpServer.Send(mail);
                }

                return true;
            }

            catch (Exception ex)
            {
                WarSystem.WriteToLog("email failed to send " + ex.Message, Logging.MessageType.error);
                return false;
            }
        }

        public static void SendEmailsFromDataEntries(List<DataEntry> entries, string subject, string message)
        {
            if (entries == null)
                throw new NullReferenceException("The entries cannot be null");

            if (entries.Count < 1)
                throw new ArgumentException("The entries list cannot be empty");

            var en = entries[0];

            var data = en.GetAllowedDataValues();

            int loc = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].ParseToParagraph().IsEmailString())
                {
                    loc = data[i].GetColumnInfo().ColumnLocation;
                    break;
                }
            }

            List<string> emails = new List<string>();
            string userEmail = WarSystem.CurrentActiveAccount.UserName.Trim();
            emails.Add(userEmail);

            foreach (var x in entries)
            {
                if (x.TryGetValueAt(loc, out var ele))
                {
                    string email = ele.ParseToParagraph();

                    if (email.Trim() != userEmail)
                    {
                        emails.Add(email);
                    }
                }
            }

            try
            {
                SendSMTPEmail(emails.ToArray(), subject, message);
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }
        }
    }
}
