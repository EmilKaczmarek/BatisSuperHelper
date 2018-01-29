//using EnvDTE;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VSIXProject5.Indexers;
//using static VSIXProject5.Indexers.XmlIndexer;

//namespace VSIXProject5.Indexers2
//{
//    public sealed class IndexerSingleton
//    {
//        public IndexerSingleton Instance { get; } = new IndexerSingleton();

//        public static Dictionary<IndexerKey, StatmentInfo> statmentInfo = new Dictionary<IndexerKey, StatmentInfo>();

//        static IndexerSingleton()
//        {

//        }
//        private IndexerSingleton()
//        {

//        }
//        public static void AddXmlStatment(StatmentInfo statmentInfo)
//        {
//            if (IndexerSingleton.statmentInfo.ContainsKey(statmentInfo.key))
//            {
//                IndexerSingleton.statmentInfo[statmentInfo.key].XmlInfo = statmentInfo.XmlInfo;
//                IndexerSingleton.statmentInfo[statmentInfo.key].CodeInfo = statmentInfo.CodeInfo;
//            }
//            else
//            {
//                IndexerSingleton.statmentInfo.Add(statmentInfo.key, statmentInfo);
//            }
//        }
//        public static void BuildFromXml(List<XmlIndexerResult> xmlIndexerResult)
//        {
//            foreach (var result in xmlIndexerResult)
//            {
//                IndexerKey key = new IndexerKey
//                {
//                    StatmentName = result.QueryId,
//                    VsProjectName = result.QueryVsProjectName,
//                    //StatmentFile = result.QueryFileName,
//                };
                
//                AddXmlStatment(new StatmentInfo
//                {
//                    key =key,
//                    XmlInfo =new XmlStatmentInfo
//                    {
//                        LineNumer = result.QueryLineNumber,
//                        StatmentFile = result.QueryFileName,
//                        RelativePath=result.QueryFileRelativeVsPath,
//                    }
//                });
//            }               
//        }

