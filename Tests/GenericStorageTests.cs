using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBatisSuperHelper.Storage;
using System.Threading.Tasks;

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
        }

        [TestMethod]
        public async Task ShouldCallAddItemWithoutExceptionAsync()
        {
            await _storage.AddAsync(2, "2");
        }

        [TestMethod]
        public void ShouldCallGetAllValuesWithoutException()
        {
            _storage.GetAllValues();
        }

        [TestMethod]
        public async Task ShouldCallGetAllValuesWithoutExceptionAsync()
        {
            await _storage.GetAllValuesAsync();
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
        public async Task ShouldAddAndGetSameItemAsync()
        {
            var key = 1;
            var value = "1";
            await _storage.AddAsync(key, value);
            var result = await _storage.GetValueAsync(key);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ShouldAddAndGetAllAddedItems()
        {
            _storage.Add(1, "1");
            _storage.Add(2, "2");
            _storage.Add(3, "3");
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [TestMethod]
        public async Task ShouldAddAndGetAllAddedItemsAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.AddAsync(2, "2");
            await _storage.AddAsync(3, "3");
            var result = await _storage.GetAllValuesAsync();
            Assert.AreEqual(3, result.Count());
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
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ShouldAddAndRemoveItemByKeyAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.RemoveByKeyAsync(1);
            var result = await _storage.GetAllValuesAsync();
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void ShouldAddAndRemoveItemByPredictate()
        {
            _storage.Add(1, "1");
            _storage.RemoveWhereValue(e => e == "1");
            var result = _storage.GetAllValues().ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ShouldAddAndRemoveItemByPredictateAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.RemoveWhereValueAsync(e => e == "1");
            var result = await _storage.GetAllValuesAsync();
            Assert.AreEqual(0, result.Count());
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
        public async Task ShouldAddAndUpdateValueAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.UpdateAsync(1, "2");
            var result = await _storage.GetValueAsync(1);
            Assert.AreEqual("2", result);
        }

        [TestMethod]
        public void ShouldAddAndGetSameValueByPredictate()
        {
            _storage.Add(1, "1");
            var result = _storage.GetByPredictate(e => e == "1");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("1", result.First());
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
