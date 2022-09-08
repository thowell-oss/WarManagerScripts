
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

using WarManager.Sharing.Security;



namespace WarManager.Testing.Unit
{
    [Notes.Author("Permissions Tests")]
    public class test_Permissions : MonoBehaviour
    {

        [Test]
        public void CreatePermissionCorrectInit()
        {
            foreach (var x in GetNames())
            {

                var categories = GetCategories();

                Permissions perm = new Permissions(x, false, categories);
                Assert.AreEqual(x, perm.Name);
                Assert.AreEqual(categories, perm.PermissionCategories);

                Assert.IsFalse(perm.IsAdmin);

                Assert.IsFalse(perm.ContainsAllCategoriesAccessCharacter);
                Assert.AreEqual(categories.Length, perm.PermissionCategories.Length);
            }
        }

        [Test]
        public void ComparePermissions()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());
            Permissions permissions1 = new Permissions("test 2", false, GetCategoriesWithLessToCompare());

            Assert.IsFalse(permissions.ContainsAllCategoriesAccessCharacter);
            Assert.IsFalse(permissions1.ContainsAllCategoriesAccessCharacter);


            Assert.IsFalse(permissions.CompareTo(permissions1) == 0);

            Permissions permissions3 = new Permissions("test 3 ", false, GetCategories());
            Assert.IsFalse(permissions3.ContainsAllCategoriesAccessCharacter);

            Assert.IsFalse(permissions.CompareTo(permissions3) == 0);
        }

        [Test]
        public void ContainsKeyWord()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            for (int i = 0; i < GetCategories().Length; i++)
            {
                Assert.IsTrue(permissions.ContainsKeywordPermission(GetCategories()[i]));
            }

            Assert.IsFalse(permissions.ContainsKeywordPermission("sdfasdfasdfasdf"));
        }

        [Test]
        public void PermissionsIsAdmin()
        {
            Permissions permissions = new Permissions("test", true, GetCategories());
            Assert.IsTrue(permissions.IsAdmin);
            Assert.IsFalse(permissions.ContainsAllCategoriesAccessCharacter);
        }

        [Test]
        public void PermissionsIsStar()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "*" });
            Assert.IsFalse(permissions.IsAdmin);
            Assert.IsTrue(permissions.ContainsAllCategoriesAccessCharacter);
        }

        [Test]
        public void PermissionsIsStarAndAdmin()
        {
            Permissions permissions = new Permissions("test", true, new string[] { "*" });
            Assert.IsTrue(permissions.IsAdmin);
            Assert.IsTrue(permissions.ContainsAllCategoriesAccessCharacter);
        }

        [Test]
        public void TestPermissionsGreedyCanAccess()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions >= GetCategoriesWithLessToCompare();

            Assert.IsTrue(x);
        }


        [Test]
        public void TestPermissionsGreedyCannotAccess()
        {
            Permissions permissions = new Permissions("test", false, GetCategoriesWithLessToCompare());

            bool x = permissions >= GetCategories();

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCanAccess()
        {
            Permissions permissions = new Permissions("test", false, GetCategoriesWithLessToCompare());

            bool x = permissions <= GetCategories();

            Assert.IsTrue(x);
        }

        [Test]
        public void TestPermissionsWithGreedyCanAccessWithStar()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "* " });

            bool x = permissions >= GetCategories();

            Assert.IsTrue(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCanAccessWithStar()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "* " });

            bool x = permissions <= GetCategories();

            Assert.IsTrue(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCannotAccessWithStarIncorrect()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "* sanity" });

            bool x = permissions <= GetCategories();

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsGreedyCannotAccessWithStarIncorrect()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "* sanity", "natant" });

            bool x = permissions >= GetCategories();

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsGreedyCannotAccessWithStarIncorrectLocation()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "natant", "*", });

            bool x = permissions >= GetCategories();

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCannotAccessWithStarIncorrectLocation()
        {
            Permissions permissions = new Permissions("test", false, new string[] { "natant", "*", });

            bool x = permissions <= GetCategories();

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCategoryContainsStar()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions <= new string[1] { "* " };

            Assert.IsTrue(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCategoryContainsStarAtIncorrectLocation()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions <= new string[2] { "test", "* " };

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsNonGreedyCategoryContainsStarWithOtherTextIncorrect()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions <= new string[1] { "*test " };

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsGreedyCategoryContainsStar()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions >= new string[2] { "* ", "test" };

            Assert.IsTrue(x);
        }

        [Test]
        public void TestPermissionsGreedyCategoryContainsStarAtIncorrectLocation()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions >= new string[2] { "test", "* " };

            Assert.IsFalse(x);
        }

        [Test]
        public void TestPermissionsGreedyCategoryContainsStarWithOtherTextIncorrect()
        {
            Permissions permissions = new Permissions("test", false, GetCategories());

            bool x = permissions >= new string[1] { "*test " };

            Assert.IsFalse(x);
        }

        private string[] GetNames()
        {
            return new string[6] { "test permissions", "War Manager Developer", "10 Mo Comm", "Administrator", "Toughly", "Later Street" };
        }

        private string[] GetCategories()
        {
            return new string[] { "abc", "def", "ghi", "Admin", "developer", "10 Mo Comm employees" };
        }


        private string[] GetCategoriesWithLessToCompare()
        {
            return new string[] { "Admin", "def", "10 Mo Comm employees" };
        }


    }
}
