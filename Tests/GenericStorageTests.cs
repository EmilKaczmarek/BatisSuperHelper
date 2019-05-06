using System;
using System.Linq;
using IBatisSuperHelper.Storage;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class GenericStorageTests
    {
        private GenericStorage<int, string> _storage;

        public GenericStorageTests()
        {
            _storage = new GenericStorage<int, string>();
        }

        [Fact]
        public void ShouldCallAddItemWithoutException()
        {
            _storage.Add(1, "1");
        }

        [Fact]
        public async Task ShouldCallAddItemWithoutExceptionAsync()
        {
            await _storage.AddAsync(2, "2");
        }

        [Fact]
        public void ShouldCallGetAllValuesWithoutException()
        {
            _storage.GetAllValues();
        }

        [Fact]
        public async Task ShouldCallGetAllValuesWithoutExceptionAsync()
        {
            await _storage.GetAllValuesAsync();
        }

        [Fact]
        public void ShouldAddAndGetSameItem()
        {
            var key = 1;
            var value = "1";
            _storage.Add(key, value);
            var result = _storage.GetValue(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task ShouldAddAndGetSameItemAsync()
        {
            var key = 1;
            var value = "1";
            await _storage.AddAsync(key, value);
            var result = await _storage.GetValueAsync(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void ShouldAddAndGetAllAddedItems()
        {
            _storage.Add(1, "1");
            _storage.Add(2, "2");
            _storage.Add(3, "3");
            var result = _storage.GetAllValues().ToList();
            Assert.Equal(3, result.Count);
            Assert.Equal("1", result[0]);
            Assert.Equal("2", result[1]);
            Assert.Equal("3", result[2]);
        }

        [Fact]
        public async Task ShouldAddAndGetAllAddedItemsAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.AddAsync(2, "2");
            await _storage.AddAsync(3, "3");
            var result = await _storage.GetAllValuesAsync();
            Assert.Equal(3, result.Count());
            Assert.Contains("1", result);
            Assert.Contains("2", result);
            Assert.Contains("3", result);
        }

        [Fact]
        public void ShouldAddAndRemoveItemByKey()
        {
            _storage.Add(1, "1");
            _storage.RemoveByKey(1);
            var result = _storage.GetAllValues().ToList();
            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldAddAndRemoveItemByKeyAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.RemoveByKeyAsync(1);
            var result = await _storage.GetAllValuesAsync();
            Assert.Empty(result);
        }

        [Fact]
        public void ShouldAddAndRemoveItemByPredictate()
        {
            _storage.Add(1, "1");
            _storage.RemoveWhereValue(e => e == "1");
            var result = _storage.GetAllValues().ToList();
            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldAddAndRemoveItemByPredictateAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.RemoveWhereValueAsync(e => e == "1");
            var result = await _storage.GetAllValuesAsync();
            Assert.Empty(result);
        }

        [Fact]
        public void ShouldAddAndUpdateValue()
        {
            _storage.Add(1, "1");
            _storage.Update(1, "2");
            var result = _storage.GetValue(1);
            Assert.Equal("2", result);
        }

        [Fact]
        public async Task ShouldAddAndUpdateValueAsync()
        {
            await _storage.AddAsync(1, "1");
            await _storage.UpdateAsync(1, "2");
            var result = await _storage.GetValueAsync(1);
            Assert.Equal("2", result);
        }

        [Fact]
        public void ShouldAddAndGetSameValueByPredictate()
        {
            _storage.Add(1, "1");
            var result = _storage.GetByPredictate(e => e == "1");
            Assert.Single(result);
            Assert.Equal("1", result.First());
        }

        [Fact]
        public void ShouldNotAddSameKeyValue()
        {
            _storage.Add(1, "1");
            _storage.Add(1, "2");
            var result = _storage.GetValue(1);
            Assert.Equal("1", result);
        }

        [Fact]
        public void ShouldNotUpdateNonExistingKeyValue()
        {
            _storage.Update(1, "1");
        }

    }
}
