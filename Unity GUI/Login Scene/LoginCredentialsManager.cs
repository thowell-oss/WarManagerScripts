using System.Collections;
using System.Collections.Generic;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using UnityEngine;

using RestSharp;

using UnityEngine.Networking;

namespace WarManager.Sharing.Security
{
    /// <summary>
    /// Handles checking the email validity and password validity
    /// </summary>
    public class LoginCredentialsManager
    {

        /// <summary>
        /// password instructions
        /// </summary>
        public static readonly string passwordInstructions = "Your password must contain 8 characters, one special character.one upper case and one lower case character.";

        public string VerifyEmail(string email)
        {
            return VerifyEmail(email, GeneralSettings.API_Key_EmailVerify);
        }

        /// <summary>
        /// Checks to see if an email is a valid email
        /// </summary>
        /// <param name="email">the email string to verify</param>
        /// <returns>Returns the email if the repsonse is successful, otherwise it returns the error message if the reponse failed<returns>
        private string VerifyEmail(string email, string key)
        {
            var client = new RestClient($"https://verifier.meetchopra.com/verify/{email}");

            var request = new RestRequest(Method.GET);
            request.AddQueryParameter("token",
             key);

            var response = client.Execute(request);
            var content = response.Content;

            bool status = false;

            using (var doc = JsonDocument.Parse(content))
            {
                var root = doc.RootElement;
                JsonElement jstatus = root.GetProperty("status");
                status = jstatus.GetBoolean();

                if (!status)
                {
                    var error = root.GetProperty("error");
                    var errorTitle = error.GetProperty("message");

                    return errorTitle.GetString();
                }
                else
                {
                    return email;
                }
            }
        }

        /// <summary>
        /// The password must contain at least 8 characters, one special character, upper and lower case letter
        /// </summary>
        /// <param name="password">the string password</param>
        /// <returns>returns true if the password is valid, false if not</returns>
        public bool VerifyPassword(string password)
        {
            return Regex.IsMatch(password, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{8,}$");
        }
    }
}
