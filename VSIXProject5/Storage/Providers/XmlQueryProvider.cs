﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage.Interfaces;

namespace VSIXProject5.Storage.Domain
{
    public class XmlQueryProvider : IProvider<IndexerKey, XmlIndexerResult>
    {
        private Dictionary<IndexerKey, XmlIndexerResult> xmlStatments = new Dictionary<IndexerKey, XmlIndexerResult>();

        public XmlIndexerResult GetValue(IndexerKey key)
        {
            return xmlStatments[key];
        }

        public XmlIndexerResult GetValueOrNull(IndexerKey key)
        {
            if (xmlStatments.ContainsKey(key))
            {
                return GetValue(key);
            }
            return null;
        }

        public void Add(IndexerKey key, XmlIndexerResult value)
        {          
            if (!xmlStatments.ContainsKey(key))
            {
                xmlStatments.Add(key, value);
            }
        }

        public void AddWithoutKey(XmlIndexerResult value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.QueryId,
                VsProjectName = value.QueryVsProjectName
            };

            Add(key, value);
        }

        public void AddMultipleWithoutKey(List<XmlIndexerResult> values)
        {
            if (!values.Any())
                return;

            foreach (var value in values)
            {
                AddWithoutKey(value as XmlIndexerResult);
            }
        }

        public void AddMultiple(List<KeyValuePair<IndexerKey, XmlIndexerResult>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public List<XmlIndexerResult> GetAllStatmentsByFileName(string fileName)
        {
            return xmlStatments.Values.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        public List<IndexerKey> GetKeysByQueryId(string queryId)
        {
            return xmlStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }

        public void RemoveStatmentByValue(XmlIndexerResult value)
        {
            xmlStatments.Remove(new IndexerKey { StatmentName = value.QueryId, VsProjectName = value.QueryFileName });
        }

        public void RemoveStatmentsForFilePath(string filePath)
        {
            var statmentsToRemove = xmlStatments.Where(e => e.Value.QueryFilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase));
            foreach (var statment in statmentsToRemove.ToList())
            {
                xmlStatments.Remove(statment.Key);
            }
        }

        public void RemoveStatmentsByDefinedObject(object obj)
        {
            RemoveStatmentsForFilePath(obj as string);
        }

        public void UpdateStatmentsForFile(List<KeyValuePair<IndexerKey, XmlIndexerResult>> keyValuePairs)
        {
            if (!keyValuePairs.Any())
                return;
            RemoveStatmentsForFilePath(keyValuePairs.First().Value.QueryFilePath);
            AddMultiple(keyValuePairs);
        }

        public void UpdateStatmentForFileWihoutKey(List<XmlIndexerResult> results)
        {
            if (!results.Any())
                return;
            RemoveStatmentsForFilePath(results.First().QueryFilePath);
            AddMultipleWithoutKey(results);
        }

        public void RenameStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileXmlStatments = xmlStatments.Where(e => e.Value.QueryFileName.Equals(oldFileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
            foreach (var xmlFileStatment in oldFileXmlStatments)
            {
                xmlStatments[xmlFileStatment.Key].QueryFileName = newFileName;
                xmlStatments[xmlFileStatment.Key].QueryFilePath = xmlStatments[xmlFileStatment.Key].QueryFilePath.Replace(oldFileName, newFileName);
            }
        }

        public void RenameQuery(IndexerKey key, string newQueryId)
        {
            var statmentInfo = xmlStatments[key];
            var newKey = new IndexerKey { StatmentName = newQueryId, VsProjectName = key.VsProjectName };
            statmentInfo.QueryId = newQueryId;
            xmlStatments.Remove(key);
            xmlStatments.Add(newKey, statmentInfo);
        }

        public void Clear()
        {
            xmlStatments.Clear();
        }
    }
}