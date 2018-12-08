﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage.Interfaces;

namespace VSIXProject5.Storage.Domain
{
    public class CodeQueryProvider : IProvider<IndexerKey, List<CSharpIndexerResult>>
    {
        private Dictionary<IndexerKey, List<CSharpIndexerResult>> codeStatments = new Dictionary<IndexerKey, List<CSharpIndexerResult>>();

        public void Add(IndexerKey key, List<CSharpIndexerResult> value)
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

        public void AddMultiple(List<KeyValuePair<IndexerKey, List<CSharpIndexerResult>>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void AddMultipleWithoutKey(List<List<CSharpIndexerResult>> values)
        {
            foreach (var value in values)
            {
                AddWithoutKey(value);
            }
        }

        private void AddSingleWithoutKey(CSharpIndexerResult value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.QueryId,
                VsProjectName = value.QueryVsProjectName
            };
            if (codeStatments.ContainsKey(key))
            {
                var codeStatmentsForKey = codeStatments[key];
                codeStatmentsForKey.Add(value);
                codeStatments[key] = codeStatmentsForKey;
            }
            else
            {
                var codeStamentsForKey = new List<CSharpIndexerResult>
                {
                    value,
                };
                codeStatments.Add(key, codeStamentsForKey);
            }
        }

        public void AddWithoutKey(List<CSharpIndexerResult> value)
        {
            if (!value.Any())
                return;

            foreach (var single in value)
            {
                AddSingleWithoutKey(single);
            }
        }

        public List<List<CSharpIndexerResult>> GetAllStatmentsByFileName(string fileName)
        {
            return codeStatments.Values
               .Select(e => e.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList())
               .Where(x => x.Count != 0)
               .ToList();
        }

        public List<CSharpIndexerResult> GetValue(IndexerKey key)
        {
            return codeStatments[key];
        }

        public List<CSharpIndexerResult> GetValueOrNull(IndexerKey key)
        {
            if (codeStatments.ContainsKey(key))
            {
                return GetValue(key);
            }
            return null;
        }

        public List<List<CSharpIndexerResult>> GetWhere(Func<List<CSharpIndexerResult>, bool> predictable)
        {
            return codeStatments.Values.Where(predictable).ToList();
        }

        public List<IndexerKey> GetKeysByQueryId(string queryId)
        {
            return codeStatments.Keys
                .Where(e => e.StatmentName == queryId)
                .ToList();
        }

        public void RemoveStatmentByValue(List<CSharpIndexerResult> value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.First().QueryId,
                VsProjectName = value.First().QueryFileName
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
            var newKey = new IndexerKey { StatmentName = newQueryId, VsProjectName = key.VsProjectName };
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

        public void UpdateStatmentsForFile(List<KeyValuePair<IndexerKey, List<CSharpIndexerResult>>> keyValuePairs)
        {
            if (!keyValuePairs.Any() && !keyValuePairs.SelectMany(e => e.Value).Any())
                return;
            RemoveStatmentsForFilePath(keyValuePairs.First().Value.First().QueryFilePath);
            AddMultiple(keyValuePairs);
        }

        public void UpdateStatmentForFileWihoutKey(List<List<CSharpIndexerResult>> values)
        {
            if (!values.Any() && !values.SelectMany(e=>e).Any())
                return;
            RemoveStatmentsForFilePath(values.First().First().QueryFilePath);
            AddMultipleWithoutKey(values);
        }

        public void Clear()
        {
            codeStatments.Clear();
        }
    }
}