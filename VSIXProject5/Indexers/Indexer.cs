using EnvDTE;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers.Models;
using static VSIXProject5.Indexers.XmlIndexer;

namespace VSIXProject5.Indexers
{
    public class Indexer
    {
        private static Dictionary<IndexerKey, XmlIndexerResult> xmlStatments = new Dictionary<IndexerKey, XmlIndexerResult>();
        //Since lookup is immutable, dictonary is better idea
        private static Dictionary<IndexerKey, List<CSharpIndexerResult>> codeStatments = new Dictionary<IndexerKey, List<CSharpIndexerResult>>();
        
        #region Get/Retreive Methods
        public Tuple<IndexerKey, XmlIndexerResult, List<CSharpIndexerResult>> this[IndexerKey key]
        {
            get
            {
                return Tuple.Create(key, xmlStatments[key], codeStatments[key]);
            }
        }
        public static Tuple<XmlIndexerResult, List<CSharpIndexerResult>> Get(IndexerKey key)
        {
            return Tuple.Create(xmlStatments[key], codeStatments[key]);
        }

        public static Dictionary<IndexerKey, List<CSharpIndexerResult>> GetCodeStatmentsDictonary()
        {
            return codeStatments;
        }
        public static Dictionary<IndexerKey, XmlIndexerResult> GetXmlStatmentsDictonary()
        {
            return xmlStatments;
        }
        public static List<IndexerKey> GetCodeKeysByQueryId(string queryId)
        {
            return codeStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }
        public static List<IndexerKey> GetXmlKeysByQueryId(string queryId)
        {
            return xmlStatments.Keys.Where(e => e.StatmentName.Equals(queryId)).ToList();
        }
        public static List<CSharpIndexerResult> GetCodeStatmentOrNull(IndexerKey key)
        {
            if (codeStatments.ContainsKey(key))
            {
                return codeStatments[key];
            }
            return null;
        }
        public static XmlIndexerResult GetXmlStatmentOrNull(IndexerKey key)
        {
            if (xmlStatments.ContainsKey(key))
            {
                return xmlStatments[key];
            }
            return null;
        }
        #endregion
        #region Clear methods
        public static void ClearAll()
        {
            ClearCodeStatments();
            ClearXmlStatments();
        }
        public static void ClearXmlStatments()
        {
            xmlStatments.Clear();
        }
        public static void ClearCodeStatments()
        {
            codeStatments.Clear();
        }
        #endregion
        #region Count Methods
        public static int CodeStatmentsCount()
        {
            return codeStatments.SelectMany(e => e.Value).Count();
        }
        public static int XmlStatmentsCount()
        {
            return xmlStatments.Count();
        }
        public static int DistinctStatmentsCount()
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
        public static XmlIndexerResult GetXmlStatment(IndexerKey key)
        {
            return xmlStatments[key];
        }
        public static List<CSharpIndexerResult> GetCodeStatments(IndexerKey key)
        {
            return codeStatments[key];
        }

        public static void Build<T>(List<T> values) where T : BaseIndexerValue
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
        public static void Add(XmlIndexerResult value)
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

        public static void Add(CSharpIndexerResult value)
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
        public static List<XmlIndexerResult> GetAllXmlFileStatments(string fileName)
        {
            return xmlStatments.Values.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
        public static List<List<CSharpIndexerResult>> GetAllCodeFileStatments(string fileName)
        {
            return codeStatments.Values
                .Select(e => e.Where(x => x.QueryFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToList())
                .Where(x => x.Count != 0)
                .ToList();
        }
        public static void RemoveXmlStatment(XmlIndexerResult item)
        {
            xmlStatments.Remove(new IndexerKey { StatmentName = item.QueryId, VsProjectName = item.QueryFileName });
        }
        public static void RemoveCodeStatment(CSharpIndexerResult item)
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
        public static void RemoveSingleItem<T>(T item) where T : BaseIndexerValue
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
        public static void RemoveCodeStatmentsForDocumentId(DocumentId documentId)
        {
            codeStatments.Values.Select(x => x);
            List<int> hashCodes = codeStatments.Values.SelectMany(
                x => x
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
        public static void RemoveCodeStatmentsForFile(string filePath)
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
        public static void RemoveXmlStatmentsForFile(string filePath)
        {
            var statmentsToRemove = xmlStatments.Values.Where(e => e.QueryFilePath.Equals(filePath,StringComparison.CurrentCultureIgnoreCase));
            foreach (var statment in statmentsToRemove.ToList())
            {
                xmlStatments.Remove(new IndexerKey { StatmentName = statment.QueryId, VsProjectName = statment.QueryVsProjectName });
                Debug.WriteLine($"Removed: {statment.QueryId}, at: {statment.QueryVsProjectName}");
            }
        }

        public static void RemoveStatmentsForFile(string fileName, bool codeStatments)
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
        public static void UpdateCodeStatmentForFile(List<CSharpIndexerResult> results)
        {
            if (!results.Any())
                return;
            RemoveCodeStatmentsForFile(results.First().QueryFilePath);
            Build(results);
        }
        public static void UpdateXmlStatmentForFile(List<XmlIndexerResult> results)
        {
            if (!results.Any())
                return;
            RemoveXmlStatmentsForFile(results.First().QueryFilePath);
            Build(results);
        }
        public static void UpdateStatmentsForFile<T>(List<T> results) where T : BaseIndexerValue
        {
            if(typeof(T) == typeof(CSharpIndexerResult))
            {
                UpdateCodeStatmentForFile(results as List<CSharpIndexerResult>);
            }
            else
            {
                UpdateXmlStatmentForFile(results as List<XmlIndexerResult>);
            }
        }
        public static void RenameXmlStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileXmlStatments = xmlStatments.Where(e => e.Value.QueryFileName.Equals(oldFileName,StringComparison.CurrentCultureIgnoreCase)).ToList();
            foreach (var xmlFileStatment in oldFileXmlStatments)
            {
                xmlStatments[xmlFileStatment.Key].QueryFileName = newFileName;
                xmlStatments[xmlFileStatment.Key].QueryFilePath = xmlStatments[xmlFileStatment.Key].QueryFilePath.Replace(oldFileName, newFileName);
            }
        }
        public static void RenameCodeStatmentsForFile(string oldFileName, string newFileName)
        {
            var oldFileCodeStatments = codeStatments.Values.SelectMany(e => e.Where(x => x.QueryFileName.Equals(oldFileName,StringComparison.CurrentCultureIgnoreCase))).ToList();
            foreach(var oldFileCodeStatment in oldFileCodeStatments)
            {
                oldFileCodeStatment.QueryFileName = newFileName;
                oldFileCodeStatment.QueryFilePath = oldFileCodeStatment.QueryFilePath.Replace(oldFileName, newFileName);
            }
        }
        public static void RenameStatmentsFile(string oldFileName, string newFileName)
        {
            if (Path.GetExtension(oldFileName).Equals(".cs"))
            {
                RenameCodeStatmentsForFile(oldFileName, newFileName);
            }
            else if(Path.GetExtension(oldFileName).Equals(".xml"))
            {
                RenameXmlStatmentsForFile(oldFileName, newFileName);
            }
        }
    }

}

