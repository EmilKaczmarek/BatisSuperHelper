using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VSIXProject5.Indexers.Models;
namespace VSIXProject5.Indexers
{
    public sealed class Indexer
    {
        static Indexer() { }
        private Indexer() { }
        private static readonly Indexer _instance = new Indexer();
        public static Indexer Instance
        {
            get
            {
                return _instance;
            }
        }

        private Dictionary<IndexerKey, XmlIndexerResult> xmlStatments = new Dictionary<IndexerKey, XmlIndexerResult>();
        //Since lookup is immutable, dictonary is better idea
        private Dictionary<IndexerKey, List<CSharpIndexerResult>> codeStatments = new Dictionary<IndexerKey, List<CSharpIndexerResult>>();


        public event EventHandler<BaseIndexer> OnIndexerBuildStared;
        public event EventHandler<BaseIndexer> OnIndexerBuildCompleted;
        public event EventHandler<BaseIndexer> OnIndexChanged;

        #region Get/Retreive Methods
        public Tuple<IndexerKey, XmlIndexerResult, List<CSharpIndexerResult>> this[IndexerKey key]
        {
            get
            {
                return Tuple.Create(key, xmlStatments[key], codeStatments[key]);
            }
        }
        public Tuple<XmlIndexerResult, List<CSharpIndexerResult>> Get(IndexerKey key)
        {
            return Tuple.Create(xmlStatments[key], codeStatments[key]);
        }

        public Dictionary<IndexerKey, List<CSharpIndexerResult>> GetCodeStatmentsDictonary()
        {
            return codeStatments;
        }
        public Dictionary<IndexerKey, XmlIndexerResult> GetXmlStatmentsDictonary()
        {
            return xmlStatments;
        }
        public List<IndexerKey> GetCodeKeysByQueryId(string queryId)
        {
            return codeStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }
        public List<IndexerKey> GetXmlKeysByQueryId(string queryId)
        {
            return xmlStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }
        public List<CSharpIndexerResult> GetCodeStatmentOrNull(IndexerKey key)
        {
            if (codeStatments.ContainsKey(key))
            {
                return codeStatments[key];
            }
            return null;
        }
        public XmlIndexerResult GetXmlStatmentOrNull(IndexerKey key)
        {
            if (xmlStatments.ContainsKey(key))
            {
                return xmlStatments[key];
            }
            return null;
        }
        #endregion
        #region Clear methods
        public void ClearAll()
        {
            ClearCodeStatments();
            ClearXmlStatments();
        }
        public void ClearXmlStatments()
        {
            xmlStatments.Clear();
        }
        public void ClearCodeStatments()
        {
            codeStatments.Clear();
        }
        #endregion
        #region Count Methods
        public int CodeStatmentsCount()
        {
            return codeStatments.SelectMany(e => e.Value).Count();
        }
        public int XmlStatmentsCount()
        {
            return xmlStatments.Count();
        }
        public int DistinctStatmentsCount()
        {
            var queriesList = new List<string>();
            var distinctCodeQueryId = codeStatments
                .SelectMany(e => e.Value)
                .GroupBy(e => e.QueryId)
                .Select(e => e.First().QueryId);
            var distinctXmlQueryId = xmlStatments.Select(e => e.Value.QueryId);
            queriesList.AddRange(distinctCodeQueryId);
            queriesList.AddRange(distinctXmlQueryId);
            return queriesList.Distinct().Count();
        }
        #endregion
        public XmlIndexerResult GetXmlStatment(IndexerKey key)
        {
            return xmlStatments[key];
        }
        public List<CSharpIndexerResult> GetCodeStatments(IndexerKey key)
        {
            return codeStatments[key];
        }