//        public static void UpdateXmlFileStatments(List<XmlIndexerResult> xmlIndexerResult, string fileName)
//        {
//            var st = statmentInfo;
//            var allcodeStatments = statmentInfo.Where(e => e.Value.CodeInfo != null).ToList();
//            var codeStatments = statmentInfo.Where(e => e.Value.CodeInfo != null && e.Value.XmlInfo != null && e.Value.XmlInfo.StatmentFile.ToLower() == fileName.ToLower()).ToList();
//            var fileStatments2 = statmentInfo.Where(e => e.Value.XmlInfo != null);
//            var fileStatments = fileStatments2.Where(e=>e.Value.XmlInfo.StatmentFile.ToLower() == fileName.ToLower());          
//            foreach (var value in fileStatments.ToList())
//            {
//                statmentInfo.Remove(value.Key);
//            }
//            foreach (var statment in xmlIndexerResult)
//            {
//                IndexerKey key = new IndexerKey
//                {
//                    StatmentName = statment.QueryId,
//                    VsProjectName = statment.QueryVsProjectName,
//                };
//                XmlStatmentInfo newXmlStatment = new XmlStatmentInfo
//                {
//                    LineNumer = statment.QueryLineNumber,
//                    StatmentFile = statment.QueryFileName,
//                    RelativePath = statment.QueryFileRelativeVsPath,
//                };
//                CodeStatmentInfo codeinfo = null;
//                var codeStatment = allcodeStatments.FirstOrDefault(e => e.Key.StatmentName == statment.QueryId);
//                allcodeStatments.Remove(codeStatment);
//                if (!codeStatment.Equals(new KeyValuePair<IndexerKey, StatmentInfo>()))//if code statment found:
//                {
//                    codeinfo = codeStatment.Value.CodeInfo;
//                }
//                AddXmlStatment(new StatmentInfo
//                {
//                    key = key,
//                    XmlInfo = newXmlStatment,
//                    CodeInfo = codeinfo,
//                });
//            }
//            foreach(var codeStatmentLeft in codeStatments)
//            {
//                statmentInfo.Add(codeStatmentLeft.Key, new StatmentInfo
//                {
//                    CodeInfo = codeStatmentLeft.Value.CodeInfo,
//                });
//            }
//        }
//        public static void UpdateCSharpFileStatment(List<CSharpIndexerResult> csIndexerResults, string fileName)
//        {
//            if(csIndexerResults.Any(e=>e.QueryId== "SelectQuery" || e.QueryId == "SelectQuer"))
//            {
//                var debgug = true;
//            }
//            var xmlStatments = statmentInfo.Where(e => 
//                e.Value.XmlInfo != null &&
//                e.Value.CodeInfo != null &&
//                e.Value.CodeInfo.StatmentFile.ToLower()==fileName.ToLower()
//                ).ToList();
//            var allXmlStatments = statmentInfo.Where(e => e.Value.XmlInfo != null).ToList();
//            var codeFileStatments = statmentInfo.Where(e => e.Value.CodeInfo!=null 
//                        && e.Value.CodeInfo.StatmentFile.ToLower() == fileName.ToLower()).ToList();
//            foreach(var fileCodeStatment in codeFileStatments)
//            {
//                statmentInfo.Remove(fileCodeStatment.Key);
//            }
//            foreach (var newFileCodeStatment in csIndexerResults)
//            {
//                IndexerKey key = new IndexerKey
//                {
//                    StatmentName = newFileCodeStatment.QueryId,
//                    VsProjectName = newFileCodeStatment.QueryVsProjectName,
//                };
//                CodeStatmentInfo codeInfo = new CodeStatmentInfo
//                {
//                    LineNumber = newFileCodeStatment.QueryLineNumber,
//                    StatmentFile = newFileCodeStatment.QueryFileName,
//                    RelativePath = newFileCodeStatment.QueryFileRelativeVsPath,
//                };
//                XmlStatmentInfo xmlStatment = null;
//                var existingXmlStatment = allXmlStatments.FirstOrDefault(e => e.Key.StatmentName == newFileCodeStatment.QueryId);
//                xmlStatments.Remove(existingXmlStatment);
//                if (!existingXmlStatment.Equals(new KeyValuePair<IndexerKey, StatmentInfo>()))//if code statment found:
//                {
//                    xmlStatment = existingXmlStatment.Value.XmlInfo;
//                }
//                if (statmentInfo.ContainsKey(key))
//                {
//                    statmentInfo[key].CodeInfo = codeInfo;
//                    statmentInfo[key].XmlInfo = xmlStatment;
//                }
//                else
//                {
//                    statmentInfo.Add(key, new StatmentInfo
//                    {
//                        XmlInfo = xmlStatment,
//                        CodeInfo = codeInfo,
//                    });
//                }
//            }
//            foreach (var xmlStatmentLeftover in xmlStatments)
//            {
//                statmentInfo.Add(xmlStatmentLeftover.Key, new StatmentInfo
//                {
//                    XmlInfo = xmlStatmentLeftover.Value.XmlInfo
//                });
//            }
//        }
//        public static void AppendCSharpFileStatment(List<CSharpIndexerResult> csIndexerResults)
//        {
//            foreach(var statment in csIndexerResults)
//            {
//                var value=statmentInfo.FirstOrDefault(e => e.Key.StatmentName == statment.QueryId);
//                if (value.Key != null) {
//                    value.Value.CodeInfo = new CodeStatmentInfo
//                    {
//                        LineNumber = statment.QueryLineNumber,
//                        StatmentFile = statment.QueryFileName,
//                        RelativePath = statment.QueryFileRelativeVsPath,
//                    };
//                }
//                else
//                {
//                    var relativePathTest = statment.QueryFileRelativeVsPath;
//                    statmentInfo.Add(
//                        new IndexerKey
//                        {
//                            StatmentName=statment.QueryId,
//                            VsProjectName= statment.QueryVsProjectName,
//                        },
//                        new StatmentInfo
//                        {
//                            CodeInfo = new CodeStatmentInfo
//                            {
//                                LineNumber=statment.QueryLineNumber,
//                                StatmentFile=statment.QueryFileName,
//                                RelativePath=statment.QueryFileRelativeVsPath,
//                            }
//                        });
//                }
//            }
//        }
//        private static void UpdateXmlStatmentFile(string newName, List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments)
//        {
//            foreach (var statment in oldFileStatments)
//            {
//                statmentInfo[statment.Key].CodeInfo.StatmentFile = newName;
//            }
//        }
//        private static void UpdateCSharpStatmentFile(string newName, List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments)
//        {
//            foreach(var statment in oldFileStatments)
//            {
//                statmentInfo[statment.Key].CodeInfo.StatmentFile = newName;
//            }
//        }
//        public static void UpdateStatmentFile(string oldName, string newName, bool isCSharpFile)
//        {
//            List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments;
//            if (isCSharpFile)
//            {
//                oldFileStatments = statmentInfo.Where(e => e.Value.CodeInfo != null && e.Value.CodeInfo.StatmentFile == oldName).ToList();
//                UpdateCSharpStatmentFile(newName, oldFileStatments);
//            }
//            else
//            {
//                oldFileStatments = statmentInfo.Where(e => e.Value.XmlInfo != null && e.Value.XmlInfo.StatmentFile == oldName).ToList();
//                UpdateXmlStatmentFile(newName, oldFileStatments);
//            }
//        }
//        private static void RemoveXmlStatmentFile(List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments)
//        {
//            foreach(var statment in oldFileStatments)
//            {
//                statmentInfo[statment.Key].XmlInfo = null;
//            }
//        }
//        private static void RemoveCSharpStatmentFile(List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments)
//        {
//            foreach (var statment in oldFileStatments)
//            {
//                statmentInfo[statment.Key].CodeInfo = null;
//            }
//        }
//        public static void RemoveStatmentFile(string fileName, bool isCSharpFile)
//        {
//            List<KeyValuePair<IndexerKey, StatmentInfo>> oldFileStatments;
//            if (isCSharpFile)
//            {
//                oldFileStatments = statmentInfo.Where(e => e.Value.CodeInfo != null && e.Value.CodeInfo.StatmentFile == fileName).ToList();

//            }
//            else
//            {
//                oldFileStatments = statmentInfo.Where(e => e.Value.CodeInfo != null && e.Value.CodeInfo.StatmentFile == fileName).ToList();

//            }
//        }




//    }
//}

