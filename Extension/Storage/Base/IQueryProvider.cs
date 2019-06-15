using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Storage.Providers;

namespace BatisSuperHelper.Storage.Interfaces
{
    public interface IQueryProvider<T, T1>
    {
        List<T1> GetAllValues();
        List<T> GetAllKeys();
        T1 GetValue(T key);
        T1 GetValueOrNull(T key);
        List<T> GetKeys(string queryId, bool useNamespace);
        void Add(T key, T1 value);
        void AddWithoutKey(T1 value);
        void AddMultiple(List<KeyValuePair<T, T1>> keyValuePairs);
        void AddMultipleWithoutKey(List<T1> values);
        List<T1> GetAllStatmentsByFileName(string fileName);
        void RemoveStatmentByValue(T1 value);
        void RemoveStatmentsForFilePath(string filePath);
        void RemoveStatmentsByDefinedObject(object obj);
        void UpdateStatmentsForFile(List<KeyValuePair<T, T1>> keyValuePairs);
        void UpdateStatmentForFileWihoutKey(List<T1> values);
        void RenameStatmentsForFile(string oldFileName, string newFileName);
        void RenameQuery(T key, string newQueryId);
        void Clear();
    }
}
