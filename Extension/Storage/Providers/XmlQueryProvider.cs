using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage.Interfaces;
using IBatisSuperHelper.Storage.Providers;

namespace IBatisSuperHelper.Storage.Domain
{
    public class XmlQueryProvider : IQueryProvider<IndexerKey, XmlQuery>
    {
        private Dictionary<IndexerKey, XmlQuery> xmlStatments = new Dictionary<IndexerKey, XmlQuery>();

        public XmlQuery GetValue(IndexerKey key)
        {
            return xmlStatments[key];
        }

        public XmlQuery GetValueOrNull(IndexerKey key)
        {
            if (xmlStatments.ContainsKey(key))
            {
                return GetValue(key);
            }
            return null;
        }

        public void Add(IndexerKey key, XmlQuery value)
        {          
            if (!xmlStatments.ContainsKey(key))
            {
                xmlStatments.Add(key, value);
            }
        }

        public void AddWithoutKey(XmlQuery value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.QueryId,
                VsProjectName = value.QueryVsProjectName,
                StatmentFullyQualifiedName = value.FullyQualifiedQuery
            };

            Add(key, value);
        }

        public void AddMultipleWithoutKey(List<XmlQuery> values)
        {
            if (!values.Any())
                return;

            foreach (var value in values)
            {
                AddWithoutKey(value as XmlQuery);
            }
        }

        public void AddMultiple(List<KeyValuePair<IndexerKey, XmlQuery>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public List<XmlQuery> GetAllStatmentsByFileName(string fileName)
        {
            return xmlStatments.Values.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        private List<IndexerKey> GetKeysByQueryId(string queryId)
        {
            return xmlStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }

        private List<IndexerKey> GetKeysByFullyQualifiedName(string queryId)
        {
            return xmlStatments.Keys.Where(e => e.StatmentFullyQualifiedName.Equals(queryId)).ToList();
        }

        public List<IndexerKey> GetKeysByQueryId(string queryId, bool useNamespace)
        {
            return useNamespace? GetKeysByFullyQualifiedName(queryId): GetKeysByQueryId(queryId);
        }

        public void RemoveStatmentByValue(XmlQuery value)
        {
            xmlStatments.Remove(new IndexerKey { StatmentName = value.QueryId, VsProjectName = value.QueryFileName, StatmentFullyQualifiedName = value.FullyQualifiedQuery });
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

        public void UpdateStatmentsForFile(List<KeyValuePair<IndexerKey, XmlQuery>> keyValuePairs)
        {
            if (!keyValuePairs.Any())
                return;
            RemoveStatmentsForFilePath(keyValuePairs.First().Value.QueryFilePath);
            AddMultiple(keyValuePairs);
        }

        public void UpdateStatmentForFileWihoutKey(List<XmlQuery> results)
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
            var newKey = new IndexerKey {
                StatmentName = MapNamespaceHelper.GetQueryWithoutNamespace(newQueryId),
                VsProjectName = key.VsProjectName,
                StatmentFullyQualifiedName = newQueryId
            };

            statmentInfo.QueryId = newQueryId;
            xmlStatments.Remove(key);
            xmlStatments.Add(newKey, statmentInfo);
        }

        public void Clear()
        {
            xmlStatments.Clear();
        }

        public List<XmlQuery> GetAllValues()
        {
            return xmlStatments.Select(e => e.Value).ToList();
        }

        public List<IndexerKey> GetAllKeys()
        {
            return xmlStatments.Keys.ToList();
        }
    }
}
