using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSIXProject5.HelpersAndExtensions;

namespace Tests
{
    [TestClass]
    public class MapNamespaceHelperTests
    {
        [TestMethod]
        public void ShouldCreateProperFullyQualifiedStatmentName()
        {
            var mapNamespace = "namespace";
            var queryId = "Select";
            Assert.AreEqual("namespace.Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [TestMethod]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithEmptyQueryId()
        {
            var mapNamespace = "namespace";
            var queryId = "";
            Assert.AreEqual("", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [TestMethod]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithEmptyNamespace()
        {
            var mapNamespace = "";
            var queryId = "Select";
            Assert.AreEqual("Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [TestMethod]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithNullQueryId()
        {
            string mapNamespace = "namespace";
            string queryId = null;
            Assert.AreEqual("", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [TestMethod]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithNullNamespace()
        {
            string mapNamespace = null;
            string queryId = "Select";
            Assert.AreEqual("Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [TestMethod]
        public void ShouldGetQueryWithoutNamespace()
        {
            string queryId = "namespace.Select";       
            Assert.AreEqual("Select", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }

        [TestMethod]
        public void ShouldGetQueryWithoutNamespaceForNullQueryId()
        {
            string queryId = null;
            Assert.AreEqual("", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }

        [TestMethod]
        public void ShouldGetQueryWithoutNamespaceForEmptyQueryId()
        {
            string queryId = "";
            Assert.AreEqual("", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }
    }
}
