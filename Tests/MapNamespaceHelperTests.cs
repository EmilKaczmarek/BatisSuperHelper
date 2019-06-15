using System;
using BatisSuperHelper.HelpersAndExtensions;
using Xunit;

namespace Tests
{
    public class MapNamespaceHelperTests
    {
        [Fact]
        public void ShouldCreateProperFullyQualifiedStatmentName()
        {
            var mapNamespace = "namespace";
            var queryId = "Select";
            Assert.Equal("namespace.Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [Fact]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithEmptyQueryId()
        {
            var mapNamespace = "namespace";
            var queryId = "";
            Assert.Equal("", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [Fact]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithEmptyNamespace()
        {
            var mapNamespace = "";
            var queryId = "Select";
            Assert.Equal("Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [Fact]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithNullQueryId()
        {
            string mapNamespace = "namespace";
            string queryId = null;
            Assert.Equal("", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [Fact]
        public void ShouldCreateProperFullyQualifiedStatmentNameWithNullNamespace()
        {
            string mapNamespace = null;
            string queryId = "Select";
            Assert.Equal("Select", MapNamespaceHelper.CreateFullQueryString(mapNamespace, queryId));
        }

        [Fact]
        public void ShouldGetQueryWithoutNamespace()
        {
            string queryId = "namespace.Select";       
            Assert.Equal("Select", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }

        [Fact]
        public void ShouldGetQueryWithoutNamespaceForNullQueryId()
        {
            string queryId = null;
            Assert.Equal("", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }

        [Fact]
        public void ShouldGetQueryWithoutNamespaceForEmptyQueryId()
        {
            string queryId = "";
            Assert.Equal("", MapNamespaceHelper.GetQueryWithoutNamespace(queryId));
        }
    }
}
