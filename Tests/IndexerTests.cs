//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using BatisSuperHelper.Indexers;
//using static BatisSuperHelper.Indexers.XmlIndexer;
//using System.Collections.Generic;

//namespace GoToQueryUnitTests
//{
//    [TestClass]
//    public class IndexerTests
//    {
//        [Fact]
//        public void ShouldAddXmlStatmentWithoutErrors()
//        {
//            var xmlIndexerResult = new XmlIndexerResult
//            {
//                QueryFileName = "TestMethod1.xml",
//                QueryFileRelativeVsPath = "/Project/TestMethod1.xml",
//                QueryId = "TestQuery1",
//                QueryLineNumber = 1,
//                QueryVsProjectName = "TestProject"
//            };
//            List<XmlIndexerResult> list = new List<XmlIndexerResult> { xmlIndexerResult};
//            Indexer.BuildFromXml(list);
//            var instanceCopy = Indexer.statmentInfo;
//            Assert.Equal(instanceCopy.Count, 1);
//            var key = IndexerKey.ConvertToKey(xmlIndexerResult.QueryId, xmlIndexerResult.QueryVsProjectName);
//            Assert.Equal(instanceCopy[key].XmlInfo.LineNumer, xmlIndexerResult.QueryLineNumber);
//        }
//        [Fact]
//        public void ShouldRemoveAllStatments()
//        {
//            Indexer.statmentInfo.Clear();
//            Assert.Equal(Indexer.statmentInfo.Count, 0);
//        }
//        [Fact]
//        public void ShouldRenameStatmentFromXmlWithoutErrors()
//        {
//            Indexer.statmentInfo.Clear();
//            var xmlIndexerResult = new XmlIndexerResult
//            {
//                QueryFileName = "TestMethod1.xml",
//                QueryFileRelativeVsPath = "/Project/TestMethod1.xml",
//                QueryId = "TestQuery1",
//                QueryLineNumber = 1,
//                QueryVsProjectName = "TestProject"
//            };
//            List<XmlIndexerResult> list = new List<XmlIndexerResult> { xmlIndexerResult };
//            Indexer.BuildFromXml(list);
//            Assert.Equal(Indexer.statmentInfo.Count, 1);
//            Indexer.UpdateStatmentFile("TestMethod1.xml", "TestMethodRenamed.xml", false);
//            Assert.Equal(Indexer.statmentInfo[IndexerKey.ConvertToKey("TestQuery1", "TestProject")].XmlInfo.StatmentFile, "TestMethodRenamed.xml");
//        }
//        [Fact]
//        public void ShouldRemoveStatmentFile()
//        {
//            Indexer.statmentInfo.Clear();
//            var xmlIndexerResult = new XmlIndexerResult
//            {
//                QueryFileName = "TestMethod1.xml",
//                QueryFileRelativeVsPath = "/Project/TestMethod1.xml",
//                QueryId = "TestQuery1",
//                QueryLineNumber = 1,
//                QueryVsProjectName = "TestProject"
//            };
//            List<XmlIndexerResult> list = new List<XmlIndexerResult> { xmlIndexerResult };
//            Indexer.BuildFromXml(list);
//            Assert.Equal(Indexer.statmentInfo.Count, 1);
//            Indexer.RemoveStatmentFile("TestMethod1.xml",false);
//            Assert.IsNull(Indexer.statmentInfo[IndexerKey.ConvertToKey("TestQuery1", "TestProject")].XmlInfo);
//        }
//        [Fact]
//        public void ShouldCombineXmlAndCodeStatments()
//        {
//            Indexer.statmentInfo.Clear();
//            var xmlIndexerResult = new XmlIndexerResult
//            {
//                QueryFileName = "TestMethod1.xml",
//                QueryFileRelativeVsPath = "/Project/TestMethod1.xml",
//                QueryId = "TestQuery1",
//                QueryLineNumber = 1,
//                QueryVsProjectName = "TestProject"
//            };
//            List<XmlIndexerResult> xmlList = new List<XmlIndexerResult> { xmlIndexerResult };
//            var codeIndexerResult = new CSharpIndexerResult
//            {
//                QueryFileName = "TestMethod1.cs",
//                QueryFileRelativeVsPath = "/Project/TestMethod1.cs",
//                QueryId = "TestQuery1",
//                QueryLineNumber = 2,
//                QueryVsProjectName = "TestProject"
//            };
//            List<XmlIndexerResult> codeList = new List<XmlIndexerResult> { xmlIndexerResult };

//        }
//    }
//}
