
using System;
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
    [Notes.Author("Handles the testing for the data sets and data set views")]
    public class test_DataSet : MonoBehaviour
    {

        /// <summary>
        /// The manual has an id upon creation (just has something)
        /// </summary>
        [Test]
        [Timeout(1)]
        public void ManualHasID()
        {
            DataSetView dataSetView = new DataSetView();
            Assert.IsNotNull(dataSetView.ID);
        }

        /// <summary>
        /// The manual cateogories is not null
        /// </summary>
        [Test]
        [Timeout(1)]
        public void ManualCategoriesIsNotNull()
        {
            DataSetView d = new DataSetView();
            Assert.IsNotNull(d.Categories);
        }

        /// <summary>
        /// The manual categories always must have something in the list
        /// </summary>
        [Test]
        [Timeout(1)]
        public void ManualCategoriesCountIsGreaterThanZero()
        {
            DataSetView m = new DataSetView();
            Assert.Greater(m.Categories.Length, 0);
        }

        [Test]
        [Timeout(5)]
        public void GetJSONFromManualIsNotNull()
        {
            DataSetView m = new DataSetView();
            Assert.IsNotNull(m.GetJson());
        }

        [Test]
        [Timeout(5)]
        public void DataSetViewEqualsFalse()
        {
            DataSetView m1 = new DataSetView();
            DataSetView m2 = new DataSetView();

            bool s = m1.Equals(m2);

            Assert.IsFalse(s);
        }

        [Test]
        [Timeout(5)]
        public void DataSetViewEqualsNullDoesNotThrowErrorAndEqualsFalse()
        {
            DataSetView m = new DataSetView();
            Assert.IsFalse(m.Equals(null));
        }

        [Test]
        [Timeout(5)]
        public void dataSetViewCompareToWorksWithViewName()
        {
            DataSetView m = new DataSetView();
            m.ViewName = "aaa";

            DataSetView m2 = new DataSetView();
            m2.ViewName = "bbb";

            DataSetView m3 = new DataSetView();
            m3.ViewName = "aaa";

            Assert.True(-1 == m.CompareTo(m2));
            Assert.True(1 == m2.CompareTo(m));
            Assert.True(0 == m.CompareTo(m3));
        }

        [Test]
        [Timeout(5)]
        public void CanReturnCorrectDataSetViewWithExactCategories()
        {
            string[] newCategories = new string[2]
            {
                "aa",
                "bb"
            };

            DataSetView manual = new DataSetView(newCategories);

            string[] compareCategories = new string[2]
            {
                "aa",
                "bb"
            };

            Assert.IsTrue(manual.CompareCategories(compareCategories));
        }

        [Test]
        [Timeout(5)]
        public void DataSetViewerIsGreedy()
        {
            DataSetView view = new DataSetView();
            Assert.IsTrue(view.Greedy);
        }

        [Test]
        [Timeout(5)]
        public void CanReturnCorrectDataSetViewWithOneExtraPermissionCategory()
        {
            string[] newCategories = new string[2]
            {
                "aa",
                "bb"
            };

            DataSetView manual = new DataSetView(newCategories);

            string[] compareCategories = new string[3]
            {
                "aa",
                "bb",
                "cc"
            };

            Assert.IsTrue(manual.CompareCategories(compareCategories));
        }

        [Test]
        [Timeout(5)]
        public void CanReturnCorrectDataSetViewWithOneExtraPermissionCategoryUnalphabetizedOrder()
        {
            string[] newCategories = new string[2]
            {
                "sas",
                "asd"
            };

            DataSetView manual = new DataSetView(newCategories);

            string[] compareCategories = new string[3]
            {
                "asd",
                "bb",
                "sas"
            };

            Assert.IsTrue(manual.CompareCategories(compareCategories));
        }

        [Test]
        [Timeout(5)]
        public void CanReturnFalseDataSetViewWithOneExtraPermissionCategoryUnalphabetizedOrder()
        {
            string[] newCategories = new string[2]
            {
                "sas",
                "asd"
            };

            DataSetView manual = new DataSetView(newCategories);

            string[] compareCategories = new string[3]
            {
                "asd",
                "bb",
                "ggg"
            };

            Assert.IsTrue(!manual.CompareCategories(compareCategories));
        }

        [Test]
        [Timeout(5)]
        public void CanReturnFalseDataSetViewWithOneExtraPermissionCategoryUnalphabetizedOrder2()
        {
            string[] newCategories = new string[3]
            {
                "sas",
                "asd",
                "ggg"
            };

            DataSetView manual = new DataSetView(newCategories);

            string[] compareCategories = new string[2]
            {
                "asd",
                "ggg"
            };

            Assert.IsTrue(!manual.CompareCategories(compareCategories));
        }
    }
}
