using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Storage
{
    public class GenericStorage<T, T1>
    {
        private ConcurrentDictionary<T, Lazy<T1>> keyValuePairs = new ConcurrentDictionary<T, Lazy<T1>>();

        public bool Add(T key, T1 value)
        {
            return keyValuePairs.TryAdd(key, new Lazy<T1>(() => value));
        }

        public async Task<bool> AddAsync(T key, T1 value)
        {
            return await Task.Run(() => keyValuePairs.TryAdd(key, new Lazy<T1>(() => value)));
        }

        public void AddMultiple(IEnumerable<KeyValuePair<T, T1>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                this.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public async Task AddMultipleAsync(IEnumerable<KeyValuePair<T, T1>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                await this.AddAsync(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public IEnumerable<T1> GetAllValues()
        {
            return keyValuePairs.Values.Select(e => e.Value);
        }

        public async Task<IEnumerable<T1>> GetAllValuesAsync()
        {
            return await Task.Run(() => GetAllValues());
        }

        public IEnumerable<T1> GetByPredictate(Func<T1, bool> predictate)
        {
            return keyValuePairs.Select(e => e.Value.Value).Where(e => predictate?.Invoke(e) ?? default(bool));

        }

        public async Task<IEnumerable<T1>> GetByPredictateAsync(Func<T1, bool> predictate)
        {
            return await Task.Run(() => GetByPredictate(predictate));
        }

        public T1 GetValue(T key)
        {
            return keyValuePairs[key].Value;
        }

        public async Task<T1> GetValueAsync(T key)
        {
            return await Task.Run(() => GetValue(key));
        }
        public void RemoveByKey(T key)
        {
            keyValuePairs.TryRemove(key, out Lazy<T1> value);
        }

        public async Task RemoveByKeyAsync(T key)
        {
            await Task.Run(() => RemoveByKey(key));
        } 

        public void RemoveWhereValue(Func<T1,bool> predictate)
        {
            var matchingKeys = keyValuePairs.Where(e=> predictate?.Invoke(e.Value.Value) ?? default(bool)).Select(e=>e.Key);
            foreach (var matchingKey in matchingKeys)
            {
                RemoveByKey(matchingKey);
            }
        }

        public async Task RemoveWhereValueAsync(Func<T1, bool> predictate)
        {
            var matchingKeys = keyValuePairs.Where(e => predictate?.Invoke(e.Value.Value) ?? default(bool)).Select(e => e.Key);
            foreach (var matchingKey in matchingKeys)
            {
                await RemoveByKeyAsync(matchingKey);
            }
        }

        public void Update(T key, T1 newValue)
        {
            if (keyValuePairs.TryGetValue(key, out Lazy<T1> oldValue))
            {
                keyValuePairs.TryUpdate(key, new Lazy<T1>(() => newValue), oldValue);
            }
        }

        public async Task UpdateAsync(T key, T1 newValue)
        {
            var oldValue = keyValuePairs[key];
            await Task.Run(()=> keyValuePairs.TryUpdate(key, new Lazy<T1>(() => newValue), oldValue)); 
        }
    }
}
