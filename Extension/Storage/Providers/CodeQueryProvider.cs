using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Storage.Interfaces;
using BatisSuperHelper.Storage.Providers;

namespace BatisSuperHelper.Storage.Domain
{
    public class CodeQueryProvider : IQueryProvider<IndexerKey, List<CSharpQuery>>
    {
        private readonly Dictionary<IndexerKey, List<CSharpQuery>> codeStatments = new Dictionary<IndexerKey, List<CSharpQuery>>();

        public void Add(IndexerKey key, List<CSharpQuery> value)
        {
            if (codeStatments.ContainsKey(key))
            {
                var codeStatmentsForKey = codeStatments[key];
                codeStatmentsForKey.AddRange(value);
                codeStatments[key] = codeStatmentsForKey;
            }
            else
            {
                codeStatments.Add(key, value);
            }
        }

        public void AddMultiple(List<KeyValuePair<IndexerKey, List<CSharpQuery>>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void AddMultipleWithoutKey(List<List<CSharpQuery>> values)
        {
            foreach (var value in values)
            {
                AddWithoutKey(value);
            }
        }

        private void AddSingleWithoutKey(CSharpQuery value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = MapNamespaceHelper.GetQueryWithoutNamespace(value.QueryId),
                VsProjectName = value.QueryVsProjectName,
                StatmentFullyQualifiedName = value.QueryId,
            };
            if (codeStatments.ContainsKey(key))
            {
                var codeStatmentsForKey = codeStatments[key];
                codeStatmentsForKey.Add(value);
                codeStatments[key] = codeStatmentsForKey;
            }
            else
            {
                var codeStamentsForKey = new List<CSharpQuery>
                {
                    value,
                };
                codeStatments.Add(key, codeStamentsForKey);
            }
        }

        public void AddWithoutKey(List<CSharpQuery> value)
        {
            if (!value.Any())
                return;

            foreach (var single in value)
            {
                AddSingleWithoutKey(single);
            }
        }

        public List<List<CSharpQuery>> GetAllStatmentsByFileName(string fileName)
        {
            return codeStatments.Values
               .Select(e => e.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList())
               .Where(x => x.Count != 0)
               .ToList();
        }

        public List<CSharpQuery> GetValue(IndexerKey key)
        {
            return codeStatments[key];
        }

        public List<CSharpQuery> GetValueOrNull(IndexerKey key)
        {
            if (codeStatments.ContainsKey(key))
            {
                return GetValue(key);
            }
            return null;
        }

        public List<List<CSharpQuery>> GetWhere(Func<List<CSharpQuery>, bool> predictable)
        {
            return codeStatments.Values.Where(predictable).ToList();
        }

        private List<IndexerKey> GetKeysByFullyQualifiedName(string queryId)
        {
            return codeStatments.Keys
                .Where(e => e.StatmentFullyQualifiedName == queryId)
                .ToList();
        }

        public List<IndexerKey> GetKeys(string queryId, bool useNamespace)
        {
            return GetKeysByFullyQualifiedName(queryId);
        }

        public void RemoveStatmentByValue(List<CSharpQuery> value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = MapNamespaceHelper.GetQueryWithoutNamespace(value.First().QueryId),
                VsProjectName = value.First().QueryFileName,
                StatmentFullyQualifiedName = value.First().QueryId,
            };
            var codeStatmentsForKey = codeStatments[key];
            codeStatmentsForKey.Remove(value.First());
            codeStatments[key] = codeStatmentsForKey;
        }
        public void RemoveStatmentsByDefinedObject(object obj)
        {
            var documentId = obj as DocumentId;
            List<int> hashCodes = codeStatments.Values.SelectMany(x => x
                    .Where(e => e.DocumentId.Equals(documentId))
                    .Select(statment => statment.HashCode))
                .ToList();
            foreach (var hashCode in hashCodes)
            {
                var singleStatment = codeStatments.FirstOrDefault(x => x.Value.Any(e => e.HashCode == hashCode));
                var oneOfListStatment = codeStatments[singleStatment.Key];
                if (singleStatment.Value.Count < 2)
                {
                    codeStatments.Remove(singleStatment.Key);
                }
                else
                {
                    oneOfListStatment.RemoveAll(e => e.HashCode == hashCode);
                    codeStatments[singleStatment.Key] = oneOfListStatment;
                }
            }
        }
        public void RemoveStatmentsForFilePath(string filePath)
        {
            List<int> hashCodes = codeStatments.Values.SelectMany(
                x => x
                .Where(e => e.QueryFilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase))
                .Select(statment => statment.HashCode))
                .ToList();
            foreach (var hashCode in hashCodes)
            {
                var singleStatment = codeStatments.FirstOrDefault(x => x.Value.Any(e => e.HashCode == hashCode));
                var oneOfListStatment = codeStatments[singleStatment.Key];
                if (singleStatment.Value.Count < 2)
                {
                    codeStatments.Remove(singleStatment.Key);
                }
                else
                {
                    oneOfListStatment.RemoveAll(e => e.HashCode == hashCode);
                    codeStatments[singleStatment.Key] = oneOfListStatment;
                }
            }
        }

        public void RenameQuery(IndexerKey key, string newQueryId)
        {
            var statments = codeStatments[key];
            foreach (var statment in statments)
            {
                statment.QueryId = newQueryId;
            }
            var newKey = new IndexerKey {
                StatmentName = MapNamespaceHelper.GetQueryWithoutNamespace(newQueryId),
                VsProjectName = key.VsProjectName,
                StatmentFullyQualifiedName = newQueryId,
            };
            codeStatments.Remove(key);
            if (codeStatments.ContainsKey(newKey))
            {
                codeStatments[newKey] = statments;
            }
            else
            {
                codeStatments.Add(newKey, statments);
            }
        }

        public void RenameStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileCodeStatments = codeStatments.Values.SelectMany(e => e.Where(x => x.QueryFileName.Equals(oldFileName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            foreach (var oldFileCodeStatment in oldFileCodeStatments)
            {
                oldFileCodeStatment.QueryFileName = newFileName;
                oldFileCodeStatment.QueryFilePath = oldFileCodeStatment.QueryFilePath.Replace(oldFileName, newFileName);
            }
        }

        public void UpdateStatmentsForFile(List<KeyValuePair<IndexerKey, List<CSharpQuery>>> keyValuePairs)
        {
            if (!keyValuePairs.Any() && !keyValuePairs.SelectMany(e => e.Value).Any())
                return;
            RemoveStatmentsForFilePath(keyValuePairs.First().Value.First().QueryFilePath);
            AddMultiple(keyValuePairs);
        }

        public void UpdateStatmentForFileWihoutKey(List<List<CSharpQuery>> values)
        {
            if (!values.Any() || !values.SelectMany(e=>e).Any())
                return;
            RemoveStatmentsForFilePath(values.First().First().QueryFilePath);
            AddMultipleWithoutKey(values);
        }

        public void Clear()
        {
            codeStatments.Clear();
        }

        public List<List<CSharpQuery>> GetAllValues()
        {
            return codeStatments.Select(e => e.Value).ToList();
        }

        public List<IndexerKey> GetAllKeys()
        {
            return codeStatments.Keys.ToList();
        }
    }
}