        public void Build<T>(List<T> values) where T : BaseIndexerValue
        {
            if (!values.Any())
                return;

             if (typeof(T) == typeof(XmlIndexerResult))
            {
                foreach (var value in values)
                {
                    Add(value as XmlIndexerResult);
                }
            }
            else
            {
                foreach (var value in values)
                {
                    Add(value as CSharpIndexerResult);
                }
            }
        }
        public void Add(XmlIndexerResult value)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = value.QueryId,
                VsProjectName = value.QueryVsProjectName
            };
            if (xmlStatments.ContainsKey(key))
            {
                //throw new ArgumentException("Element with same key already exist in xml statments dictonary");
            }
            else
            {
                xmlStatments.Add(key, value);
            }
        }

        public void Add(CSharpIndexerResult value)
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
        public List<XmlIndexerResult> GetAllXmlFileStatments(string fileName)
        {
            return xmlStatments.Values.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
        public List<List<CSharpIndexerResult>> GetAllCodeFileStatments(string fileName)
        {
            return codeStatments.Values
                .Select(e => e.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList())
                .Where(x => x.Count != 0)
                .ToList();
        }
        public void RemoveXmlStatment(XmlIndexerResult item)
        {
            xmlStatments.Remove(new IndexerKey { StatmentName = item.QueryId, VsProjectName = item.QueryFileName });
        }
        public void RemoveCodeStatment(CSharpIndexerResult item)
        {
            IndexerKey key = new IndexerKey
            {
                StatmentName = item.QueryId,
                VsProjectName = item.QueryFileName
            };
            var codeStatmentsForKey = codeStatments[key];
            codeStatmentsForKey.Remove(item);
            codeStatments[key] = codeStatmentsForKey;
        }
        public void RemoveSingleItem<T>(T item) where T : BaseIndexerValue
        {
            if (typeof(T) == typeof(XmlIndexerResult))
            {
                RemoveXmlStatment(item as XmlIndexerResult);
            }
            else if (typeof(T) == typeof(CSharpIndexerResult))
            {
                RemoveCodeStatment(item as CSharpIndexerResult);
            }
        }
        public void RemoveCodeStatmentsForDocumentId(DocumentId documentId)
        {
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
        public void RemoveCodeStatmentsForFile(string filePath)
        {
            codeStatments.Values.Select(x => x);
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
        public void RemoveXmlStatmentsForFile(string filePath)
        {
            var statmentsToRemove = xmlStatments.Values.Where(e => e.QueryFilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase));
            foreach (var statment in statmentsToRemove.ToList())
            {
                xmlStatments.Remove(new IndexerKey { StatmentName = statment.QueryId, VsProjectName = statment.QueryVsProjectName });
                Debug.WriteLine($"Removed: {statment.QueryId}, at: {statment.QueryVsProjectName}");
            }
        }

        public void RemoveStatmentsForFile(string fileName, bool codeStatments)
        {
            if (codeStatments)
            {
                RemoveCodeStatmentsForFile(fileName);
            }
            else
            {
                RemoveXmlStatmentsForFile(fileName);
            }
        }
        public void UpdateCodeStatmentForFile(List<CSharpIndexerResult> results)
        {
            if (!results.Any())
                return;
            RemoveCodeStatmentsForFile(results.First().QueryFilePath);
            Build(results);
        }
        public void UpdateXmlStatmentForFile(List<XmlIndexerResult> results)
        {
            if (!results.Any())
                return;
            RemoveXmlStatmentsForFile(results.First().QueryFilePath);
            Build(results);
        }
        public void UpdateStatmentsForFile<T>(List<T> results) where T : BaseIndexerValue
        {
            if (typeof(T) == typeof(CSharpIndexerResult))
            {
                UpdateCodeStatmentForFile(results as List<CSharpIndexerResult>);
            }
            else
            {
                UpdateXmlStatmentForFile(results as List<XmlIndexerResult>);
            }
        }
        public void RenameXmlStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileXmlStatments = xmlStatments.Where(e => e.Value.QueryFileName.Equals(oldFileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
            foreach (var xmlFileStatment in oldFileXmlStatments)
            {
                xmlStatments[xmlFileStatment.Key].QueryFileName = newFileName;
                xmlStatments[xmlFileStatment.Key].QueryFilePath = xmlStatments[xmlFileStatment.Key].QueryFilePath.Replace(oldFileName, newFileName);
            }
        }
        public void RenameCodeStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileCodeStatments = codeStatments.Values.SelectMany(e => e.Where(x => x.QueryFileName.Equals(oldFileName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            foreach (var oldFileCodeStatment in oldFileCodeStatments)
            {
                oldFileCodeStatment.QueryFileName = newFileName;
                oldFileCodeStatment.QueryFilePath = oldFileCodeStatment.QueryFilePath.Replace(oldFileName, newFileName);
            }
        }
        public void RenameStatmentsFile(string oldFileName, string newFileName)
        {
            if (Path.GetExtension(oldFileName).Equals(".cs"))
            {
                RenameCodeStatmentsForFile(oldFileName, newFileName);
            }
            else if (Path.GetExtension(oldFileName).Equals(".xml"))
            {
                RenameXmlStatmentsForFile(oldFileName, newFileName);
            }
        }
        public void RenameXmlQuery(IndexerKey key, string newQueryId)
        {
            var statmentInfo = xmlStatments[key];
            var newKey = new IndexerKey { StatmentName = newQueryId, VsProjectName = key.VsProjectName };
            statmentInfo.QueryId = newQueryId;
            xmlStatments.Remove(key);
            xmlStatments.Add(newKey, statmentInfo);
        }
        public void RenameCodeQueries(IndexerKey key, string newQueryId)
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
    }

}

