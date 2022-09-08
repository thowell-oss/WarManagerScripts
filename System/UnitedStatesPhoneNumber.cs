
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{

    /// <summary>
    /// The United States Phone Number style
    /// </summary>
    [Notes.Author("The united states style of phone number")]
    public struct UnitedStatesPhoneNumber : IEquatable<UnitedStatesPhoneNumber>, IComparable<UnitedStatesPhoneNumber>
    {
        /// <summary>
        /// The full US style phone number (including the country code digit) -> +xxxxxxxxxxx
        /// </summary>

        public string FullNumberUS => "+1" + AreaCode + MiddleThree + LastFour;

        /// <summary>
        /// The US style phone number (not including the country code) -> (xxx) xxx - xxxx
        /// </summary>
        /// <returns></returns>
        public string NumberUS => "(" + AreaCode + ") " + MiddleThree + " - " + LastFour;

        /// <summary>
        /// The area code
        /// </summary>
        /// <value></value>
        public string AreaCode { get; private set; }

        /// <summary>
        /// The middle three digits
        /// </summary>
        /// <value></value>
        public string MiddleThree { get; private set; }

        /// <summary>
        /// The last four digits
        /// </summary>
        /// <value></value>
        public string LastFour { get; private set; }

        /// <summary>
        /// Is there an error with this phone number (that it might be able to be displayed but not used)?
        /// </summary>
        /// <value></value>
        public bool Error { get; set; }

        /// <summary>
        /// Is this phone number a default number?
        /// </summary>
        /// <returns></returns>
        public bool IsDefault => UnitedStatesPhoneNumber.IsDefaultPhoneNumber(this.FullNumberUS);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="area">the area code</param>
        /// <param name="middleThree">the middle three</param>
        /// <param name="lastFour">the last four digits</param>
        public UnitedStatesPhoneNumber(string area, string middleThree, string lastFour)
        {
            if (area.Length != 3)
                throw new ArgumentOutOfRangeException("the area code is not the correct string length: " + area);

            foreach (var x in area)
            {
                if (!Char.IsDigit(x))
                {
                    throw new ArgumentException("a character is not a digit in area code " + area);
                }
            }

            if (middleThree.Length != 3)
                throw new ArgumentOutOfRangeException("the middle three is not the correct string length: " + middleThree);

            foreach (var x in middleThree)
            {
                if (!Char.IsDigit(x))
                {
                    throw new ArgumentException("a character is not a digit in middle three digits " + middleThree);
                }
            }

            if (lastFour.Length != 4)
                throw new ArgumentOutOfRangeException("the last four digits are not the correct string length: " + lastFour);

            foreach (var x in lastFour)
            {
                if (!Char.IsDigit(x))
                {
                    throw new ArgumentException("a character is not a digit in the last four digits " + lastFour);
                }
            }

            AreaCode = area;
            MiddleThree = middleThree;
            LastFour = lastFour;
            Error = false;
        }

        /// <summary>
        /// Try parse a string into the united states phone standard
        /// </summary>
        /// <param name="strIn">the string in</param>
        /// <param name="outResult">the resulting United States Phone Number class</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryParse(string strIn, out UnitedStatesPhoneNumber outResult)
        {
            if (strIn == null || strIn == string.Empty)
            {
                var number = new UnitedStatesPhoneNumber("000", "000", "0000");

                Debug.Log("empty phone number");

                number.Error = false;

                outResult = number;
                return false;
            }

            string str = "";

            int totalDigits = 10;

            int i = 0;
            bool foundCountryCode = false;

            while (i < strIn.Length && str.Length < totalDigits)
            {
                char character = strIn[i];

                if (char.IsDigit(character))
                {
                    if (str.Length == 0 && character == '1' && foundCountryCode)
                    {
                        foundCountryCode = false;
                    }
                    else
                    {
                        str += character;
                    }
                }
                else if (str.Length == 0 && character == '+')
                {
                    foundCountryCode = true;
                }
                else if (character == '\n' || character == '\b' || character == '\r' || character == '\t')
                {
                    var number = new UnitedStatesPhoneNumber("000", "000", "0000");
                    number.Error = true;
                    outResult = number;
                    return false;
                }

                i++;
            }

            // if (foundCountryCode)
            // {
            //     str = str.Remove(0, 2);
            // }

            // Debug.Log(str);
            // Debug.Log(str.Length);

            if (str.Length == totalDigits)
            {

                string area = str.Substring(0, 3);
                string middleThree = str.Substring(3, 3);
                string lastFour = str.Substring(6, 4);

                // Debug.Log(str);
                // Debug.Log(area);
                // Debug.Log(middleThree);
                // Debug.Log(lastFour);

                try
                {
                    UnitedStatesPhoneNumber number = new UnitedStatesPhoneNumber(area, middleThree, lastFour);
                    number.Error = false;
                    outResult = number;
                    return true;
                }
                catch (Exception ex)
                {
                    var y = new UnitedStatesPhoneNumber("000", "000", "0000");
                    y.Error = true;

                    outResult = y;
                    return false;
                }
            }

            Debug.Log("total digits incorrect");

            var x = new UnitedStatesPhoneNumber("000", "000", "0000");
            x.Error = true;

            outResult = x;
            return false;
        }


        /// <summary>
        /// Is the phone number (000) 000 - 0000 (default)
        /// </summary>
        /// <param name="phone">default phone number</param>
        /// <returns>returns true if the phone is the default number, false if not</returns>
        public static bool IsDefaultPhoneNumber(string phone)
        {
            if (phone == "+10000000000")
                return true;

            return false;
        }

        public override string ToString()
        {
            return NumberUS;
        }

        public static bool operator ==(UnitedStatesPhoneNumber a, UnitedStatesPhoneNumber b)
        {
            return a.FullNumberUS == b.FullNumberUS;
        }

        public static bool operator !=(UnitedStatesPhoneNumber a, UnitedStatesPhoneNumber b)
        {
            return !(a == b);
        }

        public static bool operator ==(UnitedStatesPhoneNumber a, string b)
        {
            if (UnitedStatesPhoneNumber.TryParse(b, out var phoneNumber))
            {
                return a == phoneNumber;
            }

            return false;
        }

        public static bool operator !=(UnitedStatesPhoneNumber a, string b)
        {
            return !(a == b);
        }

        public static bool operator ==(string b, UnitedStatesPhoneNumber a)
        {
            if (UnitedStatesPhoneNumber.TryParse(b, out var phoneNumber))
            {
                return a == phoneNumber;
            }

            return false;
        }

        public static bool operator !=(string b, UnitedStatesPhoneNumber a)
        {
            return !(a == b);
        }

        public bool Equals(UnitedStatesPhoneNumber other)
        {
            return this == other;
        }

        public int CompareTo(UnitedStatesPhoneNumber other)
        {
            if (other == null)
                return 1;

            return FullNumberUS.CompareTo(other.FullNumberUS);
        }

        public override bool Equals(object obj)
        {
            if (obj is UnitedStatesPhoneNumber num)
            {
                return this == num;
            }

            if (obj is string str)
            {
                if (UnitedStatesPhoneNumber.TryParse(str, out var phone))
                {
                    return this == phone;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}