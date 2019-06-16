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
    public class StatementProvider : IQueryProvider<IndexerKey, Statement>
    {
        private readonly Dictionary<IndexerKey, Statement> _xmlStatments = new Dictionary<IndexerKey, Statement>();

        public Statement GetValue(IndexerKey key)
        {
            return _xmlStatments[key];
        }

        public Statement GetValueOrNull(IndexerKey key)
        {
            if (_xmlStatments.ContainsKey(key))
            {
                return GetValue(key);
            }
            return null;
        }

        public void Add(IndexerKey key, Statement value)
        {          
            if (!_xmlStatments.ContainsKey(key))
            {
                _xmlStatments.Add(key, value);
            }
        }

        public void AddWithoutKey(Statement value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.QueryId,
                VsProjectName = value.QueryVsProjectName,
                StatmentFullyQualifiedName = value.FullyQualifiedQuery
            };

            Add(key, value);
        }

        public void AddMultipleWithoutKey(List<Statement> values)
        {
            if (!values.Any())
                return;

            foreach (var value in values)
            {
                AddWithoutKey(value);
            }
        }

        public void AddMultiple(List<KeyValuePair<IndexerKey, Statement>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public List<Statement> GetAllStatmentsByFileName(string fileName)
        {
            return _xmlStatments.Values.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        private List<IndexerKey> GetKeysByQueryId(string queryId, bool useNamespace)
        {
            return _xmlStatments
               .Where(e => e.Key.StatmentName.Equals(queryId))
               .Where(e => e.Value.Config.Settings.UseStatementNamespaces == useNamespace)
               .Select(e => e.Key)
               .ToList();
        }

        private List<IndexerKey> GetKeysByFullyQualifiedName(string queryId, bool useNamespace)
        {
            return _xmlStatments
                .Where(e => e.Key.StatmentFullyQualifiedName.Equals(queryId))
                .Where(e => e.Value.Config.Settings.UseStatementNamespaces == useNamespace)
                .Select(e => e.Key)
                .ToList();
        }

        public List<IndexerKey> GetKeys(string queryId, bool useNamespace)
        {
            return useNamespace? GetKeysByFullyQualifiedName(queryId, useNamespace): GetKeysByQueryId(queryId, useNamespace);
        }

        public void RemoveStatmentByValue(Statement value)
        {
            _xmlStatments.Remove(new IndexerKey { StatmentName = value.QueryId, VsProjectName = value.QueryFileName, StatmentFullyQualifiedName = value.FullyQualifiedQuery });
        }

        public void RemoveStatmentsForFilePath(string filePath)
        {
            var statmentsToRemove = _xmlStatments.Where(e => e.Value.QueryFilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase));
            foreach (var statment in statmentsToRemove.ToList())
            {
                _xmlStatments.Remove(statment.Key);
            }
        }

        public void RemoveStatmentsByDefinedObject(object obj)
        {
            RemoveStatmentsForFilePath(obj as string);
        }

        public void UpdateStatmentsForFile(List<KeyValuePair<IndexerKey, Statement>> keyValuePairs)
        {
            if (!keyValuePairs.Any())
                return;
            RemoveStatmentsForFilePath(keyValuePairs.First().Value.QueryFilePath);
            AddMultiple(keyValuePairs);
        }

        public void UpdateStatmentForFileWihoutKey(List<Statement> values)
        {
            if (!values.Any())
                return;
            RemoveStatmentsForFilePath(values.First().QueryFilePath);
            AddMultipleWithoutKey(values);
        }

        public void RenameStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileXmlStatments = _xmlStatments.Where(e => e.Value.QueryFileName.Equals(oldFileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
            foreach (var xmlFileStatment in oldFileXmlStatments)
            {
                _xmlStatments[xmlFileStatment.Key].QueryFileName = newFileName;
                _xmlStatments[xmlFileStatment.Key].QueryFilePath = _xmlStatments[xmlFileStatment.Key].QueryFilePath.Replace(oldFileName, newFileName);
            }
        }

        public void RenameQuery(IndexerKey key, string newQueryId)
        {
            var statmentInfo = _xmlStatments[key];
            var newKey = new IndexerKey {
                StatmentName = MapNamespaceHelper.GetQueryWithoutNamespace(newQueryId),
                VsProjectName = key.VsProjectName,
                StatmentFullyQualifiedName = newQueryId
            };

            statmentInfo.QueryId = newQueryId;
            _xmlStatments.Remove(key);
            _xmlStatments.Add(newKey, statmentInfo);
        }

        public void Clear()
        {
            _xmlStatments.Clear();
        }

        public List<Statement> GetAllValues()
        {
            return _xmlStatments.Select(e => e.Value).ToList();
        }

        public List<IndexerKey> GetAllKeys()
        {
            return _xmlStatments.Keys.ToList();
        }
    }
}
