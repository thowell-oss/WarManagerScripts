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
    [Notes.Author("United States Phone Number Tests")]
    public class test_PhoneNumber : MonoBehaviour
    {
        [Test]
        public void TestEmptyPhoneNumber()
        {
            UnitedStatesPhoneNumber PhoneNumber = new UnitedStatesPhoneNumber("000", "000", "0000");
            Assert.IsTrue(PhoneNumber.IsDefault);
        }

        [Test]
        public void TestPhoneNumberCorrect()
        {
            UnitedStatesPhoneNumber PhoneNumber = new UnitedStatesPhoneNumber("123", "456", "7890");

            Assert.IsFalse(PhoneNumber.IsDefault);
            Assert.IsFalse(PhoneNumber.Error);
        }

        [Test]
        public void TestIncorrectPhoneNumberAtLastDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UnitedStatesPhoneNumber("123", "123", "123"));
        }

        [Test]
        public void TestIncorrectPhoneNumberAtFirstDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UnitedStatesPhoneNumber("12", "123", "1234"));
        }

        [Test]
        public void TestIncorrectPhoneNumberAtMiddleDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UnitedStatesPhoneNumber("123", "12", "1234"));
        }

        [Test]
        public void TestIncorrectCharacterAtFirstDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new UnitedStatesPhoneNumber("12d", "123", "1234"));
        }

        [Test]
        public void TestIncorrectCharacterAtMiddleDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new UnitedStatesPhoneNumber("123", "1gd", "1234"));
        }

        [Test]
        public void TestIncorrectCharacterAtLastDigitsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new UnitedStatesPhoneNumber("123", "123", "1d34"));
        }

        [Test]
        public void TestParseCorrectPhoneNumbers()
        {

            for (int i = 0; i < ListOfPhoneNumbers().Length; i++)
            {

                var parseResult = UnitedStatesPhoneNumber.TryParse(ListOfPhoneNumbers()[i], out var phone);

                Assert.IsTrue(parseResult);
                Assert.AreEqual(ListOfHumanReadablePhoneNumbers()[i], phone.NumberUS);
                Assert.AreEqual("+1" + ListOfPhoneNumbers()[i], phone.FullNumberUS);
            }
        }

        [Test]
        public void TestParseIncorrectLengthPhoneNumber()
        {
            var parseResult = UnitedStatesPhoneNumber.TryParse("12345678", out var phone);

            Assert.IsFalse(parseResult);
            Assert.IsTrue(phone.Error);
        }

        [Test]
        public void TestParseCorrectPhoneNumberWithGarbage1()
        {
            var parseResult = UnitedStatesPhoneNumber.TryParse(" (234) 344-3333", out var result);

            Assert.IsTrue(parseResult);
            Assert.IsFalse(result.Error);
            Assert.IsFalse(result.IsDefault);

            Assert.AreEqual("(234) 344 - 3333", result.NumberUS);
            Assert.AreEqual("+12343443333", result.FullNumberUS);
        }

        [Test]
        public void TestParseCorrectPhoneNumberWithGarbage2()
        {
            var parseResult = UnitedStatesPhoneNumber.TryParse(" (234) 34g4- 33d3334 ", out var result);

            Assert.IsTrue(parseResult);
            Assert.IsFalse(result.Error);
            Assert.IsFalse(result.IsDefault);

            Assert.AreEqual("(234) 344 - 3333", result.NumberUS);
            Assert.AreEqual("+12343443333", result.FullNumberUS);
        }

        [Test]
        public void TestParseIncorrectPhoneNumberWithGarbage()
        {
            var parseResult = UnitedStatesPhoneNumber.TryParse("+1 (234\n\t) 34g4- 33d3334 ", out var result);

            Assert.IsFalse(parseResult);
            Assert.IsTrue(result.Error);
            Assert.IsTrue(result.IsDefault);

            Assert.AreNotEqual("(234) 344 - 3333", result.NumberUS);
            Assert.AreNotEqual("+12343443333", result.FullNumberUS);

            Assert.AreEqual("(000) 000 - 0000", result.NumberUS);
            Assert.AreEqual("+10000000000", result.FullNumberUS);
        }

        private string[] ListOfPhoneNumbers()
        {
            return new string[6] { "1234567890", "1293944556", "1209433334", "9540039483", "3409484853", "8739476990" };
        }

        private string[] ListOfHumanReadablePhoneNumbers()
        {
            return new string[6] { "(123) 456 - 7890", "(129) 394 - 4556", "(120) 943 - 3334", "(954) 003 - 9483", "(340) 948 - 4853", "(873) 947 - 6990" };
        }
    }
}

