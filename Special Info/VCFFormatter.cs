
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VCFFormatter
{
    /// <summary>
    /// Create a vcf card
    /// </summary>
    /// <param name="fullName">full name</param>
    /// <param name="position">the position of the person</param>
    /// <param name="organization">the organization</param>
    /// <param name="email">the email</param>
    /// <param name="phone">the phone</param>
    /// <param name="address">address</param>
    /// <returns>returns a string</returns>
    [Obsolete("Untested")]
    public static string Create(string fullName, string position, string organization, string email, string phone, string address)
    {
        StringBuilder vcf = new StringBuilder();
        vcf.AppendLine("BEGIN:VCARD");
        vcf.AppendLine("VERSION:2.1");

        vcf.AppendLine("FN:" + fullName);
        vcf.AppendLine("TITLE:" + position);
        vcf.AppendLine("ORG:" + organization);
        vcf.AppendLine("EMAIL;PREF;INTERNET:" + email);
        vcf.AppendLine("TEL:CELL;VOICE" + phone);
        vcf.AppendLine("ADR;HOME;PREF:" + address);

        vcf.AppendLine("END:VCARD");

        return vcf.ToString();
    }
}
