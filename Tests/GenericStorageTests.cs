using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSIXProject5.Storage;

namespace Tests
{
    [TestClass]
    public class GenericStorageTests
    {
        private GenericStorage<int, string> _storage;


        [TestInitialize]
        public void Initialize()
        {
            _storage = new GenericStorage<int, string>();
        }

        [TestCleanup]
        public void CleanupAfterTest()
        {
            Initialize();
        }
        [TestMethod]
        public void ShouldCallAddItemWithoutException()
        {
            _storage.Add(1, "1");
            _storage.AddAsync(2, "2").Wait();
        }

        [TestMethod]
        public void ShouldCallGetAllValuesWithoutException()
        {
            _storage.GetAllValues();
            _storage.GetAllValuesAsync().Wait();
        }

        [TestMethod]
        public void ShouldAddAndGetSameItem()
        {
            var key = 1;
            var value = "1";
            _storage.Add(key, value);
            var result = _storage.GetValue(key);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ShouldAddAndGetSameItemAsync()
        {
            var key = 1;
            var value = "1";
            _storage.AddAsync(key, value).Wait();
            var result = _storage.GetValueAsync(key).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ShouldAddAndGetAllAddedItems()
        {
            _storage.Add(1, "1");
            _storage.Add(2, "2");
            _storage.Add(3, "3");
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0], "1");
            Assert.AreEqual(result[1], "2");
            Assert.AreEqual(result[2], "3");
        }

        [TestMethod]
        public void ShouldAddAndGetAllAddedItemsAsync()
        {
            _storage.AddAsync(1, "1").Wait();
            _storage.AddAsync(2, "2").Wait();
            _storage.AddAsync(3, "3").Wait();
            var result = _storage.GetAllValuesAsync().Result.ToList();
            Assert.AreEqual(result.Count, 3);
            Assert.IsTrue(result.Contains("1"));
            Assert.IsTrue(result.Contains("2"));
            Assert.IsTrue(result.Contains("3"));
        }

        [TestMethod]
        public void ShouldAddAndRemoveItemByKey()
        {
            _storage.Add(1, "1");
            _storage.RemoveByKey(1);
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void ShouldAddAndRemoveItemByKeyAsync()
        {
            _storage.AddAsync(1, "1").Wait();
            _storage.RemoveByKeyAsync(1).Wait();
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void ShouldAddAndRemoveItemByPredictate()
        {
            _storage.Add(1, "1");
            _storage.RemoveWhereValue(e => e == "1");
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void ShouldAddAndRemoveItemByPredictateAsync()
        {
            _storage.AddAsync(1, "1").Wait();
            _storage.RemoveWhereValueAsync(e => e == "1").Wait();
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void ShouldAddAndUpdateValue()
        {
            _storage.Add(1, "1");
            _storage.Update(1, "2");
            var result = _storage.GetValue(1);
            Assert.AreEqual("2", result);
        }

        [TestMethod]
        public void ShouldAddAndUpdateValueAsync()
        {
            _storage.AddAsync(1, "1").Wait();
            _storage.UpdateAsync(1, "2").Wait();
            var result = _storage.GetValue(1);
            Assert.AreEqual("2", result);
        }

        [TestMethod]
        public void ShouldAddAndGetSameValueByPredictate()
        {
            _storage.Add(1, "1");
            var result = _storage.GetByPredictate(e => e == "1");
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.First(), "1");
        }

        [TestMethod]
        public void ShouldNotAddSameKeyValue()
        {
            _storage.Add(1, "1");
            _storage.Add(1, "2");
            var result = _storage.GetValue(1);
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void ShouldNotUpdateNonExistingKeyValue()
        {
            _storage.Update(1, "1");
        }

    }
}
